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

            using (var db = new Banking_system.DataBase.Database())
            {
                if (db.Users.Any(u => u.Email == email || u.Ipn == ipn))
                {
                    MessageBox.Show("Користувач з таким Email або ІПН вже існує!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 1. Створюємо користувача та зберігаємо його в БД, щоб отримати ID
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

                db.Users.Add(newUser);
                db.SaveChanges(); // Тепер newUser.ID має реальне значення

                string? selectedCard = ((ComboBoxItem)CmbCardType.SelectedItem).Content.ToString();
                string createdCardNumber = "";
                bool showDefaultSuccessMessage = true;

                // 2. Логіка створення картки залежно від вибору
                if (selectedCard == "Кредитна")
                {
                    // Відкриваємо вікно вибору кредиту, передаючи туди створеного користувача
                    CreditPlanWindow creditWindow = new CreditPlanWindow(newUser);
                    creditWindow.ShowDialog(); // Код зупиниться тут, поки вікно не закриється

                    if (!creditWindow.IsCardCreated)
                    {
                        // Якщо користувач закрив вікно або відмовився від умов, створюємо дебетову картку
                        DebitCard defaultCard = new DebitCard { UserId = newUser.ID };
                        db.Cards.Add(defaultCard);
                        db.SaveChanges();
                        createdCardNumber = defaultCard.CardNumber;

                        MessageBox.Show("Оформлення кредиту скасовано. Вам відкрито стандартну Дебетову картку.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Кредит успішно створено (CreditPlanWindow вже показало своє повідомлення успіху)
                        var creditCard = db.Cards.FirstOrDefault(c => c.UserId == newUser.ID);
                        createdCardNumber = creditCard?.CardNumber ?? "Невідомо";
                        showDefaultSuccessMessage = false; // Вимикаємо стандартне повідомлення, щоб не дублювати
                    }
                }
                else
                {
                    // Логіка для Дебетової та Юніорської
                    AbstractCard newCard;
                    if (selectedCard == "Юніорська") newCard = new CurrencyCard { UserId = newUser.ID }; // Або JuniorCard
                    else newCard = new DebitCard { UserId = newUser.ID };

                    db.Cards.Add(newCard);
                    db.SaveChanges();
                    createdCardNumber = newCard.CardNumber;
                }

                Logger.AppendSystemLog(newUser.Email, $"Користувач успішно зареєструвався в системі. Створено картку: {createdCardNumber}");

                if (showDefaultSuccessMessage)
                {
                    MessageBox.Show($"Реєстрація успішна!\nВаш номер картки: {createdCardNumber}", "Успіх!", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                this.Close();
            }
        }
    }
}