using System;
using System.Collections.Generic;
using System.Text;

namespace Banking_system.Models
{
    internal class DebitCard : AbstractCard
    {
        double Balance { get; set; } = double.MaxValue;
    }
}
