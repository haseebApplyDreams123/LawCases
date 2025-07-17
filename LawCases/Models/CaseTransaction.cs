using LawCases.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawCases.Models
{
    public class CaseTransaction 
    {
        [Key]
        public int TransactionId { get; set; }

        [Required]
        public int CaseId { get; set; }

        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedOn { get; set; }

        [ForeignKey("CaseId")]
        public virtual Case Case { get; set; }
    }
}
