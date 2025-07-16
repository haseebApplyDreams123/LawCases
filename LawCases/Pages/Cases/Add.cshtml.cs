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
    public class AddModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AddModel(LawCases.Data.ApplicationDbContext context,
                      UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public CaseViewModel Case { get; set; }

        public SelectList ClientSelectList { get; set; }
        public SelectList CategorySelectList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Load clients for dropdown
            var clients = await _context.Clients
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .Select(c => new { c.ClientId, FullName = c.FirstName + " " + c.LastName })
                .ToListAsync();

            ClientSelectList = new SelectList(clients, "ClientId", "FullName");

            // Load categories for dropdown
            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Name, c.Description })
                .ToListAsync();

            CategorySelectList = new SelectList(categories, "Name", "Name");

            Case = new CaseViewModel
            {
                StartDate = DateTime.Now,
                NextDate = DateTime.Now.AddDays(7),
                PreviousDate = DateTime.Now,
                FIRYear = DateTime.Now.Year,
                Status = "Open"
            };

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

                ClientSelectList = new SelectList(clients, "ClientId", "FullName");

                var categories = await _context.Categories
                    .Where(c => !c.IsDeleted)
                    .Select(c => new { c.Name, c.Description })
                    .ToListAsync();

                CategorySelectList = new SelectList(categories, "Name", "Name");

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
                    // Create new Case
                    var newCase = new Case
                    {
                        Title = Case.Title,
                        CourtName = Case.CourtName,
                        CaseCode = Case.CaseCode,
                        FilingNumber = Case.FilingNumber,
                        FIRNumber = Case.FIRNumber,
                        CaseNumber = Case.CaseNumber,
                        StartDate = Case.StartDate,
                        Status = Case.Status,
                        CloseType = Case.CloseType,
                        PoliceStation = Case.PoliceStation,
                        District = Case.District,
                        FIRYear = Case.FIRYear,
                        Category = Case.Category,
                        JudgeName = Case.JudgeName,
                        ClientId = Case.ClientId,
                        UserId = userId2,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _context.Cases.AddAsync(newCase);
                    await _context.SaveChangesAsync();

                    // Create CasePayment
                    var casePayment = new CasePayment
                    {
                        CaseId = newCase.CaseId,
                        TotalAmount = (decimal)Case.TotalAmount,
                        InitialAmount = (decimal)Case.InitialAmount,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _context.CasePayments.AddAsync(casePayment);

                    // Create CaseDate
                    var caseDate = new CaseDate
                    {
                        CaseId = newCase.CaseId,
                        PreviousDate = (DateTime)Case.PreviousDate,
                        NextDate = (DateTime)Case.NextDate,
                        Comment = Case.Comment,
                        JudgeRemarks = Case.JudgeRemarks,
                        ClientRemarks = Case.ClientRemarks,
                        Conclusion = Case.Conclusion,
                        CreatedOn = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _context.CaseDates.AddAsync(caseDate);

                    // Create Document if file information is provided
                    if (!string.IsNullOrEmpty(Case.FileName))
                    {
                        var document = new Document
                        {
                            FileName = Case.FileName,
                            FileType = Case.FileType,
                            CaseId = newCase.CaseId,
                            CaseDateId = caseDate.CaseDateId,
                            ClientId = Case.ClientId,
                            CreatedOn = DateTime.UtcNow,
                            IsDeleted = false
                        };

                        await _context.Documents.AddAsync(document);
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