namespace Banking_system.Models.Transactions
{
    internal class WithdrawTransaction : AbstractTransaction
    {
        private AbstractCard _sourceCard = null!;
       
        protected WithdrawTransaction() { }
        public WithdrawTransaction(AbstractCard sourceCard, decimal amount) : base(amount)=>_sourceCard = sourceCard;
        public override bool Execute()
        {
            using (var db = new Banking_system.DataBase.Database())
            {
                var card = db.Cards.FirstOrDefault(c => c.CardNumber == _sourceCard.CardNumber);

                if (card != null && card.Withdraw(Amount))
                {
                    card.Operation(this);
                    db.SaveChanges();

                    return true;
                }
                return false;
            }
        }
    }
}