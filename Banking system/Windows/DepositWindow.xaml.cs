using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Banking_system.Windows
{
    public partial class DepositWindow : Window
    {
        public DepositWindow()
        {
            InitializeComponent();
            LoadOperationTypes();
        }

        private void LoadOperationTypes()
        {
            // Список типів поповнення/зняття
            var types = new List<string> { "З банківської картки", "Через термінал", "Зарплата", "Стипендія", "Готівка" };
            CmbOperationType.ItemsSource = types;
        }

        private void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {
            // ТУТ ТВОЯ ЛОГІКА ПОПОВНЕННЯ (БАЗА ДАНИХ)
            if (ValidateInput())
            {
                MessageBox.Show($"Ви успішно поповнили картку {TxtCardNumber.Text} на {TxtAmount.Text} грн\nТип: {CmbOperationType.Text}", "Успіх");
                // TODO: Додати код оновлення балансу в БД
            }
        }

        private void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            // ТУТ ТВОЯ ЛОГІКА ЗНЯТТЯ (ІМІТАЦІЯ БАНКОМАТУ)
            if (ValidateInput())
            {
                MessageBox.Show($"Кошти ({TxtAmount.Text} грн) було успішно знято через {CmbOperationType.Text}!", "Банкомат");
                // TODO: Додати код списання грошей в БД
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(TxtCardNumber.Text) || string.IsNullOrEmpty(TxtAmount.Text))
            {
                MessageBox.Show("Заповніть всі поля!", "Помилка");
                return false;
            }
            return true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
