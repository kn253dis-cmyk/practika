using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Banking_system.Models.Transactions
{
    public abstract class AbstractTransaction
    {
        [Key]
        public int id { get; set; }
        public Guid TransactionId { get; protected set; }
        public DateTime Date { get; protected set; }
        public decimal Amount { get; protected set; }
        public string TransactionTarget { get; protected set; } = string.Empty;
        public string Description { get; protected set; } = string.Empty;
        public AbstractTransaction() { }

        public AbstractTransaction(decimal amount)
        {
            TransactionId = Guid.NewGuid();
            Date = DateTime.Now;
            Amount = amount;
        }
        public AbstractTransaction(decimal amount, string transactionTarget, string description)
        {
            TransactionId = Guid.NewGuid();
            Date = DateTime.Now;
            Amount = amount;
            TransactionTarget = transactionTarget;
            Description = description;
        }
        public abstract bool Execute();
    }
}
