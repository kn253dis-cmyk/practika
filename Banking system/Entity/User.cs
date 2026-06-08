using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Banking_system.Entity
{
    public class User
    {
        [Key]
        private int ID { get; set; }
        private string Password { get; set; } = string.Empty;
        private string Email { get; set; } = string.Empty;
        private string Name { get; set; } = string.Empty;
        private string Surname { get; set; } = string.Empty;
        private string MiddleName { get; set; } = string.Empty;
        private string CardNumber { get; set; } = string.Empty;

        public User() { }

        public User(string password, string email, string name, string surname, string middleName,string cardNumber)
        {
            Password = password;
            Email = email;
            Name = name;
            Surname = surname;
            MiddleName = middleName;
            CardNumber = cardNumber;
        }
    }
}
