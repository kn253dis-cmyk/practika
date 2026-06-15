using Banking_system.DataBase;
using Banking_system.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Banking_system.Windows
{
    public partial class DepositWindow : Window
    {
        // Окремі списки категорій для перевірки
        private readonly List<string> _withdrawOnlyCategories = new List<string>
        {
            "Зняття готівки", "Супермаркети", "Кафе та ресторани", "Підписки та сервіси", "Аптеки", "Транспорт"
        };

        private readonly List<string> _depositOnlyCategories = new List<string>
        {
            "Зарплата", "Стипендія", "Через термінал", "Поповнення з іншої картки"
        };

        public DepositWindow()
        {
            InitializeComponent();
            LoadOperationTypes();
        }

        private void LoadOperationTypes()
        {
            // Об'єднуємо всі категорії в один список для ComboBox + додаємо нейтральну "Інше"
            var allTypes = new List<string>();
            allTypes.AddRange(_depositOnlyCategories);
            allTypes.AddRange(_withdrawOnlyCategories);
            allTypes.Add("Інше");

            CmbOperationType.ItemsSource = allTypes;
            CmbOperationType.SelectedIndex = 0;
        }

        private void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string selectedCategory = CmbOperationType.Text;

                if (_withdrawOnlyCategories.Contains(selectedCategory))
                {
                    MessageBox.Show($"Категорія '{selectedCategory}' призначена лише для зняття коштів!\nБудь ласка, оберіть категорію для поповнення (наприклад, 'Зарплата' або 'Через термінал').", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string cardNumber = TxtCardNumber.Text.Trim();
                decimal amount = decimal.Parse(TxtAmount.Text);

                using (var db = new Database())
                {
                    var card = db.Cards.FirstOrDefault(c => c.CardNumber == cardNumber);
                    if (card == null)
                    {
                        MessageBox.Show("Картку не знайдено!", "Помилка");
                        return;
                    }

                    var depositTx = new DepositTransaction(card, amount);
                    if (depositTx.Execute())
                    {
                        MessageBox.Show($"Ви успішно поповнили картку {cardNumber} на {amount} грн\nТип: {selectedCategory}", "Успіх");
                        this.Close();
                    }
                    else
                        MessageBox.Show("Помилка поповнення. Спробуйте пізніше.", "Помилка");
                }
            }
        }

        private void BtnWithdraw_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInput())
            {
                string selectedCategory = CmbOperationType.Text;

                // ПЕРЕВІРКА: Чи не намагається користувач зняти кошти через категорію поповнення
                if (_depositOnlyCategories.Contains(selectedCategory))
                {
                    MessageBox.Show($"Категорія '{selectedCategory}' призначена лише для поповнення!\nБудь ласка, оберіть категорію для витрат.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string cardNumber = TxtCardNumber.Text.Trim();
                decimal amount = decimal.Parse(TxtAmount.Text);

                using (var db = new Database())
                {
                    var card = db.Cards.FirstOrDefault(c => c.CardNumber == cardNumber);
                    if (card == null)
                    {
                        MessageBox.Show("Картку не знайдено!", "Помилка");
                        return;
                    }

                    var withdrawTx = new WithdrawTransaction(card, amount, selectedCategory);
                    if (withdrawTx.Execute())
                    {
                        MessageBox.Show($"Витрата ({amount} грн) у категорії '{selectedCategory}' успішна!", "Успіх");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Недостатньо коштів на картці!", "Помилка");
                    }
                }
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrEmpty(TxtCardNumber.Text) || string.IsNullOrEmpty(TxtAmount.Text))
            {
                MessageBox.Show("Заповніть всі поля!", "Помилка");
                return false;
            }
            if (!decimal.TryParse(TxtAmount.Text, out decimal result) || result <= 0)
            {
                MessageBox.Show("Сума повинна бути більшою за нуль!", "Помилка");
                return false;
            }
            if (string.IsNullOrEmpty(CmbOperationType.Text))
            {
                MessageBox.Show("Оберіть тип операції!", "Помилка");
                return false;
            }
            return true;
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}