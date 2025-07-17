using LawCases.Data;

namespace LawCases.Models
{
    public class Document 
    {
        public int DocumentId { get; set; }

        public string? FileName { get; set; }

        public string? FileType { get; set; }

        public int CaseId { get; set; }

        public Case Case { get; set; }


        public int ClientId { get; set; }

        public Client Client { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? ModifiedOn { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedOn { get; set; }
    }
}
