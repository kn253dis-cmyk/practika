using Banking_system.Entity;
using Banking_system.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Banking_system.DataBase
{
    internal class Database : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<AbstractCard> Cards { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString = "Host=ep-floral-pine-ab8u8m5f-pooler.eu-west-2.aws.neon.tech; Database=neondb; Username=neondb_owner; Password=npg_5Aagl7zryMWH; SSL Mode=VerifyFull; Channel Binding=Require;Timeout=60;Command Timeout=60;";
            optionsBuilder.UseNpgsql(connectionString, builder =>
            {
                builder.EnableRetryOnFailure(
                    maxRetryCount: 5, 
                    maxRetryDelay: TimeSpan.FromSeconds(30), 
                    errorCodesToAdd: null);
            });
            optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
            optionsBuilder.UseNpgsql(connectionString);
            */

            // ====================================================================
            // АКТИВНО: Локальне підключення до бази SQLite 
            // ====================================================================
            optionsBuilder.UseSqlite("Data Source=BankingSystem.db");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AbstractCard>()
                .HasDiscriminator<string>("CardType")
                .HasValue<DebitCard>("Debit")
                .HasValue<CreditCard>("Credit")
                .HasValue<JuniorCard>("Unior");
        }

        public List<AbstractCard> FindAllCardsByUserId(int userId) => Cards.Where(c => c.UserId == userId).ToList();

        public string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                return builder.ToString();
            }
        }
    }
}