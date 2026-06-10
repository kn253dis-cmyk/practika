namespace Banking_system.Models
{
    public class JuniorCard : AbstractCard
    {
      
        public decimal TransactionLimit { get; set; } = 5000m;

        public JuniorCard() : base("20")
        {
            Logger.Log($"Ініціалізовано картку юніора {CardNumber} з лімітом операцій {TransactionLimit:C}.");
        }

        public override bool Withdraw(decimal amount)
        {
            if (amount > TransactionLimit)
            {
                Logger.Log($"Відмова ({CardNumber}): Спроба зняти {amount:C} перевищує встановлений ліміт для юніора ({TransactionLimit:C}).");
                return false;
            }
            return base.Withdraw(amount);
        }
    }
}