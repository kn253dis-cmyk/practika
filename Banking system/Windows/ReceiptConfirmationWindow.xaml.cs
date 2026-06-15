using Banking_system.Models;
using Banking_system.Service;
using System;
using System.Windows;
using System.Windows.Input;

namespace Banking_system.Views
{
    public partial class ReceiptConfirmationWindow : Window
    {
        private JsonLog.LogEntry _transactionLog;
        private EmailService _emailService;
        private string? _readyHtmlContent;

        public ReceiptConfirmationWindow(JsonLog.LogEntry log)
        {
            InitializeComponent();

            _transactionLog = log;
            _emailService = new EmailService();
            TxtEmail.Text = log.UserEmail ?? "";

            ShowPreview();
        }

        private async void ShowPreview()
        {
            try
            {
                _readyHtmlContent = _emailService.PrepareReceiptHtml(
                    _transactionLog.TemplateName,
                    _transactionLog.ReceiptData
                );

                await ReceiptBrowser.EnsureCoreWebView2Async(null);

                ReceiptBrowser.ZoomFactor = 0.8;

                ReceiptBrowser.NavigateToString(_readyHtmlContent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка генерації перегляду: {ex.Message}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            string targetEmail = TxtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(targetEmail) || !targetEmail.Contains("@"))
            {
                MessageBox.Show("Будь ласка, введіть коректну адресу.", "Перевірка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(_readyHtmlContent))
            {
                MessageBox.Show("Квитанція не згенерована або сталася помилка при попередньому перегляді.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                BtnSend.IsEnabled = false; 

                string subject = $"Квитанція за операцією: {_transactionLog.Id}";

                await _emailService.SendEmailAsync(targetEmail, subject, _readyHtmlContent!);

                MessageBox.Show("Квитанцію успішно надіслано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при відправці: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                BtnSend.IsEnabled = true;
            }
        }
    }
}