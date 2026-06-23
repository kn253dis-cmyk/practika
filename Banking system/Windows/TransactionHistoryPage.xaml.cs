using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Banking_system.Models;
using MaterialDesignThemes.Wpf;

namespace Banking_system.Views
{
    public partial class TransactionHistoryPage : Page
    {
        private readonly string _currentUserEmail;

        private List<JsonLog.LogEntry> _allTransactions;

        private bool _isSortDescending = true;

        public TransactionHistoryPage(string userEmail)
        {
            InitializeComponent();
            _currentUserEmail = userEmail;
            LoadTransactions();
        }

        private void LoadTransactions()
        {
            var rawLogs = Logger.ReadUserLogs(_currentUserEmail);

            if (rawLogs != null)
            {
                _allTransactions = rawLogs.ToList();
            }
            else
            {
                _allTransactions = new List<JsonLog.LogEntry>();
            }

            ApplyFilterAndSort(); 
        }


        private void BtnToggleSort_Click(object sender, RoutedEventArgs e)
        {

            _isSortDescending = !_isSortDescending;

            if (_isSortDescending)
            {
                IconSort.Kind = PackIconKind.ArrowDown;
                BtnToggleSort.ToolTip = "Спочатку нові";
            }
            else
            {
                IconSort.Kind = PackIconKind.ArrowUp;
                BtnToggleSort.ToolTip = "Спочатку старі";
            }

            ApplyFilterAndSort();
        }

        private void DpSearchDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            BtnClearDate.Visibility = DpSearchDate.SelectedDate.HasValue
                ? Visibility.Visible
                : Visibility.Collapsed;

            ApplyFilterAndSort();
        }

        private void BtnClearDate_Click(object sender, RoutedEventArgs e)
        {
            DpSearchDate.SelectedDate = null;
        }

        private void ApplyFilterAndSort()
        {
            if (_allTransactions == null) return;

            var filteredList = _allTransactions.AsEnumerable();

            if (DpSearchDate.SelectedDate.HasValue)
            {
                DateTime targetDate = DpSearchDate.SelectedDate.Value.Date;

                filteredList = filteredList.Where(t => t.Date.Date == targetDate);
            }

            if (_isSortDescending)
            {
                filteredList = filteredList.OrderByDescending(t => t.Date);
            }
            else
            {
                filteredList = filteredList.OrderBy(t => t.Date);
            }

            TransactionsList.ItemsSource = filteredList.ToList();
        }


        private void TransactionsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TransactionsList.SelectedItem is JsonLog.LogEntry selectedTransaction)
            {
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