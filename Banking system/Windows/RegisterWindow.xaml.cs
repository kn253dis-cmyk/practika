using Banking_system.Entity;
using Banking_system.Models; 
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Banking_system.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
        private void BtnBackToLogin_Click(object sender, RoutedEventArgs e) => this.Close();

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtFullName.Text.Trim();
            string phone = TxtPhone.Text.Trim();
            string email = TxtEmail.Text.Trim(); 
            string ipn = TxtIpn.Text.Trim();
            string password = TxtPassword.Password;
            string confirmPassword = TxtConfirmPassword.Password;

            
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(ipn) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show("Будь ласка, введіть коректний Email!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show("Паролі не співпадають!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string[] nameParts = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length < 3)
            {
                MessageBox.Show("Введіть Прізвище, Ім'я та По батькові повністю (через пробіл)!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();

                if (db.Users.Any(u => u.Email == email || u.Ipn == ipn))
                {
                    MessageBox.Show("Користувач з таким Email або ІПН вже існує!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string? selectedCard = ((ComboBoxItem)CmbCardType.SelectedItem).Content.ToString();
                AbstractCard? newCard = null;

                if (selectedCard == "Дебетова") newCard = new DebitCard();
                else if (selectedCard == "Кредитна") newCard = new CreditCard();
                else if (selectedCard == "Юніорська") newCard = new UniorCard();

                if (newCard == null) return;

                User newUser = new User
                {
                    Surname = nameParts[0],
                    Name = nameParts[1],
                    MiddleName = nameParts[2],
                    Phone = phone,
                    Email = email,
                    Ipn = ipn,
                    Password = db.HashPassword(password)
                };

                // ВАЖЛИВО: Додаємо картку до колекції користувача
                newUser.Cards.Add(newCard);
                db.Users.Add(newUser);
                db.SaveChanges(); 

                MessageBox.Show($"Реєстрація успішна!\nВаш номер картки: {newCard.CardNumber}", "Успіх!", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
        }
        private void CmbCardType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }
    }
}