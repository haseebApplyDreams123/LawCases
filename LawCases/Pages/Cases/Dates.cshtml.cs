using LawCases.Models;
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

        public DatesModel(LawCases.Data.ApplicationDbContext context)
        {
            _context = context;
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
    }
}