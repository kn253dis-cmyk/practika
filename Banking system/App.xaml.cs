using System.Configuration;
using System.Data;
using System.Windows;

namespace Banking_system
{
    public partial class App : Application
    {
        public App()
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
