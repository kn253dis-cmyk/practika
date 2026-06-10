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
            if (TransactionsList.SelectedItem is JsonLog.LogEntry selectedTransaction)
            {
                // Якщо це системне повідомлення — показуємо просте вікно та перериваємо метод
                if (selectedTransaction.TemplateName == "SystemLog")
                {
                    MessageBox.Show("Для цієї системної дії електронна квитанція недоступна.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirmationWindow = new ReceiptConfirmationWindow(selectedTransaction);
                confirmationWindow.ShowDialog();
            }
        }
    }
}