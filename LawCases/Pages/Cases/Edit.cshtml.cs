using LawCases.Models;
using LawCases.Models.ViewModels;
using LawCases.Models.ViewModels.LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LawCases.Pages.Cases
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(LawCases.Data.ApplicationDbContext context,
                        UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public CaseViewModel Case { get; set; }

        public SelectList ClientSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userIdString = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Get the case with related data
            var caseEntity = await _context.Cases
                .Include(c => c.CaseTransactions)
                .Include(c => c.CasePayment)
                .Include(c => c.CaseDates)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CaseId == id && c.UserId == userId && !c.IsDeleted);

            if (caseEntity == null)
            {
                return NotFound();
            }

            // Get the latest payment (optional: use FirstOrDefault for earliest)
            var payment = caseEntity.CasePayment?.OrderByDescending(p => p.CreatedOn).FirstOrDefault();

            // Get the latest case date
            var latestCaseDate = caseEntity.CaseDates?.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault();

            // Get the first document (assuming one document per case for now)
            var document = caseEntity.Documents?.FirstOrDefault();

            // Map to view model
            Case = new CaseViewModel
            {
                CaseId = caseEntity.CaseId,
                Title = caseEntity.Title,
                CourtName = caseEntity.CourtName,
                CaseCode = caseEntity.CaseCode,
                FilingNumber = caseEntity.FilingNumber,
                FIRNumber = caseEntity.FIRNumber,
                CaseNumber = caseEntity.CaseNumber,
                StartDate = caseEntity.StartDate,
                PoliceStation = caseEntity.PoliceStation,
                District = caseEntity.District,
                FIRYear = caseEntity.FIRYear,
                Category = caseEntity.Category,
                JudgeName = caseEntity.JudgeName,
                ClientId = caseEntity.ClientId,
                TotalAmount = payment?.TotalAmount,
                InitialAmount = payment?.InitialAmount,
                Comment = latestCaseDate?.Comment,
                JudgeRemarks = latestCaseDate?.JudgeRemarks,
                ClientRemarks = latestCaseDate?.ClientRemarks,
                Conclusion = latestCaseDate?.Conclusion,
                FileName = document?.FileName,
                FileType = document?.FileType
            };

            // Load clients for dropdown
            var clients = await _context.Clients
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName })
                .ToListAsync();

            ClientSelectList = new SelectList(clients, "ClientId", "FullName", Case.ClientId);

            // Load categories for dropdown
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Name, c.Description })
                .ToListAsync();

            CategorySelectList = new SelectList(categories, "Name", "Name", Case.Category);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            var userIdString2 = _userManager.GetUserId(User);
            if (!int.TryParse(userIdString2, out int userId2))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var existingCase = await _context.Cases
                        .Include(c => c.CaseTransactions)
                        .Include(c => c.CasePayment)
                        .Include(c => c.CaseDates)
                        .Include(c => c.Documents)
                        .FirstOrDefaultAsync(c => c.CaseId == Case.CaseId && c.UserId == userId2 && !c.IsDeleted);

                    if (existingCase == null)
                    {
                        return NotFound();
                    }

                    // Update Case fields
                    existingCase.Title = Case.Title;
                    existingCase.CourtName = Case.CourtName;
                    existingCase.CaseCode = Case.CaseCode;
                    existingCase.FilingNumber = Case.FilingNumber;
                    existingCase.FIRNumber = Case.FIRNumber;
                    existingCase.CaseNumber = Case.CaseNumber;
                    existingCase.StartDate = Case.StartDate;
                    existingCase.Status = "Open";
                    existingCase.PoliceStation = Case.PoliceStation;
                    existingCase.District = Case.District;
                    existingCase.FIRYear = Case.FIRYear;
                    existingCase.Category = Case.Category;
                    existingCase.JudgeName = Case.JudgeName;
                    existingCase.ClientId = Case.ClientId;
                    existingCase.ModifiedOn = DateTime.UtcNow;

                    _context.Cases.Update(existingCase);

                    // Handle CasePayment (1-to-many: use latest or first)
                    var existingPayment = existingCase.CasePayment.OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                    var firstPayment = existingCase.CaseTransactions.OrderBy(cd => cd.CreatedOn).FirstOrDefault();

                    if (existingPayment != null)
                    {
                        existingPayment.TotalAmount = (int?)(Case.TotalAmount ?? 0);
                        existingPayment.InitialAmount = (int?)(Case.InitialAmount ?? 0);
                        existingPayment.ModifiedOn = DateTime.UtcNow;
                        _context.CasePayments.Update(existingPayment);
                    }

                    else if (Case.TotalAmount.HasValue || Case.InitialAmount.HasValue)
                    {
                        var newPayment = new CasePayment
                        {
                            CaseId = existingCase.CaseId,
                            TotalAmount = (int?)(Case.TotalAmount ?? 0),
                            InitialAmount = (int?)(Case.InitialAmount ?? 0),
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        var paymentTransaction = new CaseTransaction
                        {
                            CaseId = existingCase.CaseId,
                            Amount = (int)Case.InitialAmount,
                            CreatedOn = DateTime.UtcNow
                        };
                        await _context.CasePayments.AddAsync(newPayment);
                        await _context.CaseTransactions.AddAsync(paymentTransaction);
                    }

                    if (firstPayment != null)
                    {
                        firstPayment.Amount = (int)Case.InitialAmount;
                        firstPayment.ModifiedOn = DateTime.UtcNow;
                        _context.CaseTransactions.Update(firstPayment);
                    }


                    else if (Case.TotalAmount.HasValue || Case.InitialAmount.HasValue)
                    {
                        var paymentTransaction = new CaseTransaction
                        {
                            CaseId = existingCase.CaseId,
                            Amount = (int)Case.InitialAmount,
                            CreatedOn = DateTime.UtcNow
                        };
                        await _context.CaseTransactions.AddAsync(paymentTransaction);
                    }


                    // Handle CaseDate
                    var latestCaseDate = existingCase.CaseDates?.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault();

         

                    // Handle Document
                    if (!string.IsNullOrEmpty(Case.FileName))
                    {
                        var existingDocument = existingCase.Documents?.FirstOrDefault();
                        if (existingDocument != null)
                        {
                            existingDocument.FileName = Case.FileName;
                            existingDocument.FileType = Case.FileType;
                            existingDocument.ModifiedOn = DateTime.UtcNow;
                            _context.Documents.Update(existingDocument);
                        }
                        else
                        {
                            var document = new Document
                            {
                                FileName = Case.FileName,
                                FileType = Case.FileType,
                                CaseId = existingCase.CaseId,
                                ClientId = Case.ClientId,
                                CreatedOn = DateTime.UtcNow,
                                IsDeleted = false
                            };
                            await _context.Documents.AddAsync(document);
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return RedirectToPage("/cases/list");
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }




    }
}