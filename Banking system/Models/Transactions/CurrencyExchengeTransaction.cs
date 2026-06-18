using System;
using System.Collections.Generic;
using System.Linq;

namespace Banking_system.Models.Transactions
{
    public class CurrencyExchangeTransaction : AbstractTransaction
    {
        private readonly string _sourceCardNumber = string.Empty;
        private readonly string _targetCardNumber = string.Empty;
        private readonly decimal _exchangeRate = decimal.Zero;
        private readonly string _currencyCode = string.Empty; 

        public CurrencyExchangeTransaction(string sourceCardNumber, string targetCardNumber, decimal foreignAmount, decimal exchangeRate, string currencyCode)
            : base(foreignAmount)
        {
            _sourceCardNumber = sourceCardNumber;
            _targetCardNumber = targetCardNumber;
            _exchangeRate = exchangeRate;
            _currencyCode = currencyCode;
        }
        protected CurrencyExchangeTransaction() { }
        public override bool Execute()
        {
            // Перевірка на коректність даних
            if (Amount <= 0 || _exchangeRate <= 0 || _sourceCardNumber == _targetCardNumber) return false;

            using (var db = new DataBase.Database())
            {
                var sourceCard = db.Cards.FirstOrDefault(c => c.CardNumber == _sourceCardNumber);
                var targetCard = db.Cards.FirstOrDefault(c => c.CardNumber == _targetCardNumber);

                var user = sourceCard != null ? db.Users.FirstOrDefault(u => u.ID == sourceCard.UserId) : null;

                if (sourceCard == null || targetCard == null || user == null)
                {
                    Logger.Log($"[Транзакція {TransactionId}] Помилка: картку або користувача не знайдено.");
                    return false;
                }

                // Розраховуємо еквівалент у гривнях (Amount береться з AbstractTransaction)
                decimal amountInUah = Amount * _exchangeRate;

                // 1. ПРОДАЖ ВАЛЮТИ: Валютна картка -> Гривнева (Дебетова)
                if (sourceCard is CurrencyCard sourceCurrencyCard && targetCard is DebitCard)
                {
                    // Перевірка, чи відповідає валюта транзакції валюті картки
                    if (!string.IsNullOrEmpty(sourceCurrencyCard.CurrencyType) && sourceCurrencyCard.CurrencyType != _currencyCode)
                    {
                        Logger.Log($"[Транзакція {TransactionId}] Помилка: Спроба продати {_currencyCode} з картки типу {sourceCurrencyCard.CurrencyType}.");
                        return false;
                    }

                    // Знімаємо валюту
                    if (sourceCurrencyCard.Withdraw(Amount))
                    {
                        // Зараховуємо гривні
                        targetCard.Deposit(amountInUah);
                        db.SaveChanges();

                        Logger.Log($"[Транзакція {TransactionId}] Успішний продаж: знято {Amount} {_currencyCode}, зараховано {amountInUah:F2} ₴.");
                        LogExchangeReceipt(user.Email, "Продаж валюти", Amount, amountInUah, sourceCard.CardNumber, targetCard.CardNumber);
                        return true;
                    }
                    else
                    {
                        Logger.Log($"[Транзакція {TransactionId}] Відхилено. Недостатньо коштів на валютній картці.");
                        return false;
                    }
                }

                // 2. КУПІВЛЯ ВАЛЮТИ: Гривнева картка (Дебетова) -> Валютна
                else if (sourceCard is DebitCard && targetCard is CurrencyCard targetCurrencyCard)
                {
                    // КРИТИЧНА ПЕРЕВІРКА: чи не намагаються купити іншу валюту на цю картку
                    if (!string.IsNullOrEmpty(targetCurrencyCard.CurrencyType) && targetCurrencyCard.CurrencyType != _currencyCode)
                    {
                        Logger.Log($"[Транзакція {TransactionId}] Відхилено: Спроба зарахувати {_currencyCode} на картку, яка вже прив'язана до {targetCurrencyCard.CurrencyType}.");
                        return false; // Жорстка заборона
                    }

                    // Знімаємо гривні
                    if (sourceCard.Withdraw(amountInUah))
                    {
                        // ЯКЩО КАРТКА НОВА І БЕЗ ВАЛЮТИ - НАЗАВЖДИ ПРИВ'ЯЗУЄМО ЇЙ ТИП ВАЛЮТИ
                        if (string.IsNullOrEmpty(targetCurrencyCard.CurrencyType))
                        {
                            targetCurrencyCard.CurrencyType = _currencyCode;
                            Logger.Log($"[Транзакція {TransactionId}] Валютній картці {targetCurrencyCard.CardNumber} встановлено тип валюти: {_currencyCode}.");
                        }

                        // Зараховуємо валюту
                        targetCurrencyCard.Deposit(Amount);
                        db.SaveChanges();

                        Logger.Log($"[Транзакція {TransactionId}] Успішна купівля: знято {amountInUah:F2} ₴, зараховано {Amount} {_currencyCode}.");
                        LogExchangeReceipt(user.Email, "Купівля валюти", Amount, amountInUah, sourceCard.CardNumber, targetCard.CardNumber);
                        return true;
                    }
                    else
                    {
                        Logger.Log($"[Транзакція {TransactionId}] Відхилено. Недостатньо коштів на гривневій картці.");
                        return false;
                    }
                }

                else
                {
                    Logger.Log($"[Транзакція {TransactionId}] Помилка: Обмін можливий лише між CurrencyCard та DebitCard.");
                    return false;
                }
            }
        }

        // Приватний метод для генерації електронної квитанції для Історії Транзакцій
        private void LogExchangeReceipt(string email, string operationName, decimal amountForeign, decimal amountUah, string source, string target)
        {
            var receiptData = new Dictionary<string, string>
            {
                { "Amount", amountUah.ToString("F2") },
                { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                { "SenderCard", source },
                { "ReceiverCard", target },
                { "Purpose", $"{operationName}. Сума: {amountForeign} {_currencyCode}. Курс: {_exchangeRate:F2}" }
            };

            Logger.AppendLog(
                userEmail: email,
                templateName: "TransferReceipt",
                text: $"{operationName}: {amountForeign} {_currencyCode} за {amountUah:F2} ₴",
                data: receiptData
            );
        }
    }
}