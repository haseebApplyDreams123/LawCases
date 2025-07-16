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

        public Client Client { get; set; }

        public int UserId { get; set; }

        public User User { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public CasePayment? CasePayment { get; set; }

        public ICollection<CaseDate>? CaseDates { get; set; }

        public ICollection<Document>? Documents { get; set; }
    }
}
