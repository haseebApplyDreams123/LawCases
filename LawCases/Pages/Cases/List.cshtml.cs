using LawCases.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;


namespace LawCases.Pages.Cases
{
    [Authorize]
    public class ListModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ListModel(LawCases.Data.ApplicationDbContext context,
                        UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<CaseListViewModel> Cases { get; set; } = new List<CaseListViewModel>();

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var query = _context.Cases
                .Where(c => c.UserId == userId && !c.IsDeleted)
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
                  //  TotalAmount = c.CasePayment.FirstOrDefault().TotalAmount ?? 0,
                    //InitialAmount = c.CasePayment.FirstOrDefault().InitialAmount ?? 0,
                    NextDate = c.CaseDates.OrderByDescending(cd => cd.CreatedOn).FirstOrDefault().NextDate,
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

            // Apply status filter
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                query = query.Where(c => c.Status == StatusFilter);
            }

            // Apply category filter
            if (!string.IsNullOrEmpty(CategoryFilter))
            {
                query = query.Where(c => c.Category == CategoryFilter);
            }

            Cases = await query.OrderByDescending(c => c.CreatedOn).ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var caseToDelete = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseId == id && c.UserId == userId && !c.IsDeleted);

            if (caseToDelete != null)
            {
                // Soft delete
                caseToDelete.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToPage();
        }
    }

    public class CaseListViewModel
    {
        public int CaseId { get; set; }
        public string Title { get; set; }
        public string CourtName { get; set; }
        public string CaseCode { get; set; }
        public string FilingNumber { get; set; }
        public string FIRNumber { get; set; }
        public string CaseNumber { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; }
        public string CloseType { get; set; }
        public string PoliceStation { get; set; }
        public string District { get; set; }
        public int FIRYear { get; set; }
        public string Category { get; set; }
        public string JudgeName { get; set; }
        public string ClientName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal InitialAmount { get; set; }
        public DateTime? NextDate { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}