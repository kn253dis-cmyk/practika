using System;

namespace Banking_system.Models.Transactions
{
    internal class DepositTransaction : AbstractTransaction
    {
        private AbstractCard _targetCard;

        public DepositTransaction(AbstractCard targetCard, decimal amount) : base(amount)
        {
            _targetCard = targetCard;
        }

        public override bool Execute()
        {
            try
            {
                _targetCard.Deposit(Amount);
                Logger.Log($"[Транзакція {TransactionId}] Успішне поповнення. Картка: {_targetCard.CardNumber}. Сума: {Amount:C}.");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"[Транзакція {TransactionId}] Помилка поповнення: {ex.Message}");
                return false;
            }
        }
    }
}