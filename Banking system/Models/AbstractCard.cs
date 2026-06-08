using System;

namespace Banking_system.Models
{
    internal abstract class AbstractCard
    {
        public string CardNumber { get; protected set; } = string.Empty;
        public decimal Balance { get; protected set; } = 0m;
        private short CVV { get; set; }
        private DateTime ExpirationDate { get; set; }

        public AbstractCard() { }

        public AbstractCard(string code)
        {
            CardNumber = GenerateCardNumber(code);
            Random rnd = new Random();
            ExpirationDate = DateTime.Now.AddYears(5);
            CVV = (short)rnd.Next(100, 999);

            Logger.Log($"Створено нову картку. Номер: {CardNumber}");
        }

        public short GetCVV() => CVV;
        public DateTime GetExpirationDate() => ExpirationDate;

        public virtual void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                Logger.Log($"Помилка поповнення ({CardNumber}): Сума має бути більшою за нуль.");
                throw new ArgumentException("Сума поповнення має бути більшою за 0.");
            }

            Balance += amount;
            Logger.Log($"Картку {CardNumber} поповнено на {amount:C}. Новий баланс: {Balance:C}");
        }

        public virtual bool Withdraw(decimal amount)
        {
            if (amount <= 0) return false;

            if (Balance >= amount)
            {
                Balance -= amount;
                Logger.Log($"З картки {CardNumber} знято {amount:C}. Залишок: {Balance:C}");
                return true;
            }

            Logger.Log($"Відмова ({CardNumber}): Недостатньо коштів для зняття {amount:C}. Баланс: {Balance:C}");
            return false;
        }

        protected string GenerateCardNumber(string cardTypeCode)
        {
            string countryCode = "38";
            string bankCode = "7777";

            Random rnd = new Random();
            string uniqueAccountPart = rnd.Next(1000000, 9999999).ToString();
            string first15Digits = countryCode + cardTypeCode + bankCode + uniqueAccountPart;
            int checkDigit = CalculateLuhnCheckDigit(first15Digits);

            return first15Digits + checkDigit.ToString();
        }

        private int CalculateLuhnCheckDigit(string number)
        {
            int sum = 0;
            bool alternate = true;

            for (int i = number.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(number[i].ToString());
                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n -= 9;
                }
                sum += n;
                alternate = !alternate;
            }
            return (10 - (sum % 10)) % 10;
        }
    }
}