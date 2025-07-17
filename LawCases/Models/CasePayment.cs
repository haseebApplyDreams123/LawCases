using LawCases.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LawCases.Models
{
    public class CasePayment 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ✅ This ensures AUTO_INCREMENT
        public int PaymentId { get; set; }
        public int CaseId { get; set; }

        public int? TotalAmount { get; set; }

        public int? InitialAmount { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public virtual  Case Case { get; set; }
    }
}
