using System.ComponentModel.DataAnnotations;

namespace LawCases.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required] public string Name { get; set; }

        [Required, EmailAddress] public string Email { get; set; }

        [Required] public string PasswordHash { get; set; }

        public string? Image { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedOn { get; set; }

        public ICollection<Client>? Clients { get; set; }

        public ICollection<Case>? Cases { get; set; }
    }
}
