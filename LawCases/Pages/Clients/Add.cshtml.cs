using LawCases.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using LawCases.Models.ViewModels;
using LawCases.Helpers;

namespace LawCases.Pages.Clients
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
        public ClientViewModel Client { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            Client = new ClientViewModel
            {
                // Remove CreatedOn from ViewModel as it's not needed in the form
                DateOfBirth = DateTime.Now.AddYears(-18) // Default to 18 years ago
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userIdString = _userManager.GetUserId(User);

            int userId = 0;
            bool isParsed = int.TryParse(userIdString, out userId);

            if (string.IsNullOrEmpty(userIdString) || !isParsed)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var newClient = new Client
            {
                UserId = userId,
                FirstName = Client.FirstName,
                LastName = Client.LastName,
                Image = Client.Image,
                DateOfBirth = Client.DateOfBirth,
                Email = Client.Email,
                Username = Client.Username,
                PhoneNumber = Client.PhoneNumber,
                Address = Client.Address,
                Gender = Client.Gender,
                MaritalStatus = Client.MaritalStatus,
                CreatedOn = DateTime.UtcNow,
                IsDeleted = false
            };

            await _context.Clients.AddAsync(newClient);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Index");
        }



    }
}