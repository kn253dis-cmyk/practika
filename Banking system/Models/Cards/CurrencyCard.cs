using System;
using System.Collections.Generic;

namespace Banking_system.Models
{
    public class CurrencyCard : AbstractCard
    {
        public string CurrencyType { get; set; } = string.Empty;

        // Список підтримуваних валют
        private List<string> SupportedCurrencies = new List<string> { "USD", "EUR", "PLN", "GBP", "XAU", "XAG" };

        public CurrencyCard() : base("20") => Logger.Log($"Ініціалізовано валютну картку {CardNumber}.");

        
        public CurrencyCard(string currencyType) : base("20")
        {
            if (SupportedCurrencies.Contains(currencyType.ToUpper()))
                CurrencyType = currencyType.ToUpper();
            else
                throw new ArgumentException("Непідтримуваний тип валюти.");

            Logger.Log($"Створено нову валютну картку {CardNumber} ({CurrencyType}).");
        }
    }
}