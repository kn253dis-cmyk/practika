using System;

namespace Banking_system.Models
{
    internal class CreditCard : AbstractCard
    {
        public int CreditLimit { get; set; } = 50000;
        public string CreditType { get; set; } = "Універсальна";
        public DateTime CreditEndDate { get; set; } = DateTime.Now.AddYears(1);
        public decimal Percentage { get; set; } = 3.5m; // Відсотки краще тримати в decimal

        public CreditCard() : base("30")
        {
            Logger.Log($"Ініціалізовано кредитну картку {CardNumber} з лімітом {CreditLimit:C}.");
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount <= 0) return false;

            if (Balance + CreditLimit >= amount)
            {
                Balance -= amount;
                Logger.Log($"З кредитної картки {CardNumber} знято {amount:C}. Баланс: {Balance:C}");
                return true;
            }

            Logger.Log($"Відмова ({CardNumber}): Перевищено кредитний ліміт під час спроби зняти {amount:C}.");
            return false;
        }
    }
}