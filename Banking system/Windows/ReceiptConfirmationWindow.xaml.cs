using System;
using System.Windows;
using Banking_system.Models;
using Banking_system.Service;

namespace Banking_system.Views
{
    public partial class ReceiptConfirmationWindow : Window
    {
        private readonly JsonLog.LogEntry _transactionData;

        public ReceiptConfirmationWindow(JsonLog.LogEntry transactionData)
        {
            InitializeComponent();
            _transactionData = transactionData;

            TxtInfo.Text = $"Операція: {_transactionData.Text}\nДата: {_transactionData.Date:dd.MM.yyyy HH:mm}";

            // За замовчуванням підставляємо пошту користувача
            TxtEmail.Text = _transactionData.UserEmail;
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                MessageBox.Show("Будь ласка, введіть коректну адресу.", "Перевірка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // TODO: В майбутньому тут можна додати перевірку наявності транзакції в БД
                var emailService = new EmailService();
                string htmlContent = emailService.PrepareReceiptHtml(_transactionData.TemplateName, _transactionData.ReceiptData);
                string emailSubject = $"Квитанція: {_transactionData.Text}";

                await emailService.SendEmailAsync(email, emailSubject, htmlContent);

                MessageBox.Show("Квитанцію успішно надіслано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}