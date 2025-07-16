namespace LawCases.Models.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    namespace LawCases.Models.ViewModels
    {
        public class CaseViewModel


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

            public string? CaseNumber { get; set; }

            [Required]
            public DateTime StartDate { get; set; }

            [Required]
            public string Status { get; set; } // Block, Open, Closed

            public string? CloseType { get; set; } // Temporary, Permanent

            public string? PoliceStation { get; set; }

            public string? District { get; set; }

            [Required]
            public int FIRYear { get; set; }

            [Required]
            public string Category { get; set; }

            public string? JudgeName { get; set; }

            [Required]
            public int ClientId { get; set; }

            public decimal? TotalAmount { get; set; }

            public decimal? InitialAmount { get; set; }

            // Case Date fields
            public DateTime? PreviousDate { get; set; }

            public DateTime? NextDate { get; set; }

            public string? Comment { get; set; }

            public string? JudgeRemarks { get; set; }

            public string? ClientRemarks { get; set; }

            public string? Conclusion { get; set; }

            // Document fields
            public string? FileName { get; set; }

            public string? FileType { get; set; }
        }
    }
}
