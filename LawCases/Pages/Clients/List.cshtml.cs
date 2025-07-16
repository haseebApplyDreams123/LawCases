using LawCases.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace LawCases.Pages.Clients
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

        public IEnumerable<Client> Clients { get; set; } = new List<Client>();

        // Filter properties
        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string GenderFilter { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string MaritalStatusFilter { get; set; } = "";

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "CreatedOn";

        [BindProperty(SupportsGet = true)]
        public string SortOrder { get; set; } = "desc";

        // Pagination properties
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        // Statistics properties
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int RecentClients { get; set; }
        public int ActiveCases { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user ID
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Start with base query for current user's clients
            var query = _context.Clients
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                var searchLower = SearchTerm.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(searchLower) ||
                    c.LastName.ToLower().Contains(searchLower) ||
                    c.Email.ToLower().Contains(searchLower) ||
                    c.PhoneNumber.Contains(SearchTerm) ||
                    c.Username.ToLower().Contains(searchLower)
                );
            }

            // Apply gender filter
            if (!string.IsNullOrEmpty(GenderFilter))
            {
                query = query.Where(c => c.Gender == GenderFilter);
            }

            // Apply marital status filter
            if (!string.IsNullOrEmpty(MaritalStatusFilter))
            {
                query = query.Where(c => c.MaritalStatus == MaritalStatusFilter);
            }

            // Count total records for pagination
            TotalRecords = await query.CountAsync();
            TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);

            // Apply sorting
            switch (SortBy.ToLower())
            {
                case "firstname":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.FirstName) : query.OrderByDescending(c => c.FirstName);
                    break;
                case "lastname":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.LastName) : query.OrderByDescending(c => c.LastName);
                    break;
                case "email":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.Email) : query.OrderByDescending(c => c.Email);
                    break;
                case "dateofbirth":
                    query = SortOrder == "asc" ? query.OrderBy(c => c.DateOfBirth) : query.OrderByDescending(c => c.DateOfBirth);
                    break;
                case "createdon":
                default:
                    query = SortOrder == "asc" ? query.OrderBy(c => c.CreatedOn) : query.OrderByDescending(c => c.CreatedOn);
                    break;
            }

            // Apply pagination
            query = query.Skip((PageNumber - 1) * PageSize).Take(PageSize);

            // Execute query
            Clients = await query.ToListAsync();

            // Calculate statistics
            await CalculateStatistics(userId);

            return Page();
        }

        private async Task CalculateStatistics(int userId)
        {
            var allClients = await _context.Clients
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .ToListAsync();

            TotalClients = allClients.Count;
            ActiveClients = allClients.Count; // Assuming all non-deleted clients are active

            // Recent clients (added in the last 30 days)
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            RecentClients = allClients.Count(c => c.CreatedOn >= thirtyDaysAgo);

            // Active cases - this would need to be implemented based on your Cases model
            // For now, setting to 0 as placeholder
            ActiveCases = 0;

            // If you have a Cases table, you can calculate like this:
            // ActiveCases = await _context.Cases
            //     .Where(c => c.UserId == userId && c.IsActive)
            //     .CountAsync();
        }

        // Fixed OnPostDeleteAsync method
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.ClientId == id && c.UserId == userId);

            if (client == null)
            {
                TempData["ErrorMessage"] = "Client not found";
                return RedirectToPage();
            }

            try
            {
                // Soft delete
                client.IsDeleted = true;
                client.DeletedOn = DateTime.UtcNow;

                _context.Clients.Update(client);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Client deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the client";
                // Log the exception if you have logging configured
                // _logger.LogError(ex, "Error deleting client with ID {ClientId}", id);
            }

            return RedirectToPage();
        }

        // Fixed OnPostBulkDeleteAsync method
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnPostBulkDeleteAsync(int[] clientIds)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            if (clientIds == null || clientIds.Length == 0)
            {
                return new JsonResult(new { success = false, message = "No clients selected" });
            }

            try
            {
                // Fix: Use ClientId instead of UserId for finding clients
                var clients = await _context.Clients
                    .Where(c => clientIds.Contains(c.ClientId) && c.UserId == userId && !c.IsDeleted)
                    .ToListAsync();

                if (clients.Count == 0)
                {
                    return new JsonResult(new { success = false, message = "No valid clients found to delete" });
                }

                // Soft delete all selected clients
                foreach (var client in clients)
                {
                    client.IsDeleted = true;
                    client.DeletedOn = DateTime.UtcNow;
                }

                _context.Clients.UpdateRange(clients);
                await _context.SaveChangesAsync();

                return new JsonResult(new
                {
                    success = true,
                    message = $"{clients.Count} client(s) deleted successfully"
                });
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                // _logger.LogError(ex, "Error bulk deleting clients");

                return new JsonResult(new { success = false, message = "An error occurred while deleting clients" });
            }
        }
        public async Task<IActionResult> OnGetExportAsync()
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return new JsonResult(new { success = false, message = "Unauthorized" });
            }

            try
            {
                var clients = await _context.Clients
                    .Where(c => c.UserId == userId && !c.IsDeleted)
                    .Select(c => new
                    {
                        c.FirstName,
                        c.LastName,
                        c.Email,
                        c.PhoneNumber,
                        c.Username,
                        c.Gender,
                        c.MaritalStatus,
                        DateOfBirth = c.DateOfBirth.ToString("yyyy-MM-dd"),
                        CreatedOn = c.CreatedOn.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                    .ToListAsync();

                return new JsonResult(new { success = true, data = clients });
            }
            catch (Exception ex)
            {
                // Log the exception if you have logging configured
                // _logger.LogError(ex, "Error exporting clients");

                return new JsonResult(new { success = false, message = "An error occurred while exporting clients" });
            }
        }

        // Helper method to get pagination info
        public string GetPaginationInfo()
        {
            int startRecord = (PageNumber - 1) * PageSize + 1;
            int endRecord = Math.Min(PageNumber * PageSize, TotalRecords);

            return $"Showing {startRecord} to {endRecord} of {TotalRecords} results";
        }

        // Helper method to generate page URLs
        public string GetPageUrl(int pageNumber)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(SearchTerm))
                queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");

            if (!string.IsNullOrEmpty(GenderFilter))
                queryParams.Add($"genderFilter={Uri.EscapeDataString(GenderFilter)}");

            if (!string.IsNullOrEmpty(MaritalStatusFilter))
                queryParams.Add($"maritalStatusFilter={Uri.EscapeDataString(MaritalStatusFilter)}");

            if (!string.IsNullOrEmpty(SortBy))
                queryParams.Add($"sortBy={Uri.EscapeDataString(SortBy)}");

            if (!string.IsNullOrEmpty(SortOrder))
                queryParams.Add($"sortOrder={Uri.EscapeDataString(SortOrder)}");

            queryParams.Add($"pageNumber={pageNumber}");
            queryParams.Add($"pageSize={PageSize}");

            return $"/Clients/List?{string.Join("&", queryParams)}";
        }
    }
}