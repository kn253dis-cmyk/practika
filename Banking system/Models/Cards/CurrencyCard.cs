using System;
using System.Collections.Generic;

namespace Banking_system.Models
{
    public class CurrencyCard : AbstractCard
    {
        public string CurrencyType { get; set; } = string.Empty;
        private List<string> SupportedCurrencies = new List<string> { "USD", "EUR", "PLN", "GBP", "XAU", "XAG" };

        public CurrencyCard() : base("20")
        {
            Logger.Log($"Ініціалізовано валютну картку {CardNumber} з валютою {CurrencyType}.");
        }

        // Конструктор для створення карти з конкретною валютою
        public CurrencyCard(string currencyType) : base("20")
        {
            if (SupportedCurrencies.Contains(currencyType.ToUpper()))
                CurrencyType = currencyType.ToUpper();
            else
                throw new ArgumentException("Непідтримуваний тип валюти.");

            Logger.Log($"Створено нову валютну картку {CardNumber} ({CurrencyType}).");
        }

        /// <summary>
        /// Продаж валюти (переказ з валютної картки на гривневу)
        /// </summary>
        public bool ExchangeToUah(AbstractCard destinationCard, decimal amountInForeignCurrency, decimal exchangeRate)
        {
            if (amountInForeignCurrency <= 0) return false;

            if (!this.Withdraw(amountInForeignCurrency))
            {
                Logger.Log($"Відмова: недостатньо коштів на валютній картці {CardNumber} для обміну.");
                return false;
            }

            decimal amountInUah = amountInForeignCurrency * exchangeRate;

            destinationCard.Deposit(amountInUah);

            Logger.Log($"Обмін валют: знято {amountInForeignCurrency} {CurrencyType}, зараховано {amountInUah:F2} UAH на картку {destinationCard.CardNumber}. Курс: {exchangeRate}");
            return true;
        }

        /// <summary>
        /// Купівля валюти (переказ з гривневої картки на цю валютну)
        /// </summary>
        public bool BuyCurrencyFromUah(AbstractCard sourceCard, decimal amountInForeignCurrency, decimal exchangeRate)
        {
            if (amountInForeignCurrency <= 0) return false;

            decimal amountInUah = amountInForeignCurrency * exchangeRate;

            if (!sourceCard.Withdraw(amountInUah))
            {
                Logger.Log($"Відмова: недостатньо коштів на гривневій картці {sourceCard.CardNumber} для купівлі валюти.");
                return false;
            }

            this.Deposit(amountInForeignCurrency);

            Logger.Log($"Купівля валюти: знято {amountInUah:F2} UAH з {sourceCard.CardNumber}, зараховано {amountInForeignCurrency} {CurrencyType} на {CardNumber}. Курс: {exchangeRate}");
            return true;
        }
    }
}