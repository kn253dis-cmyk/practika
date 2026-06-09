using System;
using System.Windows;
using Banking_system.Models;
using Banking_system.Service;

namespace Banking_system.Views
{
    public partial class ReceiptConfirmationWindow : Window
    {
        private readonly JsonLog.LogEntry _transactionLog;
        private readonly EmailService _emailService;

        public ReceiptConfirmationWindow(JsonLog.LogEntry transactionLog)
        {
            InitializeComponent();
            _transactionLog = transactionLog;
            _emailService = new EmailService();

            // Підставляємо дані з логу в текстові поля XAML
            TxtEmail.Text = _transactionLog.UserEmail;
            TxtInfo.Text = $"Тип операції: {_transactionLog.TemplateName}\nДеталі: {_transactionLog.Text}";
        }

        // Кнопка Скасувати
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Кнопка Надіслати
        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string targetEmail = TxtEmail.Text; // Беремо пошту з текстового поля, якщо користувач її змінив

                string htmlContent = _emailService.PrepareReceiptHtml(
                    _transactionLog.TemplateName,
                    _transactionLog.ReceiptData
                );

                string subject = $"Квитанція за операцією: {_transactionLog.Id}";

                await _emailService.SendEmailAsync(targetEmail, subject, htmlContent);

                MessageBox.Show("Квитанцію успішно надіслано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відправці: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}