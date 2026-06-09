using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Banking_system.Models;

namespace Banking_system.Views
{
    public partial class TransactionHistoryPage : Page
    {
        private readonly string _currentUserEmail;

        // Тепер сторінка очікує пошту при створенні
        public TransactionHistoryPage(string userEmail)
        {
            InitializeComponent();
            _currentUserEmail = userEmail;
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            // Витягуємо логи виключно для поточної пошти
            TransactionsList.ItemsSource = Logger.ReadUserLogs(_currentUserEmail);
        }

        private void TransactionsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Якщо клікнули на лог, відкриваємо вікно квитанції
            if (TransactionsList.SelectedItem is JsonLog.LogEntry selectedTransaction)
            {
                var confirmationWindow = new ReceiptConfirmationWindow(selectedTransaction);
                confirmationWindow.ShowDialog();
            }
        }
    }
}