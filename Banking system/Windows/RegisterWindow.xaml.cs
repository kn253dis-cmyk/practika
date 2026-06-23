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
        private bool IsValidEmail(string email, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(email))
            {
                errorMessage = "Пошта не може бути порожньою.";
                return false;
            }

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                errorMessage = "Неправильний формат пошти. Переконайтеся, що є символ '@' та вказано правильний домен наприклад: user@gmail.com";
                return false;
            }
        }

        private bool IsValidPhone(string phone, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(phone))
            {
                errorMessage = "Номер телефону не може бути порожнім.";
                return false;
            }

            string cleanPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");

            if (cleanPhone.StartsWith("+380") && cleanPhone.Length == 13 && cleanPhone.Substring(1).All(char.IsDigit))
            {
                return true;
            }
            else if (cleanPhone.StartsWith("380") && cleanPhone.Length == 12 && cleanPhone.All(char.IsDigit))
            {
                return true;
            }
            else if (cleanPhone.StartsWith("0") && cleanPhone.Length == 10 && cleanPhone.All(char.IsDigit))
            {
                return true;
            }

            errorMessage = "Введіть коректний український номер телефону наприклад: +380991234567";
            return false;
        }

        private bool IsValidRealIpn(string ipn, DateTime dateOfBirth, bool isMale, out string errorMessage)
        {
            errorMessage = string.Empty;

            // Базова валідація
            if (string.IsNullOrWhiteSpace(ipn) || ipn.Length != 10 || !ipn.All(char.IsDigit))
            {
                errorMessage = "ІПН має складатися рівно з 10 цифр.";
                return false;
            }

            // Математична перевірка (контрольна цифра)
            int[] coefficients = { -1, 5, 7, 9, 4, 6, 10, 5, 7 };
            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (ipn[i] - '0') * coefficients[i];
            }
            int expectedControlDigit = (sum % 11) % 10;
            if (expectedControlDigit != (ipn[9] - '0'))
            {
                errorMessage = "ІПН не пройшов математичну перевірку (вигаданий номер).";
                return false;
            }

            // Перевірка дати народження з виправленням помилки Excel
            int daysFrom1899 = int.Parse(ipn.Substring(0, 5));
            int daysToSubtract = daysFrom1899 > 59 ? 1 : 0;
            DateTime expectedDate = new DateTime(1899, 12, 31).AddDays(daysFrom1899 - daysToSubtract);

            if (expectedDate.Year != dateOfBirth.Year ||
                expectedDate.Month != dateOfBirth.Month ||
                expectedDate.Day != dateOfBirth.Day)
            {
                errorMessage = $"ІПН не співпадає з датою народження! В ІПН зашифровано: {expectedDate:dd.MM.yyyy}, а ви вказали: {dateOfBirth:dd.MM.yyyy}";
                return false;
            }

            // Перевірка статі
            int genderDigit = ipn[8] - '0';
            bool isIpnMale = (genderDigit % 2 != 0);
            if (isIpnMale != isMale)
            {
                errorMessage = "ІПН не співпадає зі статтю, обраною у формі.";
                return false;
            }

            return true;
        }
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string fullName = TxtFullName.Text.Trim();
            string phone = TxtPhone.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string ipn = TxtIpn.Text.Trim();
            string password = TxtPassword.Password;
            string confirmPassword = TxtConfirmPassword.Password;

            DateTime? selectedDate = DpDateOfBirth.SelectedDate;
            string? selectedGender = ((ComboBoxItem)CmbGender.SelectedItem)?.Content.ToString();
            bool isMale = selectedGender == "Чоловіча";

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(phone) ||
                string.IsNullOrEmpty(email) || string.IsNullOrEmpty(ipn) ||
                string.IsNullOrEmpty(password) || DpDateOfBirth.SelectedDate == null ||
                selectedGender == "Оберіть стать")
            {
                MessageBox.Show("Будь ласка, заповніть усі поля, оберіть дату народження та стать!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedDate == null)
            {
                MessageBox.Show("Будь ласка, оберіть дату народження в календарі!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidPhone(phone, out string phoneError))
            {
                MessageBox.Show(phoneError, "Помилка телефону", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidEmail(email, out string emailError))
            {
                MessageBox.Show(emailError, "Помилка пошти", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidRealIpn(ipn, selectedDate.Value, isMale, out string ipnError))
            {
                MessageBox.Show(ipnError, "Помилка ІПН", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                User newUser = new User
                {
                    Surname = nameParts[0],
                    Name = nameParts[1],
                    MiddleName = nameParts[2],
                    Phone = phone,
                    Email = email,
                    Ipn = ipn,
                    DateOfBirth = selectedDate.Value,
                    IsMale = isMale,
                    Password = db.HashPassword(password)
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                string? selectedCard = ((ComboBoxItem)CmbCardType.SelectedItem).Content.ToString();
                string createdCardNumber = "";
                bool showDefaultSuccessMessage = true;

                if (selectedCard == "Кредитна")
                {
                    CreditPlanWindow creditWindow = new CreditPlanWindow(newUser);
                    creditWindow.ShowDialog();

                    if (!creditWindow.IsCardCreated)
                    {
                        DebitCard defaultCard = new DebitCard { UserId = newUser.ID };
                        db.Cards.Add(defaultCard);
                        db.SaveChanges();
                        createdCardNumber = defaultCard.CardNumber;

                        MessageBox.Show("Оформлення кредиту скасовано. Вам відкрито стандартну Дебетову картку.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var creditCard = db.Cards.FirstOrDefault(c => c.UserId == newUser.ID);
                        createdCardNumber = creditCard?.CardNumber ?? "Невідомо";
                        showDefaultSuccessMessage = false;
                    }
                }
                else
                {
                    AbstractCard newCard;

                    if (selectedCard == "Валютна")
                        newCard = new CurrencyCard { UserId = newUser.ID };
                    else
                        newCard = new DebitCard { UserId = newUser.ID };

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