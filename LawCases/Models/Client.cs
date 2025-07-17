using LawCases.Data;
using System.ComponentModel.DataAnnotations;

namespace LawCases.Models
{
    public class Client 
    {
        public int ClientId { get; set; }

        public int UserId { get; set; }

        [Required] public string FirstName { get; set; }

        [Required] public string LastName { get; set; }

        public string? Image { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; }

        public string Username { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public string MaritalStatus { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }

        public virtual User User { get; set; }
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}
