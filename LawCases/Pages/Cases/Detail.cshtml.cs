using LawCases.Models;
using LawCases.Models.ViewModels;
using LawCases.Models.ViewModels.LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LawCases.Pages.Cases
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(LawCases.Data.ApplicationDbContext context,
                           UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public CaseViewModel Case { get; set; }
        public string ClientName { get; set; }
        public string CategoryName { get; set; }

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
                .Include(c => c.CasePayment)
                .Include(c => c.CaseDates)
                .Include(c => c.Documents)
                .Include(c => c.Client)
                .FirstOrDefaultAsync(c => c.CaseId == id && c.UserId == userId && !c.IsDeleted);

            if (caseEntity == null)
            {
                return NotFound();
            }

            // Get client name
            ClientName = caseEntity.Client != null ?
                $"{caseEntity.Client.FirstName} {caseEntity.Client.LastName}" : "N/A";

            // Get category name
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == caseEntity.Category && !c.IsDeleted);
            CategoryName = category?.Name ?? "N/A";

            // Get the latest payment
            var payment = caseEntity.CasePayment?.OrderByDescending(p => p.CreatedOn).FirstOrDefault();

            // Get the latest case date
            var latestCaseDate = caseEntity.CaseDates?.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault();

            // Get the first document
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
                TotalAmount = payment?.TotalAmount,
                InitialAmount = payment?.InitialAmount,
              //  PreviousDate = latestCaseDate?.PreviousDate,
                NextDate = latestCaseDate?.NextDate,
                Comment = latestCaseDate?.Comment,
                JudgeRemarks = latestCaseDate?.JudgeRemarks,
                ClientRemarks = latestCaseDate?.ClientRemarks,
                Conclusion = latestCaseDate?.Conclusion,
                FileName = document?.FileName,
                FileType = document?.FileType
            };

            return Page();
        }
    }
}