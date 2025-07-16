using System.ComponentModel.DataAnnotations;

namespace LawCases.Models
{
    public class CasePayment
    {
        [Key] public int CaseId { get; set; }

        public decimal TotalAmount { get; set; }

        public decimal InitialAmount { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public Case Case { get; set; }
    }
}
