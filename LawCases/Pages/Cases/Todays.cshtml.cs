using LawCases.Models;
using LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawCases.Pages.Cases
{
    [Authorize]
    public class TodayModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TodayModel(LawCases.Data.ApplicationDbContext context,
                        UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<CaseListViewModel> Cases { get; set; } = new List<CaseListViewModel>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

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

            var today = DateTime.Today;

            var categories = await _context.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new { c.Name, c.Description })
                .ToListAsync();

            CategorySelectList = new SelectList(categories, "Name", "Name");

            var query = _context.Cases
                .Where(c => c.UserId == userId &&
                           !c.IsDeleted &&
                           c.CaseDates.Any(cd => !cd.IsDeleted && cd.NextDate.Date == today))
                .Include(c => c.Client)
                .Include(c => c.CasePayment)
                .Include(c => c.CaseDates.OrderByDescending(cd => cd.CreatedOn))
                .Select(c => new CaseListViewModel
                {
                    CaseId = c.CaseId,
                    Title = c.Title,
                    CourtName = c.CourtName,
                    CaseCode = c.CaseCode,
                    FilingNumber = c.FilingNumber,
                    FIRNumber = c.FIRNumber,
                    CaseNumber = c.CaseNumber,
                    StartDate = c.StartDate,
                    Status = c.Status,
                    CloseType = c.CloseType,
                    PoliceStation = c.PoliceStation,
                    District = c.District,
                    FIRYear = c.FIRYear,
                    Category = c.Category,
                    JudgeName = c.JudgeName,
                    ClientName = c.Client.FirstName + " " + c.Client.LastName,
                    TotalAmount = c.CasePayment.FirstOrDefault() != null ? c.CasePayment.FirstOrDefault().TotalAmount : 0,
                    InitialAmount = c.CasePayment.FirstOrDefault() != null ? c.CasePayment.FirstOrDefault().InitialAmount : 0,
                    NextDate = c.CaseDates.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault() != null ?
                              c.CaseDates.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault().NextDate : null,
                    CreatedOn = c.CreatedOn
                });

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(c =>
                    c.Title.Contains(SearchTerm) ||
                    c.CaseCode.Contains(SearchTerm) ||
                    c.FilingNumber.Contains(SearchTerm) ||
                    c.FIRNumber.Contains(SearchTerm) ||
                    c.ClientName.Contains(SearchTerm) ||
                    c.CourtName.Contains(SearchTerm));
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                query = query.Where(c => c.Category == CategoryFilter);
            }

            Cases = await query.ToListAsync();

            return Page();
        }
    }
}