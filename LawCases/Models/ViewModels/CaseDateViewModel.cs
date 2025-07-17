using System;
using System.ComponentModel.DataAnnotations;

namespace LawCases.Models
{
    public class CaseDateViewModel
    {
        public int CaseDateId { get; set; }

        [Required]
        public int CaseId { get; set; }

        public DateTime PreviousDate { get; set; }

        [Required(ErrorMessage = "Next hearing date is required")]
        public DateTime NextDate { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string Comment { get; set; }

        [StringLength(500, ErrorMessage = "Judge remarks cannot exceed 500 characters")]
        public string JudgeRemarks { get; set; }

        [StringLength(500, ErrorMessage = "Client remarks cannot exceed 500 characters")]
        public string ClientRemarks { get; set; }

        [StringLength(500, ErrorMessage = "Conclusion cannot exceed 500 characters")]
        public string Conclusion { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public virtual Case Case { get; set; }
    }
}