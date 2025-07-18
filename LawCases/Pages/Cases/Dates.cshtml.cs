using LawCases.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LawCases.Pages.Cases
{
    public class DatesModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DatesModel(LawCases.Data.ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<CaseDate> CaseDates { get; set; }
        public CaseDate casedate { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CaseId { get; set; }

        public async Task OnGetAsync()
        {
            CaseDates = await _context.CaseDates
                .Where(cd => cd.CaseId == CaseId && !cd.IsDeleted)
                .OrderByDescending(cd => cd.NextDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteDateAsync(int dateIdToDelete, int caseId)
        {
            var dateToDelete = await _context.CaseDates.FindAsync(dateIdToDelete);

            if (dateToDelete != null)
            {
                dateToDelete.IsDeleted = true;
                _context.CaseDates.Update(dateToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Dates", new { caseId });
        }

        public async Task<IActionResult> OnPostUpdateDateAsync(int CaseDateId, int CaseId, DateTime NextDate, string NextTime,
            string Comment, string JudgeRemarks, string ClientRemarks, string Conclusion)
        {
            // Combine date and time
            var timeParts = NextTime.Split(':');
            var combinedDateTime = NextDate.Add(new TimeSpan(int.Parse(timeParts[0]), int.Parse(timeParts[1]), 0));

            var dateToUpdate = await _context.CaseDates.FindAsync(CaseDateId);

            if (dateToUpdate != null)
            {
                dateToUpdate.NextDate = combinedDateTime;
                dateToUpdate.Comment = Comment;
                dateToUpdate.JudgeRemarks = JudgeRemarks;
                dateToUpdate.ClientRemarks = ClientRemarks;
                dateToUpdate.Conclusion = Conclusion;

                _context.CaseDates.Update(dateToUpdate);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Dates", new { caseId = CaseId });
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

            return RedirectToPage("./Dates", new { caseId });
        }
    }
}