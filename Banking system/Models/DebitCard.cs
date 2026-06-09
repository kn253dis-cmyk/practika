using System;
using System.Collections.Generic;
using System.Text;

namespace Banking_system.Models
{
    internal class DebitCard : AbstractCard
    {
        public DebitCard() : base("10")
        {
            Logger.Log($"Ініціалізовано дебетову картку {CardNumber}.");
        }
    }
}
