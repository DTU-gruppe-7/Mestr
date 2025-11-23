using Mestr.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Mestr.Data.DbContext
{
    public class dbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        private static readonly Lazy<dbContext> _instance = new Lazy<dbContext>(() => new dbContext(), LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public static dbContext Instance => _instance.Value;

        // Property to access the semaphore for controlling database access
        public static SemaphoreSlim DatabaseLock => _semaphore;

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;
        public DbSet<Earning> Earnings { get; set; } = null!;
        public DbSet<CompanyProfile> CompanyProfile { get; set; } = null!;

        private dbContext()
        {
            Database.EnsureCreated(); // Ensure database is created
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dbFolder = Path.Combine(appDataPath, "Mestr");
                Directory.CreateDirectory(dbFolder);
                var dbPath = Path.Combine(dbFolder, "mestr.db");

                optionsBuilder.UseSqlite($"Data Source={dbPath}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Client entity
            modelBuilder.Entity<Client>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.Property(e => e.Uuid).HasField("_uuid");
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.ContactPerson).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.PostalAddress).IsRequired();
                entity.Property(e => e.City).IsRequired();
            });

            // Configure Project entity
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.Property(e => e.Uuid).HasField("_uuid");
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Description).IsRequired();

                // Relationship with Client
                entity.HasOne(e => e.Client)
                    .WithMany(c => c.Projects)
                    .HasForeignKey("ClientUuid")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                // Relationships with Expenses and Earnings - NOW WITH NAVIGATION
                entity.HasMany(e => e.Expenses)
                    .WithOne(ex => ex.Project)
                    .HasForeignKey(ex => ex.ProjectUuid)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Earnings)
                    .WithOne(ea => ea.Project)
                    .HasForeignKey(ea => ea.ProjectUuid)
                    .OnDelete(DeleteBehavior.Cascade);

                // Ignore computed property
                entity.Ignore(e => e.Result);
            });

            // Configure Expense entity
            modelBuilder.Entity<Expense>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.Property(e => e.Uuid).HasField("_uuid");
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                
                // Explicit foreign key property
                entity.Property(e => e.ProjectUuid).IsRequired();
            });

            // Configure Earning entity
            modelBuilder.Entity<Earning>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.Property(e => e.Uuid).HasField("_uuid");
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
                entity.Property(e => e.Date).IsRequired();
                
                // Explicit foreign key property
                entity.Property(e => e.ProjectUuid).IsRequired();
            });
        }
    }
}
