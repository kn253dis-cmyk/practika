using System;
using System.Collections.Generic;
using System.Text;

namespace Banking_system.Models.Transactions
{
    internal abstract class AbstractTransaction
    {
        public Guid TransactionId { get; protected set; }
        public DateTime Date { get; protected set; }
        public decimal Amount { get; protected set; }

        public AbstractTransaction(decimal amount)
        {
            TransactionId = Guid.NewGuid();
            Date = DateTime.Now;
            Amount = amount;
        }
        public abstract bool Execute();
    }
}
