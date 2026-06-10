using System;
using System.Windows;
using Banking_system.Models;
using Banking_system.Service;

namespace Banking_system.Views
{
    public partial class ReceiptConfirmationWindow : Window
    {
        JsonLog.LogEntry transactionLog;
        EmailService emailService;

        public ReceiptConfirmationWindow(JsonLog.LogEntry log)
        {
            InitializeComponent();

            transactionLog = log;
            emailService = new EmailService();

            TxtEmail.Text = SessionManager.CurrentUser?.Email ?? "";

            TxtInfo.Text = $"Тип операції: {transactionLog.TemplateName}\nДеталі: {transactionLog.Text}";
        }

     
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

  
        private async void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                string targetEmail = TxtEmail.Text;


                string htmlContent = emailService.PrepareReceiptHtml(
                    transactionLog.TemplateName,
                    transactionLog.ReceiptData
                );


                string subject = $"Квитанція за операцією: {transactionLog.Id}";


                await emailService.SendEmailAsync(targetEmail, subject, htmlContent);

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