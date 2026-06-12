using Banking_system.Models.Transactions;
using System;
using System.Transactions;
using System.Windows.Media;

namespace Banking_system.Models
{
    public abstract class AbstractCard
    {
        public virtual ICollection<AbstractTransaction> LastTransactions { get; set; } = new List<AbstractTransaction>();
        public int Id { get; set; }
        public int UserId { get; set; }
        public string CardNumber { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0m;
        public short CVV { get; set; }
        public DateTime ExpirationDate { get; set; }
        
        public AbstractCard() { }

        public AbstractCard(string code)
        {
            CardNumber = GenerateCardNumber(code);
            Random rnd = new Random();
            ExpirationDate = DateTime.Now.AddYears(5);
            CVV = (short)rnd.Next(100, 999);

            Logger.Log($"Створено нову картку. Номер: {CardNumber}");
        }

        public AbstractCard(string cardNumber, decimal balance, short cvv, DateTime expirationDate)
        {
            CardNumber = cardNumber;
            Balance = balance;
            CVV = cvv;
            ExpirationDate = expirationDate;

            Logger.Log($"Ініціалізовано картку {CardNumber} з балансом {Balance:C}, CVV: {CVV}, терміном дії до {ExpirationDate:MM/yyyy}");
        }

        public void Operation(AbstractTransaction transaction)
        {
            if (transaction == null) return;

            if (transaction is DepositTransaction) transaction = (DepositTransaction)transaction;
            else if (transaction is WithdrawTransaction) transaction = (WithdrawTransaction)transaction;
            else transaction = (TransferTransaction) transaction;
            using (var db = new DataBase.Database())
            {
                db.transactions.Add(transaction);
                db.SaveChanges();
            }
            LastTransactions.Add(transaction);
        }

        public short GetCVV() => CVV;
        public DateTime GetExpirationDate() => ExpirationDate;
        public decimal GetBalance() => Balance;
        public string GetCardNumber() => CardNumber;

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