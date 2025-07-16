using System.ComponentModel.DataAnnotations;

namespace LawCases.Models.ViewModels
{
    public class CaseListViewModel
    {
        public int CaseId { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string CourtName { get; set; }
        [Required]
        public string CaseCode { get; set; }
        [Required]
        public string FilingNumber { get; set; }
        [Required]
        public string FIRNumber { get; set; }
        [Required]
        public string CaseNumber { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public string Status { get; set; }

        public string? CloseType { get; set; }
        public string? PoliceStation { get; set; }
        public string? District { get; set; }
        [Required]
        public int FIRYear { get; set; }
        [Required]
        public string? Category { get; set; }
        public string? JudgeName { get; set; }

        [Required]
        public string ClientName { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? InitialAmount { get; set; }
        public DateTime? NextDate { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
