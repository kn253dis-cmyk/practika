using System;

namespace Banking_system.Models
{
    public class CreditCard : AbstractCard
    {
        public int CreditLimit { get; set; } = 10000;
        public string CreditType { get; set; } = "Standard"; // Назва тарифу
        public decimal InterestRate { get; set; } = 5.0m; // Відсоток
        public decimal AccruedInterest { get; set; } = 0m; // Окремий рахунок для збереження нарахованих відсотків

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; } = DateTime.Now.AddMonths(1); // Дата погашення боргу 
        public DateTime LastReminderSentDate { get; set; } = DateTime.Now.AddDays(-30);

        public int MissedPaymentsCount { get; set; } = 0; // Скільки місяців прострочено
        public bool IsBlocked { get; set; } = false; // Чи заблоковано картку

        public CreditCard() : base("30")
        {
            Logger.Log($"Створено кредитну картку {CardNumber}.");
        }

        public override bool Withdraw(decimal amount)
        {
            if (IsBlocked) return false;
            if (amount <= 0) return false;


            decimal currentDebt = Balance < 0 ? Math.Abs(Balance) : 0;


            if (currentDebt + amount <= CreditLimit)
            {
                Balance -= amount;
                Logger.Log($"З кредитки {CardNumber} знято {amount:C}. Баланс: {Balance:C}");
                return true;
            }

            return false;
        }

        public override void Deposit(decimal amount)
        {
            if (amount <= 0) return;

            base.Deposit(amount);

            if (Balance >= 0)
            {
                IsBlocked = false;
                MissedPaymentsCount = 0;
                DueDate = DateTime.Now.AddMonths(1); 
            }
        }
    }
}