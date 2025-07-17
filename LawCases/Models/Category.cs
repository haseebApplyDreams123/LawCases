namespace LawCases.Models
{
    public class Category 
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }
    }
}
