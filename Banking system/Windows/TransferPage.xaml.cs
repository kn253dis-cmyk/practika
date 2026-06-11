using System;
using System.Windows;
using System.Windows.Controls;
using Banking_system.Service;

namespace Banking_system.Views
{
    public partial class TransferPage : Page
    {
        private readonly string _senderCardNumber;

        public TransferPage(string senderCardNumber)
        {
            InitializeComponent();
            _senderCardNumber = senderCardNumber;


            TxtSenderCard.Text = _senderCardNumber;
        }

        private void BtnSubmitTransfer_Click(object sender, RoutedEventArgs e)
        {
            string receiverCard = TxtReceiverCard.Text.Trim();
            string purpose = TxtPurpose.Text.Trim();


            if (!decimal.TryParse(TxtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму цифрами!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


           
            var transaction = new Banking_system.Models.Transactions.TransferTransaction(_senderCardNumber, receiverCard, amount, purpose);
            bool isSuccess = transaction.Execute();

            if (isSuccess)
            {
                MessageBox.Show("Переказ успішно виконано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);


                Window.GetWindow(this)?.Close();
            }
            else
            {
                MessageBox.Show("Не вдалося виконати переказ. Перевірте баланс та номер картки отримувача.", "Помилка переказу", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}