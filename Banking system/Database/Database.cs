using Banking_system.Entity;
using Banking_system.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Banking_system.Database
{
    internal class Database : Microsoft.EntityFrameworkCore.DbContext
    {

        public Microsoft.EntityFrameworkCore.DbSet<User> Users { get; set; } = null!;
        public Microsoft.EntityFrameworkCore.DbSet<AbstractCard> Cards { get; set; } = null!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Users.db");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AbstractCard>()
                .HasDiscriminator<string>("CardType")
                .HasValue<DebitCard>("Debit")
                .HasValue<CreditCard>("Credit");
        }
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
