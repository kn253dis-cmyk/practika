using Banking_system.Entity;
using Banking_system.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public void SaveCard(AbstractCard card)
        {
            using (var connection = new SqliteConnection("Data Source=Database/Cards.db"))
            {
                connection.Open();

                string insertQuery = @"
            INSERT INTO Cards (UserId, CardNumber, Balance, CardType, CreditLimit, ExpirationDate, CVV) 
            VALUES (@UserId, @CardNumber, @Balance, @CardType, @CreditLimit, @ExpirationDate, @CVV)";

                using (var command = new SqliteCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", card.UserId);
                    command.Parameters.AddWithValue("@CardNumber", card.CardNumber);
                    command.Parameters.AddWithValue("@Balance", card.Balance);
                    command.Parameters.AddWithValue("@ExpirationDate", card.GetExpirationDate());
                    command.Parameters.AddWithValue("@CVV", card.GetCVV());

                    // Визначаємо тип картки та специфічні поля
                    if (card is CreditCard creditCard)
                    {
                        command.Parameters.AddWithValue("@CardType", "Credit");
                        command.Parameters.AddWithValue("@CreditLimit", creditCard.CreditLimit);
                    }
                    else if (card is DebitCard)
                    {
                        command.Parameters.AddWithValue("@CardType", "Debit");
                        command.Parameters.AddWithValue("@CreditLimit", DBNull.Value);
                    }
                    else if (card is UniorCard)
                    {
                        command.Parameters.AddWithValue("@CardType", "Unior");
                        command.Parameters.AddWithValue("@CreditLimit", DBNull.Value);
                    }

                    command.ExecuteNonQuery();
                }
            }
        }
        public ObservableCollection<AbstractCard> GetUserCards(int userId)
        {
            var cards = new ObservableCollection<AbstractCard>();

            using (var connection = new SqliteConnection("Data Source=Database/Cards.db"))
            {
                connection.Open();
                string selectQuery = "SELECT * FROM Cards WHERE UserId = @UserId";

                using (var command = new SqliteCommand(selectQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string cardType = reader["CardType"].ToString();
                            AbstractCard cardString;

                            // Фабрика створення об'єктів залежно від типу
                            switch (cardType)
                            {
                                case "Credit":
                                    cardString = new CreditCard { CreditLimit = Convert.ToInt32(reader["CreditLimit"]) };
                                    break;
                                case "Debit":
                                    cardString = new DebitCard();
                                    break;
                                case "Unior":
                                    cardString = new UniorCard();
                                    break;
                                default:
                                    continue;
                            }

                            cardString.Id = Convert.ToInt32(reader["Id"]);
                            cardString.CardNumber = reader["CardNumber"].ToString();
                            cardString.Balance = Convert.ToDecimal(reader["Balance"]);
                            cardString.ExpirationDate = Convert.ToDateTime(reader["ExpirationDate"]);
                            cardString.CVV = Convert.ToInt16(reader["CVV"]);
                            cardString.UserId = userId;

                            cards.Add(cardString);
                        }
                    }
                }
            }
            return cards;
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
