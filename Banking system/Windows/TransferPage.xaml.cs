using Banking_system.Service;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
            string purposeCode = CmbPurposeCode.Text.Trim();
            string comment = TxtComment.Text;

            string fullPurpose = $"{purposeCode}. Коментар: {comment}";

            if (!decimal.TryParse(TxtAmount.Text, out decimal amount))
            {
                MessageBox.Show("Будь ласка, введіть коректну суму цифрами!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool isSuccess = BankingOperationService.ExecuteTransfer(_senderCardNumber, receiverCard, amount, purposeCode);

            if (isSuccess)
            {
                MessageBox.Show("Переказ успішно виконано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);


                Window.GetWindow(this)?.Close();
            }
            else
                MessageBox.Show("Не вдалося виконати переказ. Перевірте баланс та номер картки отримувача.", "Помилка переказу", MessageBoxButton.OK, MessageBoxImage.Error);

            MessageBox.Show($"Ініційовано переказ на суму {amount} ₴.\nПризначення: {fullPurpose}", "Переказ");
        }
    }
}