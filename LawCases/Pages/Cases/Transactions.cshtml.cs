using LawCases.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LawCases.Pages.Cases
{
    public class TransactionsModel : PageModel
    {
        private readonly LawCases.Data.ApplicationDbContext _context;

        public TransactionsModel(LawCases.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<CaseTransaction> CaseTransactions { get; set; }
        public CaseTransaction casetransaction { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CaseId { get; set; }

        public async Task OnGetAsync()
        {
            CaseTransactions = await _context.CaseTransactions
                .Where(ct => ct.CaseId == CaseId && !ct.IsDeleted)
                .OrderByDescending(ct => ct.TransactionDate)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteTransactionAsync(int transactionIdToDelete, int caseId)
        {
            var transactionToDelete = await _context.CaseTransactions.FindAsync(transactionIdToDelete);

            if (transactionToDelete != null)
            {
                transactionToDelete.IsDeleted = true;
                transactionToDelete.DeletedOn = DateTime.UtcNow;
                _context.CaseTransactions.Update(transactionToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Transactions", new { caseId });
        }

        public async Task<IActionResult> OnPostUpdateTransactionAsync(int TransactionId, int CaseId, decimal Amount,
            string Notes, DateTime TransactionDate)
        {
            var transactionToUpdate = await _context.CaseTransactions.FindAsync(TransactionId);

            if (transactionToUpdate != null)
            {
                transactionToUpdate.Amount = Amount;
                transactionToUpdate.Notes = Notes;
                transactionToUpdate.TransactionDate = TransactionDate;
                transactionToUpdate.ModifiedOn = DateTime.UtcNow;

                _context.CaseTransactions.Update(transactionToUpdate);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Transactions", new { caseId = CaseId });
        }
    }
}