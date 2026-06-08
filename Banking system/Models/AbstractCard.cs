using System;
using System.Collections.Generic;
using System.Text;

namespace Banking_system.Models
{
    internal abstract class AbstractCard
    {
        public string CardNumber { get; set; } = string.Empty;
        private short CVV { get; set; } = short.MaxValue;
        private DateTime ExpirationDate { get; set; } = DateTime.MaxValue;

        public AbstractCard() { }
        public AbstractCard(string cardNumber, short cVV, DateTime expirationDate)
        {
            CardNumber = cardNumber;
            CVV = cVV;
            ExpirationDate = expirationDate;
        }

        public short GetCVV() { return CVV; }
        public DateTime GetExpirationDate() { return ExpirationDate; }
    }
}
