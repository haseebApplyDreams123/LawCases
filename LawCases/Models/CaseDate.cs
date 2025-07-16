namespace LawCases.Models
{
    public class CaseDate
    {
        public int CaseDateId { get; set; }

        public int CaseId { get; set; }

        public Case Case { get; set; }

        public DateTime PreviousDate { get; set; }

        public DateTime NextDate { get; set; }

        public string Comment { get; set; }

        public string JudgeRemarks { get; set; }

        public string ClientRemarks { get; set; }

        public string Conclusion { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }
    }
}
