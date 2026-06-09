using System;
using System.Collections.Generic;
using System.Text;

namespace Banking_system.Models
{
    abstract class AbstractTransaction
    {

        protected decimal amount { get; set; } = decimal.Zero;
        public string description { get; set; } = string.Empty;
        protected string fromAccount { get; set; } = string.Empty;
        protected string toAccount { get; set; } = string.Empty;
        public DateTime date { get; set; } = DateTime.Now;
        public string transactionId { get; set; } = Guid.NewGuid().ToString();
        protected void Transfer(AbstractCard FromCard , AbstractCard ToCard , decimal amount) {
            if (FromCard.Balance >= amount)
            {
                FromCard.Withdraw(-amount);
                ToCard.Withdraw(amount);
            }
            else
                throw new InvalidOperationException("Insufficient funds in the source account.");
        }
    }
}
