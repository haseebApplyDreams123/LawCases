using LawCases.Data;
using LawCases.Models;
using LawCases.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LawCases.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            ApplicationDbContext context,
            ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public UserLoginViewModel Input { get; set; } = new();

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
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Input.Email);
                if (user == null)
                {
                    Message = "Invalid email or password.";
                    IsSuccess = false;
                    return Page();
                }

                var hashedPassword = HashPassword(Input.Password);
                if (user.PasswordHash != hashedPassword)
                {
                    Message = "Invalid email or password.";
                    IsSuccess = false;
                    return Page();
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Role, "User") // Add role claim
        };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    new AuthenticationProperties
                    {
                        IsPersistent = Input.RememberMe,
                        ExpiresUtc = Input.RememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
                    });

                // Always redirect to Index after login (or returnUrl if valid)
                return LocalRedirect(Url.Content("~/Index"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                Message = "An error occurred while trying to log in.";
                IsSuccess = false;
                return Page();
            }
        }

        private string HashPassword(string password)
        {
            // Use the same hashing algorithm as in RegisterModel
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}