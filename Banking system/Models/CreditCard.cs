using System;
using System.Collections.Generic;
using System.Text;

namespace Banking_system.Models
{
    internal class CreditCard : AbstractCard
    {
        int CreditLimit { get; set; } = int.MaxValue;
        string CreditType { get; set; } = string.Empty;
        DateTime CreditEndDate { get; set; } = DateTime.MaxValue;
        int percentage { get; set; } = int.MaxValue;

    }
}
