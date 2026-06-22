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

                decimal amountInUah = Amount * _exchangeRate;


                if (sourceCard is CurrencyCard sourceCurrencyCard && targetCard is DebitCard)
                {
                    if (!string.IsNullOrEmpty(sourceCurrencyCard.CurrencyType) && sourceCurrencyCard.CurrencyType != _currencyCode)
                    {
                        Logger.Log($"[Транзакція {TransactionId}] Помилка: Спроба продати {_currencyCode} з картки типу {sourceCurrencyCard.CurrencyType}.");
                        return false;
                    }

                    if (sourceCurrencyCard.Withdraw(Amount))
                    {
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

                else if (sourceCard is DebitCard && targetCard is CurrencyCard targetCurrencyCard)
                {
                    if (!string.IsNullOrEmpty(targetCurrencyCard.CurrencyType) && targetCurrencyCard.CurrencyType != _currencyCode)
                    {
                        Logger.Log($"[Транзакція {TransactionId}] Відхилено: Спроба зарахувати {_currencyCode} на картку, яка вже прив'язана до {targetCurrencyCard.CurrencyType}.");
                        return false; 
                    }

                    if (sourceCard.Withdraw(amountInUah))
                    {
                        if (string.IsNullOrEmpty(targetCurrencyCard.CurrencyType))
                        {
                            targetCurrencyCard.CurrencyType = _currencyCode;
                            Logger.Log($"[Транзакція {TransactionId}] Валютній картці {targetCurrencyCard.CardNumber} встановлено тип валюти: {_currencyCode}.");
                        }

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