using System;
using System.Windows.Controls;
using System.Windows.Input;
using Banking_system.Models;
using Banking_system.Helpers; // Поки залишаємо Logger, потім замінимо на DB сервіс

namespace Banking_system.Views
{
    public partial class TransactionHistoryPage : Page
    {
        // TODO: Пізніше сюди буде передаватися пошта реально авторизованого користувача з БД
        private readonly string _currentUserEmail = "client@gmail.com";

        public TransactionHistoryPage()
        {
            InitializeComponent();
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            // TODO: Замінити Logger на запит до бази даних (наприклад _dbContext.Transactions.Where(...))
            TransactionsList.ItemsSource = Logger.ReadUserLogs(_currentUserEmail);
        }

        private void TransactionsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TransactionsList.SelectedItem is JsonLog.LogEntry selectedTransaction)
            {
                var confirmationWindow = new ReceiptConfirmationWindow(selectedTransaction);
                confirmationWindow.ShowDialog();
            }
        }
    }
}