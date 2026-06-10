namespace Banking_system.Models.Transactions
{
    internal class WithdrawalTransaction : AbstractTransaction
    {
        private AbstractCard _sourceCard;

        public WithdrawalTransaction(AbstractCard sourceCard, decimal amount) : base(amount)=>_sourceCard = sourceCard;
        public override bool Execute()
        {
            Logger.Log($"[Транзакція {TransactionId}] Запит на зняття {Amount:C} з картки {_sourceCard.CardNumber}.");

            bool success = _sourceCard.Withdraw(Amount);

            if (success)
                Logger.Log($"[Транзакція {TransactionId}] Успішно знято {Amount:C} з картки {_sourceCard.CardNumber}.");
            else
                Logger.Log($"[Транзакція {TransactionId}] Відхилено. Недостатньо коштів або перевищено ліміт на картці {_sourceCard.CardNumber}.");

            return success;
        }
    }
}