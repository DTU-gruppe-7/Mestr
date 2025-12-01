using Mestr.Core.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Mestr.Data.DbContext
{
    public class dbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        // Constructor er nu public, så Repositories kan oprette deres egne instanser.
        // Dette matcher "Unit of Work" princippet beskrevet i Pro C# 10 with .NET 6.
        public dbContext()
        {
            //Tom da databasen bliver oprettet i App.xaml.cs
        }

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Expense> Expenses { get; set; } = null!;
        public DbSet<Earning> Earnings { get; set; } = null!;
        public DbSet<CompanyProfile> CompanyProfile { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Byg konfigurationen
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                // Hent connection string fra filen
                var connectionString = config.GetConnectionString("DefaultConnection");

                // Brug den
                optionsBuilder.UseSqlite(connectionString);
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

                // Relationships with Expenses and Earnings
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

            // Configure CompanyProfile entity
            modelBuilder.Entity<CompanyProfile>(entity =>
            {
                entity.HasKey(e => e.Uuid);
                entity.Property(e => e.CompanyName).IsRequired();
                entity.Property(e => e.ContactPerson).IsRequired();
                entity.Property(e => e.Address).IsRequired();
                entity.Property(e => e.ZipCode).IsRequired();
                entity.Property(e => e.City).IsRequired();
                entity.Property(e => e.Cvr).IsRequired();
                entity.Property(e => e.Email).IsRequired();
                entity.Property(e => e.PhoneNumber).IsRequired();
                entity.Property(e => e.BankRegNumber).IsRequired();
                entity.Property(e => e.BankAccountNumber).IsRequired();
            });
        }
    }
}