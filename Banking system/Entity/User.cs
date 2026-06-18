using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Banking_system.Models;


namespace Banking_system.Entity
{
    public class User
    {
        [Key]
        public int ID { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Ipn { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string MiddleName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public bool IsMale { get; set; }
        public ICollection<AbstractCard> Cards { get; set; } = new List<AbstractCard>();

        public User() { }

        public User(string password, string phone, string email, string ipn, string name, string surname, string middleName , AbstractCard Card)
        {
            Password = password;
            Phone = phone;
            Email = email;
            Ipn = ipn;
            Name = name;
            Surname = surname;
            MiddleName = middleName;
            Cards.Add(Card);
        }
    }
}
