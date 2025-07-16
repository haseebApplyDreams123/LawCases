using LawCases.Data;
using LawCases.Models;
using LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace LawCases.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            ApplicationDbContext context,
            ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public UserRegisterViewModel Input { get; set; } = new();

        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = false;
        public string? ReturnUrl { get; set; }


        public IActionResult OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                Message = "Please correct the errors and try again.";
                IsSuccess = false;
                return Page();
            }

            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
                if (existingUser != null)
                {
                    Message = "An account with this email already exists.";
                    IsSuccess = false;
                    _logger.LogWarning("Registration attempt with existing email: {Email}", Input.Email);
                    return Page();
                }

                // Create new user
                var user = new User
                {
                    Name = Input.Name,
                    Email = Input.Email,
                    PasswordHash = HashPassword(Input.Password),
                    CreatedOn = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("User {Email} created successfully", Input.Email);

                // You would typically sign in the user here, but without Identity you'll need an alternative
                // For now we'll just redirect to success

                Message = "Account created successfully! Welcome to our platform.";
                IsSuccess = true;

                // Redirect to return URL or home page
                if (Url.IsLocalUrl(ReturnUrl))
                {
                    return Redirect(ReturnUrl);
                }
                else
                {
                    return RedirectToPage("/Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration for {Email}", Input.Email);
                Message = "An error occurred while creating your account. Please try again.";
                IsSuccess = false;
                return Page();
            }
        }

        private string HashPassword(string password)
        {
            // Use proper password hashing (this is a simplified example)
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}