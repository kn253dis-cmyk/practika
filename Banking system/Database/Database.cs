using Banking_system.Entity;
using Banking_system.Models;
using Banking_system.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
        public DbSet<AbstractTransaction> transactions { get; set; } = null!;


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
            optionsBuilder.ConfigureWarnings(w =>w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AbstractCard>()
                .HasDiscriminator<string>("CardType")
                .HasValue<DebitCard>("Debit")
                .HasValue<CreditCard>("Credit")
                .HasValue<CurrencyCard>("Currency"); 

            modelBuilder.Entity<AbstractTransaction>()
                .HasDiscriminator<string>("TransactionType")
                .HasValue<DepositTransaction>("Deposit")
                .HasValue<WithdrawTransaction>("Withdraw") 
                .HasValue<TransferTransaction>("Transfer")
                .HasValue<CurrencyExchangeTransaction>("Exchange");

            modelBuilder.Entity<AbstractCard>()
                .HasMany(c => c.LastTransactions) 
                .WithOne()                       
                .HasForeignKey("CardId")         
                .OnDelete(DeleteBehavior.Cascade);
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
        public bool TrySaveCardCurrency(string cardNumber, string targetCurrency)
        {
            var card = Cards.FirstOrDefault(c => c.CardNumber == cardNumber) as CurrencyCard;

            if (card == null) return false;

            if (!string.IsNullOrEmpty(card.CurrencyType) && card.CurrencyType != targetCurrency)
                return false;

            if (string.IsNullOrEmpty(card.CurrencyType))
            {
                card.CurrencyType = targetCurrency;
                SaveChanges(); 
            }

            return true;
        }
    }
}