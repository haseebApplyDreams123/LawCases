using LawCases.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace LawCases.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CasePayment> CasePayments { get; set; }
        public DbSet<CaseDate> CaseDates { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CaseTransaction> CaseTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).IsRequired().HasMaxLength(256);
            });

            // Configure Client entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(c => c.ClientId);
                entity.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.LastName).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Email).HasMaxLength(256);
                
                // Client-User relationship
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Clients)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Case entity
            modelBuilder.Entity<Case>(entity =>
            {
                entity.HasKey(c => c.CaseId);
                entity.Property(c => c.Title).IsRequired().HasMaxLength(200);
                entity.Property(c => c.CaseCode).HasMaxLength(50);
                entity.Property(c => c.CourtName).HasMaxLength(200);
                entity.Property(c => c.Status).HasMaxLength(50);
                entity.Property(c => c.Category).HasMaxLength(50);
                
                // Case-User relationship
                entity.HasOne(c => c.User)
                      .WithMany(u => u.Cases)
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Case-Client relationship
                entity.HasOne(c => c.Client)
                      .WithMany(cl => cl.Cases)
                      .HasForeignKey(c => c.ClientId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CasePayment entity
            modelBuilder.Entity<CasePayment>(entity =>
            {
                // FIXED: CasePayment should have its own primary key, not use CaseId as PK
                entity.HasKey(cp => cp.PaymentId); // Assuming PaymentId is the primary key
                
                // CasePayment-Case relationship (One-to-Many)
                entity.HasOne(cp => cp.Case)          
                      .WithMany(c => c.CasePayment)
                      .HasForeignKey(cp => cp.CaseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure CaseDate entity
            modelBuilder.Entity<CaseDate>(entity =>
            {
                entity.HasKey(cd => cd.CaseDateId);
                
                // CaseDate-Case relationship
                entity.HasOne(cd => cd.Case)
                      .WithMany(c => c.CaseDates)
                      .HasForeignKey(cd => cd.CaseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Document entity
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(d => d.DocumentId);
                
                // Document-Case relationship
                entity.HasOne(d => d.Case)
                      .WithMany(c => c.Documents)
                      .HasForeignKey(d => d.CaseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Category entity
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.CategoryId);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasMaxLength(500);
                entity.HasIndex(c => c.Name).IsUnique();
            });

            // IMPORTANT: Add global query filters for soft delete
            // This ensures that soft-deleted records are automatically excluded from queries
            modelBuilder.Entity<Client>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Case>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<CasePayment>().HasQueryFilter(cp => !cp.IsDeleted);
            modelBuilder.Entity<CaseDate>().HasQueryFilter(cd => !cd.IsDeleted);
            modelBuilder.Entity<Document>().HasQueryFilter(d => !d.IsDeleted);

            // Add indexes for better performance
            modelBuilder.Entity<Case>()
                .HasIndex(c => new { c.UserId, c.IsDeleted })
                .HasDatabaseName("IX_Cases_UserId_IsDeleted");

            modelBuilder.Entity<Case>()
                .HasIndex(c => c.CaseCode)
                .HasDatabaseName("IX_Cases_CaseCode");

            modelBuilder.Entity<CasePayment>()
                .HasIndex(cp => new { cp.CaseId, cp.IsDeleted })
                .HasDatabaseName("IX_CasePayments_CaseId_IsDeleted");

            modelBuilder.Entity<CaseDate>()
                .HasIndex(cd => new { cd.CaseId, cd.IsDeleted })
                .HasDatabaseName("IX_CaseDates_CaseId_IsDeleted");

            modelBuilder.Entity<Document>()
                .HasIndex(d => new { d.CaseId, d.IsDeleted })
                .HasDatabaseName("IX_Documents_CaseId_IsDeleted");

            // Add index for next dates for better performance on date-based queries
            modelBuilder.Entity<CaseDate>()
                .HasIndex(cd => cd.NextDate)
                .HasDatabaseName("IX_CaseDates_NextDate");
            // Configure CaseTransaction entity
            modelBuilder.Entity<CaseTransaction>(entity =>
            {
                entity.HasKey(ct => ct.TransactionId);

                entity.HasOne(ct => ct.Case)
                      .WithMany(c => c.CaseTransactions)
                      .HasForeignKey(ct => ct.CaseId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.Property(ct => ct.Notes).HasMaxLength(500);
                entity.HasQueryFilter(ct => !ct.IsDeleted);
            });

        }


        // Override SaveChanges to automatically set audit fields
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IAuditableEntity && 
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (IAuditableEntity)entry.Entity;
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedOn = now;
                    entity.ModifiedOn = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.ModifiedOn = now;
                }
            }
        }

        // Method to include soft-deleted records when needed
        public IQueryable<T> IncludeDeleted<T>() where T : class, ISoftDeletable
        {
            return Set<T>().IgnoreQueryFilters();
        }

        // Method to get only soft-deleted records
        public IQueryable<T> GetDeleted<T>() where T : class, ISoftDeletable
        {
            return Set<T>().IgnoreQueryFilters().Where(x => x.IsDeleted);
        }
    }

    // Interface for auditable entities
    public interface IAuditableEntity
    {
        DateTime CreatedOn { get; set; }
        DateTime ModifiedOn { get; set; }
    }

    // Interface for soft deletable entities
    public interface ISoftDeletable
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedOn { get; set; }
    }
}