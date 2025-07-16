using LawCases.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using LawCases.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LawCases.Pages.Clients
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

            // Fix: Query by client ID and ensure it belongs to current user
            var client = await _context.Clients
                .Where(c => c.ClientId == id && c.UserId == userId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (client == null)
            {
                return NotFound();
            }

            Client = new ClientViewModel
            {
                Id = client.ClientId,  // Fix: Use client.Id instead of client.UserId
                FirstName = client.FirstName,
                LastName = client.LastName,
                Image = client.Image,
                DateOfBirth = client.DateOfBirth,
                Email = client.Email,
                Username = client.Username,
                PhoneNumber = client.PhoneNumber,
                Address = client.Address,
                Gender = client.Gender,
                MaritalStatus = client.MaritalStatus
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

            // Fix: Query by client ID and ensure it belongs to current user
            var existingClient = await _context.Clients
                .Where(c => c.ClientId == Client.Id && c.UserId == userId && !c.IsDeleted)
                .FirstOrDefaultAsync();

            if (existingClient == null)
            {
                return NotFound();
            }

            // Update the existing client properties
            existingClient.FirstName = Client.FirstName;
            existingClient.LastName = Client.LastName;
            existingClient.Image = Client.Image;
            existingClient.DateOfBirth = Client.DateOfBirth;
            existingClient.Email = Client.Email;
            existingClient.Username = Client.Username;
            existingClient.PhoneNumber = Client.PhoneNumber;
            existingClient.Address = Client.Address;
            existingClient.Gender = Client.Gender;
            existingClient.MaritalStatus = Client.MaritalStatus;
            existingClient.ModifiedOn = DateTime.UtcNow;

            _context.Clients.Update(existingClient);
            await _context.SaveChangesAsync();

            return RedirectToPage("/clients/list");
        }
    }
}