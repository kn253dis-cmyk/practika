using System.Configuration;
using System.Data;
using System.Windows;

namespace Banking_system
{
    public partial class App : Application
    {
        public App()
        {
            // Цей рядок каже PostgreSQL приймати DateTime.Now без помилок
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        }
    }
}
