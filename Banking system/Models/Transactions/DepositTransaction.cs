using System;

namespace Banking_system.Models.Transactions
{
    internal class DepositTransaction : AbstractTransaction
    {
        private AbstractCard _targetCard = null!;
        protected DepositTransaction() { }
        public DepositTransaction(AbstractCard targetCard, decimal amount) : base(amount)
        {
            _targetCard = targetCard;
        }

        public override bool Execute()
        {
            using (var db = new Banking_system.DataBase.Database())
            {
                var card = db.Cards.FirstOrDefault(c => c.CardNumber == _targetCard.CardNumber);
                if (card != null)
                {
                    card.Deposit(Amount);
                    card.Operation(this);

                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }
    }
}