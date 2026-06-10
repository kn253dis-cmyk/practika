namespace Banking_system.Models.Transactions
{
    internal class TransferTransaction : AbstractTransaction
    {
        private AbstractCard _sourceCard;
        private AbstractCard _targetCard;

        public TransferTransaction(AbstractCard sourceCard, AbstractCard targetCard, decimal amount) : base(amount)
        {
            _sourceCard = sourceCard;
            _targetCard = targetCard;
        }

        public override bool Execute()
        {
            if (_sourceCard.CardNumber == _targetCard.CardNumber)
            {
                Logger.Log($"[Транзакція {TransactionId}] Помилка: Спроба переказу на ту саму картку ({_sourceCard.CardNumber}).");
                return false;
            }

            Logger.Log($"[Транзакція {TransactionId}] Ініційовано переказ {Amount:C} з картки {_sourceCard.CardNumber} на {_targetCard.CardNumber}.");

            bool isWithdrawn = _sourceCard.Withdraw(Amount);

            if (isWithdrawn)
            {
                _targetCard.Deposit(Amount);
                Logger.Log($"[Транзакція {TransactionId}] Переказ успішно завершено.");
                return true;
            }
            else
            {
                Logger.Log($"[Транзакція {TransactionId}] Переказ скасовано через проблеми зі зняттям коштів з картки {_sourceCard.CardNumber}.");
                return false;
            }
        }
    }
}