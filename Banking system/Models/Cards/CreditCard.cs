using System;

namespace Banking_system.Models
{

    public enum CardAction { None, SendWarning, InterestApplied, Blocked, Blacklisted }

    public class CardProcessResult
    {
        public CardAction Action { get; set; } = CardAction.None;
        public decimal InterestAmount { get; set; } = 0;
        public int MonthsLeft { get; set; } = 0;
    }

    public class CreditCard : AbstractCard
    {
        public int CreditLimit { get; set; } = 50000;
        public string CreditType { get; set; } = "Стандартний";
        public decimal InterestRate { get; set; } = 8.0m;


        public int PlanDurationMonths { get; set; } = 3;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime TermEndDate { get; set; } // Коли закінчується дія плану
        public DateTime DueDate { get; set; } // Дата наступного штрафу
        public DateTime LastWarningSentDate { get; set; } = DateTime.MinValue;

        public int MissedPaymentsCount { get; set; } = 0;
        public int InterestAppliedCount { get; set; } = 0; // Скільки разів нарахували штраф
        public bool IsBlocked { get; set; } = false;

        public CreditCard() : base("30")
        {
            Logger.Log($"Створено кредитну картку {CardNumber}.");
        }

        public override bool Withdraw(decimal amount)
        {
            if (IsBlocked) return false;
            if (amount <= 0) return false;

            decimal currentDebt = Balance < 0 ? Math.Abs(Balance) : 0;

            if (currentDebt + amount <= CreditLimit)
            {
                Balance -= amount;
                Logger.Log($"З кредитки {CardNumber} знято {amount:C}. Баланс: {Balance:C}");
                return true;
            }

            return false;
        }

        public override void Deposit(decimal amount)
        {
            if (amount <= 0) return;

            base.Deposit(amount);

            // Якщо борг погашено
            if (Balance >= 0)
            {
                IsBlocked = false;
                MissedPaymentsCount = 0;
                InterestAppliedCount = 0;
                DueDate = DateTime.Now.AddMonths(1);
                TermEndDate = DateTime.Now.AddMonths(PlanDurationMonths);
            }
        }

        public CardProcessResult ProcessRoutine()
        {
            var result = new CardProcessResult();

            if (Balance >= 0) return result; 

            // ЧОРНИЙ СПИСОК 
            if (DateTime.Now >= TermEndDate.AddMonths(1))
            {
                IsBlocked = true;
                result.Action = CardAction.Blacklisted;
                return result; 
            }

            // БЛОКУВАННЯ КАРТКИ 
            if (DateTime.Now >= TermEndDate && !IsBlocked)
            {
                IsBlocked = true;
                result.Action = CardAction.Blocked;
            }

            // НАРАХУВАННЯ ВІДСОТКІВ
            if (DateTime.Now >= DueDate)
            {
                if (InterestAppliedCount < PlanDurationMonths)
                {
                    decimal interest = CreditLimit * (InterestRate / 100m);
                    Balance -= interest;

                    InterestAppliedCount++;
                    MissedPaymentsCount++;
                    DueDate = DueDate.AddMonths(1);

                    result.Action = CardAction.InterestApplied;
                    result.InterestAmount = interest;
                    return result;
                }
            }

            // ПОПЕРЕДЖЕННЯ ЗA 7 ДНІВ 
            if (!IsBlocked && DateTime.Now < DueDate)
            {
                TimeSpan timeToDue = DueDate - DateTime.Now;

                if (timeToDue.TotalDays <= 7 && timeToDue.TotalDays >= 0)
                {
                    if (LastWarningSentDate.Month != DateTime.Now.Month || LastWarningSentDate.Year != DateTime.Now.Year)
                    {
                        LastWarningSentDate = DateTime.Now;
                        result.Action = CardAction.SendWarning;
                        result.MonthsLeft = PlanDurationMonths - InterestAppliedCount;
                        return result;
                    }
                }
            }

            return result;
        }
    }
}