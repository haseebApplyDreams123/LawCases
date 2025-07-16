using LawCases.Models;

namespace LawCases.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated(); // Ensures DB is created without migrations

            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Civil", Description = "Civil law cases", CreatedOn = DateTime.UtcNow },
                    new Category { Name = "Criminal", Description = "Criminal law cases", CreatedOn = DateTime.UtcNow },
                    new Category { Name = "Family", Description = "Family law cases", CreatedOn = DateTime.UtcNow }
                };

                context.Categories.AddRange(categories);
                context.SaveChanges();
            }
        }
    }
}
