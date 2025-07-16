using LawCases.Models;
using LawCases.Models.ViewModels;
using LawCases.Models.ViewModels.LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
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
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Get the case with related data
            var caseEntity = await _context.Cases
                .Include(c => c.CasePayment)
                .Include(c => c.CaseDates)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.CaseId == id && c.UserId == userId && !c.IsDeleted);

            if (caseEntity == null)
            {
                return NotFound();
            }

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
                Status = caseEntity.Status,
                CloseType = caseEntity.CloseType,
                PoliceStation = caseEntity.PoliceStation,
                District = caseEntity.District,
                FIRYear = caseEntity.FIRYear,
                Category = caseEntity.Category,
                JudgeName = caseEntity.JudgeName,
                ClientId = caseEntity.ClientId,
                TotalAmount = caseEntity.CasePayment?.TotalAmount,
                InitialAmount = caseEntity.CasePayment?.InitialAmount,
                PreviousDate = latestCaseDate?.PreviousDate,
                NextDate = latestCaseDate?.NextDate,
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
            if (!ModelState.IsValid)
            {
                // Reload dropdown data
                var userIdString = _userManager.GetUserId(User);
                int userId = int.Parse(userIdString);

                var clients = await _context.Clients
                    .Where(c => c.UserId == userId && !c.IsDeleted)
                    .Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName })
                    .ToListAsync();

                ClientSelectList = new SelectList(clients, "ClientId", "FullName", Case.ClientId);

                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .Select(c => new { c.Name, c.Description })
                    .ToListAsync();

                CategorySelectList = new SelectList(categories, "Name", "Name", Case.Category);

                return Page();
            }

            var userIdString2 = _userManager.GetUserId(User);
            int userId2 = 0;
            bool isParsed2 = int.TryParse(userIdString2, out userId2);

            if (string.IsNullOrEmpty(userIdString2) || !isParsed2)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Get existing case
                    var existingCase = await _context.Cases
                        .Include(c => c.CasePayment)
                        .Include(c => c.CaseDates)
                        .Include(c => c.Documents)
                        .FirstOrDefaultAsync(c => c.CaseId == Case.CaseId && c.UserId == userId2 && !c.IsDeleted);

                    if (existingCase == null)
                    {
                        return NotFound();
                    }

                    // Update Case
                    existingCase.Title = Case.Title;
                    existingCase.CourtName = Case.CourtName;
                    existingCase.CaseCode = Case.CaseCode;
                    existingCase.FilingNumber = Case.FilingNumber;
                    existingCase.FIRNumber = Case.FIRNumber;
                    existingCase.CaseNumber = Case.CaseNumber;
                    existingCase.StartDate = Case.StartDate;
                    existingCase.Status = Case.Status;
                    existingCase.CloseType = Case.CloseType;
                    existingCase.PoliceStation = Case.PoliceStation;
                    existingCase.District = Case.District;
                    existingCase.FIRYear = Case.FIRYear;
                    existingCase.Category = Case.Category;
                    existingCase.JudgeName = Case.JudgeName;
                    existingCase.ClientId = Case.ClientId;
                    existingCase.ModifiedOn = DateTime.UtcNow;

                    _context.Cases.Update(existingCase);

                    // Update or Create CasePayment
                    if (existingCase.CasePayment != null)
                    {
                        existingCase.CasePayment.TotalAmount = (Case.TotalAmount ?? 0);
                        existingCase.CasePayment.InitialAmount = (Case.InitialAmount ?? 0);
                        existingCase.CasePayment.ModifiedOn = DateTime.UtcNow;
                        _context.CasePayments.Update(existingCase.CasePayment);
                    }
                    else if (Case.TotalAmount.HasValue || Case.InitialAmount.HasValue)
                    {
                        var casePayment = new CasePayment
                        {
                            CaseId = existingCase.CaseId,
                            TotalAmount = (decimal)(Case.TotalAmount ?? 0),
                            InitialAmount = (decimal)(Case.InitialAmount ?? 0),
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _context.CasePayments.AddAsync(casePayment);
                    }

                    // Get the latest case date or create new one
                    var latestCaseDate = existingCase.CaseDates?.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault();

                    if (latestCaseDate != null)
                    {
                        latestCaseDate.PreviousDate = Case.PreviousDate ?? DateTime.Now;
                        latestCaseDate.NextDate = Case.NextDate ?? DateTime.Now;
                        latestCaseDate.Comment = Case.Comment;
                        latestCaseDate.JudgeRemarks = Case.JudgeRemarks;
                        latestCaseDate.ClientRemarks = Case.ClientRemarks;
                        latestCaseDate.Conclusion = Case.Conclusion;
                        latestCaseDate.ModifiedOn = DateTime.UtcNow;
                        _context.CaseDates.Update(latestCaseDate);
                    }
                    else if (Case.PreviousDate.HasValue || Case.NextDate.HasValue ||
                             !string.IsNullOrEmpty(Case.Comment) || !string.IsNullOrEmpty(Case.JudgeRemarks) ||
                             !string.IsNullOrEmpty(Case.ClientRemarks) || !string.IsNullOrEmpty(Case.Conclusion))
                    {
                        var caseDate = new CaseDate
                        {
                            CaseId = existingCase.CaseId,
                            PreviousDate = Case.PreviousDate ?? DateTime.Now,
                            NextDate = Case.NextDate ?? DateTime.Now,
                            Comment = Case.Comment,
                            JudgeRemarks = Case.JudgeRemarks,
                            ClientRemarks = Case.ClientRemarks,
                            Conclusion = Case.Conclusion,
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = false
                        };
                        await _context.CaseDates.AddAsync(caseDate);
                    }

                    // Update or Create Document
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
                                CaseDateId = (int)(latestCaseDate?.CaseDateId),
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