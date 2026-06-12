namespace Banking_system.Models
{
    public class CurrencyCard : AbstractCard
    {
      
        public string CurrencyType { get; set; } = string.Empty;
        List<string> SupportedCurrencies = new List<string> { "USD", "EUR", "PLN", "GBP", "XAU", "XAG" };

        public CurrencyCard() : base("20")
        {
            Logger.Log($"Ініціалізовано валютну картку {CardNumber} з валютою {CurrencyType}.");
        }
        //public override bool Withdraw(decimal amount , string currencyType , string fromCard)
        //{
            
        //}
}
}