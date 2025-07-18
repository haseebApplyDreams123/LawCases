using LawCases.Models;
using LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        public SelectList CategorySelectList { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "Title";  // Default ab "Title" column

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "asc";  // Ascending order (A-Z)

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;


        public async Task<IActionResult> OnGetAsync()
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var categories = await _context.Categories
    .Where(c => !c.IsDeleted)
    .Select(c => new { c.Name, c.Description })
    .ToListAsync();

            CategorySelectList = new SelectList(categories, "Name", "Name");
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
                    // Fixed: Handle null CasePayments properly
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

            // Count total records for pagination
            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);

            // Apply sorting
            switch(SortBy.ToLower())
            {
                case "title":
                default:
                    query = SortOrder == "asc" ? query.OrderBy(c => c.Title) : query.OrderByDescending(c => c.Title);
                    break;
                case "clientname":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.ClientName) : query.OrderByDescending(c => c.ClientName);
                    break;
                case "startdate":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.StartDate) : query.OrderByDescending(c => c.StartDate);
                    break;
                case "nextdate":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.NextDate) : query.OrderByDescending(c => c.NextDate);
                    break;
                case "status":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.Status) : query.OrderByDescending(c => c.Status);
                    break;
                case "totalamount":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.TotalAmount) : query.OrderByDescending(c => c.TotalAmount);
                    break;
                case "createdon":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.CreatedOn) : query.OrderByDescending(c => c.CreatedOn);
                    break;
            }


            Cases = await query
          .Skip((PageNumber - 1) * PageSize)
          .Take(PageSize)
          .ToListAsync();



            return Page();
        }

        public string GetPageUrl(int pageNumber)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");

            if (!string.IsNullOrEmpty(StatusFilter))
                queryParams.Add($"statusFilter={Uri.EscapeDataString(StatusFilter)}");

            if (!string.IsNullOrEmpty(CategoryFilter))
                queryParams.Add($"categoryFilter={Uri.EscapeDataString(CategoryFilter)}");

            if (!string.IsNullOrEmpty(SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(SortBy)}");

            if (!string.IsNullOrEmpty(SortOrder))
                queryParams.Add($"sortOrder={Uri.EscapeDataString(SortOrder)}");

            queryParams.Add($"pageNumber={pageNumber}");
            queryParams.Add($"pageSize={PageSize}");

            return $"/Cases/List?{string.Join("&", queryParams)}";
        }

        // Add this helper method to generate sort URLs
        public string GetSortUrl(string sortBy)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");

            if (!string.IsNullOrEmpty(StatusFilter))
                queryParams.Add($"statusFilter={Uri.EscapeDataString(StatusFilter)}");

            if (!string.IsNullOrEmpty(CategoryFilter))
                queryParams.Add($"categoryFilter={Uri.EscapeDataString(CategoryFilter)}");

            // Toggle sort order if clicking the same column
            string newSortOrder = "asc";
            if (SortBy == sortBy)
            {
                newSortOrder = SortOrder == "asc" ? "desc" : "asc";
            }

            queryParams.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            queryParams.Add($"sortOrder={Uri.EscapeDataString(newSortOrder)}");
            queryParams.Add($"pageSize={PageSize}");
            queryParams.Add($"pageNumber={1}"); // Reset to first page when sorting

            return $"/Cases/List?{string.Join("&", queryParams)}";
        }

        // Updated delete method with proper cascading soft delete
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Use transaction to ensure all related data is deleted consistently
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var caseToDelete = await _context.Cases
                    .Include(c => c.CaseDates)
                    .Include(c => c.CasePayment)
                    .Include(c => c.Documents) // Assuming you have Documents navigation property
                    .FirstOrDefaultAsync(c => c.CaseId == id && c.UserId == userId && !c.IsDeleted);

                if (caseToDelete != null)
                {
                    // Soft delete the main case
                    caseToDelete.IsDeleted = true;
                    caseToDelete.DeletedOn = DateTime.UtcNow; // Add this property if you want to track deletion time

                    // Soft delete related CaseDates
                    foreach (var caseDate in caseToDelete.CaseDates.Where(cd => !cd.IsDeleted))
                    {
                        caseDate.IsDeleted = true;
                        caseDate.DeletedOn = DateTime.UtcNow;
                    }

                    // Soft delete related CasePayments
                    foreach (var payment in caseToDelete.CasePayment.Where(cp => !cp.IsDeleted))
                    {
                        payment.IsDeleted = true;
                        payment.DeletedOn = DateTime.UtcNow;
                    }

                    // Soft delete related Documents (if you have Documents table)
                    if (caseToDelete.Documents != null)
                    {
                        foreach (var document in caseToDelete.Documents.Where(d => !d.IsDeleted))
                        {
                            document.IsDeleted = true;
                            document.DeletedOn = DateTime.UtcNow;
                        }
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                // Log the exception
                // Consider adding proper logging here
                throw;
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCloseCase(int caseId, string closeType, string closeNotes)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var caseToClose = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseId == caseId && c.UserId == userId && !c.IsDeleted);

            if (caseToClose == null)
            {
                return NotFound();
            }

            if (closeType == "Temporary")
            {
                caseToClose.Status = "Temporary Closed";
                caseToClose.CloseType = closeType;
                caseToClose.ModifiedOn = DateTime.UtcNow;
            }

            else if (closeType == "Permanent")
            {
                caseToClose.Status = "Closed";
                caseToClose.CloseType = closeType;
                caseToClose.ModifiedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<JsonResult> OnGetCaseDates(int caseId)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return new JsonResult(new { error = "Unauthorized" });
            }

            var caseInfo = await _context.Cases
                .Where(c => c.CaseId == caseId && c.UserId == userId && !c.IsDeleted)
                .Select(c => new
                {
                    StartDate = c.StartDate,
                    LatestNextDate = (DateTime?)c.CaseDates
                        .Where(cd => !cd.IsDeleted)
                        .OrderByDescending(cd => cd.NextDate)
                        .Select(cd => cd.NextDate)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (caseInfo == null)
            {
                return new JsonResult(new { error = "Case not found" });
            }

            return new JsonResult(new
            {
                startDate = caseInfo.StartDate.ToString("yyyy-MM-dd"),
                latestNextDate = caseInfo.LatestNextDate.HasValue
                    ? caseInfo.LatestNextDate.Value.ToString("yyyy-MM-dd")
                    : null
            });
        }

        public async Task<IActionResult> OnPostAddTransaction(int caseId, DateTime transactionDate, decimal amount, string comment)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Validate the case exists and belongs to the user
            var caseExists = await _context.Cases
                .AnyAsync(c => c.CaseId == caseId && c.UserId == userId && !c.IsDeleted);

            if (!caseExists)
            {
                return NotFound();
            }

            // Server-side validation
            if (amount <= 0)
            {
                ModelState.AddModelError("amount", "Amount must be greater than 0");
            }

            if (transactionDate > DateTime.UtcNow.Date)
            {
                ModelState.AddModelError("transactionDate", "Transaction date cannot be in the future");
            }


            // Create new transaction
            var transaction = new CaseTransaction
            {
                CaseId = caseId,
                Amount = amount,
                Notes = comment,
                TransactionDate = transactionDate,
                CreatedOn = DateTime.UtcNow
            };

            _context.CaseTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddNextDate(int caseId, DateTime nextDate, string comment)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Get the case with its dates
            var caseInfo = await _context.Cases
                .Include(c => c.CaseDates)
                .FirstOrDefaultAsync(c => c.CaseId == caseId && c.UserId == userId && !c.IsDeleted);

            if (caseInfo == null)
            {
                return NotFound();
            }

            // Server-side validation
            if (nextDate <= caseInfo.StartDate)
            {
                ModelState.AddModelError("nextDate", "Next date must be after the case start date");
            }

            var latestNextDate = await _context.CaseDates
                .Where(cd => cd.CaseId == caseId && !cd.IsDeleted)
                .OrderByDescending(cd => cd.NextDate)
                .Select(cd => (DateTime?)cd.NextDate)
                .FirstOrDefaultAsync();

            if (latestNextDate.HasValue && nextDate <= latestNextDate.Value)
            {
                ModelState.AddModelError("nextDate", "Next date must be after the previous next date");
            }

     

            // Create new CaseDate
            var caseDate = new CaseDate
            {
                CaseId = caseId,
                NextDate = nextDate,
                Comment = comment,
                CreatedOn = DateTime.UtcNow
            };

            _context.CaseDates.Add(caseDate);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }


        public async Task<IActionResult> OnPostReopenCase(int caseId, string reopenNotes)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var caseToReopen = await _context.Cases
                .FirstOrDefaultAsync(c => c.CaseId == caseId && c.UserId == userId && !c.IsDeleted);

            if (caseToReopen == null)
            {
                return NotFound();
            }

            caseToReopen.Status = "Open";
            caseToReopen.CloseType = null; 
            caseToReopen.ModifiedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

      
    }

}