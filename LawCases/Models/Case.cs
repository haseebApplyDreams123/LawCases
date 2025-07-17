using LawCases.Data;

namespace LawCases.Models
{
    public class Case 
    {
        public int CaseId { get; set; }

        public string Title { get; set; }

        public string CourtName { get; set; }

        public string CaseCode { get; set; }

        public string FilingNumber { get; set; }

        public string FIRNumber { get; set; }

        public string? CaseNumber { get; set; }

        public DateTime StartDate { get; set; }

        public string Status { get; set; } // Block, Open, Closed

        public string? CloseType { get; set; } // Temporary, Permanent

        public string? PoliceStation { get; set; }

        public string? District { get; set; }

        public int FIRYear { get; set; }

        public string Category { get; set; }

        public string JudgeName { get; set; }

        public int ClientId { get; set; }


        public int UserId { get; set; }


        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public virtual User User { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<CasePayment> CasePayment { get; set; } = new List<CasePayment>();
        public virtual ICollection<CaseDate> CaseDates { get; set; } = new List<CaseDate>();
        public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
        public virtual ICollection<CaseTransaction> CaseTransactions { get; set; } = new List<CaseTransaction>();

    }
}
