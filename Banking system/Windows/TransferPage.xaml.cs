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
            try
            {
                if (!decimal.TryParse(TxtAmount.Text, out decimal amount) || amount <= 0)
                {
                    MessageBox.Show("Будь ласка, введіть коректну суму цифрами більшу за нуль!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string receiverCard = TxtReceiverCard.Text.Trim();
                string purposeCode = CmbPurposeCode.Text.Trim();
                string comment = TxtComment.Text.Trim();

                if (string.IsNullOrWhiteSpace(receiverCard))
                {
                    MessageBox.Show("Будь ласка, введіть номер картки отримувача!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Визначаємо призначення платежу
                string? selectedPurpose = CmbPurposeCode.SelectedItem is ComboBoxItem item
                    ? item.Content.ToString()
                    : purposeCode;

                string fullPurpose = $"{selectedPurpose}. Коментар: {comment}";

                // Створюємо транзакцію з правильними 5 параметрами
                var transfer = new Banking_system.Models.Transactions.TransferTransaction(
                    _senderCardNumber, receiverCard, amount, selectedPurpose ?? string.Empty, comment);

                bool isSuccess = transfer.Execute();

                // Обробка результату
                if (isSuccess)
                {
                    MessageBox.Show($"Переказ на суму {amount} ₴ успішно виконано!\nПризначення: {fullPurpose}", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    MessageBox.Show("Не вдалося виконати переказ. Перевірте баланс та номер картки отримувача.", "Помилка переказу", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Виникла непередбачена помилка під час обробки переказу:\n{ex.Message}", "Системна помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                Models.Logger.Log($"Помилка в BtnSubmitTransfer_Click: {ex.Message}");
            }
        }
    }
}