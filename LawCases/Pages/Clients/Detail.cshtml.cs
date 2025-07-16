using LawCases.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using LawCases.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LawCases.Pages.Clients
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailModel(LawCases.Data.ApplicationDbContext context,
                         UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public ClientViewModel Client { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userIdString = _userManager.GetUserId(User);
            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var client = await _context.Clients
                .Where(c => c.ClientId == id && c.UserId == userId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            Client = new ClientViewModel
            {
                Id = client.ClientId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Image = client.Image,
                DateOfBirth = client.DateOfBirth,
                Email = client.Email,
                Username = client.Username,
                PhoneNumber = client.PhoneNumber,
                Address = client.Address,
                Gender = client.Gender,
                MaritalStatus = client.MaritalStatus,
                CreatedOn = client.CreatedOn,
                ModifiedOn = client.ModifiedOn
            };

            return Page();
        }
    }
}