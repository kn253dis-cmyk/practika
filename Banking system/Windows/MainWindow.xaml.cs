using Banking_system.Entity;
using Banking_system.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Banking_system.Windows
{
    public partial class MainWindow : Window
    {
        private User _currentUser;
        private List<AbstractCard> _userCards = new List<AbstractCard>();
        private int _currentCardIndex = 0;

        public MainWindow(User authenticatedUser)
        {
            InitializeComponent();
            _currentUser = authenticatedUser;
            LoadUserData(_currentUser);

            // Завантажуємо картки користувача з бази
            if (_currentUser.Cards != null)
            {
                _userCards = _currentUser.Cards.ToList();
                UpdateCardUI();
            }
        }

        // ==========================================
        // ЛОГІКА ВІДОБРАЖЕННЯ ІНТЕРФЕЙСУ
        // ==========================================

        private void UpdateCardUI()
        {
            // Якщо індекс дорівнює кількості карток, показуємо екран "Додати картку"
            if (_currentCardIndex == _userCards.Count)
            {
                ShowAddCardUI();
                return;
            }

            // Інакше показуємо звичайну інформацію про картку
            GridCardInfo.Visibility = Visibility.Visible;
            GridAddCard.Visibility = Visibility.Collapsed;

            if (_userCards.Count == 0) return;

            AbstractCard currentCard = _userCards[_currentCardIndex];
            string cardName = "Дебетова картка";

            // Визначаємо назву та градієнт залежно від типу картки
            // Визначаємо назву та градієнт залежно від типу картки
            if (currentCard is CreditCard creditCard)
            {
                cardName = "Кредитна картка";
                Card.Background = GetCardGradient("Credit");

                // Показуємо повзунок і ставимо його на поточний ліміт картки
                CreditLimitPanel.Visibility = Visibility.Visible;
                CreditLimitSlider.Value = creditCard.CreditLimit;
                TxtCreditLimitValue.Text = $"{creditCard.CreditLimit:N0} ₴";
            }
            else if (currentCard.GetType().Name == "JuniorCard" || currentCard.GetType().Name == "UniorCard")
            {
                cardName = "Картка Юніора";
                Card.Background = GetCardGradient("Junior");
                CreditLimitPanel.Visibility = Visibility.Collapsed; // Ховаємо повзунок
            }
            else if (currentCard is DebitCard)
            {
                Card.Background = GetCardGradient("Debit");
                CreditLimitPanel.Visibility = Visibility.Collapsed; // Ховаємо повзунок
            }
            LoadCardData(currentCard, cardName);
        }

        private void ShowAddCardUI()
        {
            GridCardInfo.Visibility = Visibility.Collapsed;
            GridAddCard.Visibility = Visibility.Visible;

            // Спеціальний темний градієнт для екрану додавання картки
            LinearGradientBrush gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0F2027"), 0.0));
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#203A43"), 0.5));
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2C5364"), 1.0));

            Card.Background = gradient;
        }

        private LinearGradientBrush GetCardGradient(string cardType)
        {
            LinearGradientBrush gradient = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1)
            };

            switch (cardType)
            {
                case "Credit":
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#870000"), 0.0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#3a0002"), 1.0));
                    break;
                case "Junior":
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#11998E"), 0.0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#38EF7D"), 1.0));
                    break;
                case "Debit":
                default:
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#141E30"), 0.0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#243B55"), 1.0));
                    break;
            }

            return gradient;
        }

        public void LoadUserData(User currentUser)
        {
            TxtUserName.Text = $"{currentUser.Name} {currentUser.MiddleName}".Trim();
        }

        public void LoadCardData(AbstractCard card, string cardTypeName)
        {
            TxtCardType.Text = cardTypeName;
            TxtBalance.Text = card.GetBalance().ToString("N2");

            string fullNumber = card.GetCardNumber();
            if (fullNumber.Length >= 4)
            {
                string last4 = fullNumber.Substring(fullNumber.Length - 4);
                TxtCardNumber.Text = $"**** **** **** {last4}";
            }

            TxtExpiryDate.Text = card.GetExpirationDate().ToString("MM/yy");
        }

        // ==========================================
        // НАВІГАЦІЯ ТА ВЗАЄМОДІЯ
        // ==========================================

        private void BtnPrevCard_Click(object sender, RoutedEventArgs e)
        {
            int totalItems = _userCards.Count;
            _currentCardIndex--;

            if (_currentCardIndex < 0)
                _currentCardIndex = totalItems; // Переходимо на екран додавання картки 

            UpdateCardUI();
        }

        private void BtnNextCard_Click(object sender, RoutedEventArgs e)
        {
            int totalItems = _userCards.Count;
            _currentCardIndex++;

            if (_currentCardIndex > totalItems)
                _currentCardIndex = 0;

            UpdateCardUI();
        }

        private void TxtCardNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_userCards == null || _userCards.Count == 0 || _currentCardIndex == _userCards.Count) return;

            string fullNumber = _userCards[_currentCardIndex].GetCardNumber();
            Clipboard.SetText(fullNumber);

            MessageBox.Show($"Номер картки\n{fullNumber}\nуспішно скопійовано в буфер обміну!",
                            "Копіювання",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        private void CreditLimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Оновлюємо текст під час перетягування повзунка
            if (TxtCreditLimitValue != null)
            {
                TxtCreditLimitValue.Text = $"{e.NewValue:N0} ₴";

                // Зберігаємо нове значення в об'єкт картки
                if (_userCards != null && _userCards.Count > 0 && _userCards[_currentCardIndex] is CreditCard creditCard)
                {
                    creditCard.CreditLimit = (int)e.NewValue;

                    // Якщо є підключення до БД, тут можна викликати збереження:
                    // using (var db = new Banking_system.Database.Database()) { ... db.SaveChanges(); }
                }
            }
        }
        private void BtnLogout_Click(object sender, RoutedEventArgs e)
{
    // Запитуємо користувача, чи точно він хоче вийти
    MessageBoxResult result = MessageBox.Show("Ви впевнені, що хочете вийти з акаунту?", 
                                              "Підтвердження виходу", 
                                              MessageBoxButton.YesNo, 
                                              MessageBoxImage.Question);
    
    if (result == MessageBoxResult.Yes)
    {
        // Очищаємо дані поточної сесії (якщо у колег створено такий клас)
        if (Banking_system.Service.SessionManager.CurrentUser != null)
        {
            //Banking_system.Service.SessionManager.CurrentUser = null;
        }

        // Відкриваємо стартове вікно авторизації
        loginWindow loginForm = new loginWindow();
        loginForm.Show();

        // Закриваємо особистий кабінет
        this.Close();
    }
}

        // ==========================================
        // СТВОРЕННЯ НОВИХ КАРТОК
        // ==========================================

        private void BtnCreateCard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Тут відкриватиметься вікно замовлення нової картки.");
        }

        private void BtnCreateDebit_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            int count = _userCards.Count(card => card is DebitCard);
            if (count >= 3)
            {
                MessageBox.Show("Ви вже маєте 3 дебетові картки. Немає можливості відкрити більше.", "Обмеження");
                return;
            }

            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();
                var newCard = new DebitCard { UserId = _currentUser.ID };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1; // Одразу показуємо нову картку

                // Якщо у колег є клас Logger, залишаємо цей рядок:
                // Logger.LogUserAction(_currentUser.Email, "Створив дебетову картку"); 
            }

            UpdateCardUI();
            MessageBox.Show("Дебетову картку успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCreateCredit_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards != null && _userCards.Any(c => c is CreditCard))
            {
                MessageBox.Show("Ви вже маєте кредитну картку. Немає можливості відкрити більше однієї.", "Обмеження");
                return;
            }
            if (_currentUser == null || _userCards == null)
            {
                return;
            }
            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();
                var newCard = new CreditCard
                {
                    UserId = _currentUser.ID,
                    CreditLimit = 5000,
                    CreditType = "Standard",
                    Percentage = 15.5m
                };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;
            }

            UpdateCardUI();
            MessageBox.Show("Кредитну картку успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCreateJunior_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            int count = _userCards.Count(card => card.GetType().Name == "JuniorCard" || card.GetType().Name == "UniorCard");
            if (count >= 2)
            {
                MessageBox.Show("Ви вже маєте 2 карти юніора. Немає можливості відкрити більше.", "Обмеження");
                return;
            }

            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();
                // Тут підстав той клас Юніорки, який ви реально використовуєте (JuniorCard або UniorCard)
                var newCard = new JuniorCard
                {
                    UserId = _currentUser.ID,
                    TransactionLimit = 5000m
                };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;
            }

            UpdateCardUI();
            MessageBox.Show("Картку Юніора успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ==========================================
        // ІСТОРІЯ, ПЕРЕКАЗИ ТА КЕРУВАННЯ ВІКНОМ
        // ==========================================

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            // Отримуємо email з класу колег, якщо він там зберігся
            string safeEmail = Banking_system.Service.SessionManager.CurrentUser?.Email ?? string.Empty;

            Window historyForm = new Window
            {
                Title = "Історія транзакцій",
                Width = 520,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Content = new Banking_system.Views.TransactionHistoryPage(safeEmail)
            };

            historyForm.ShowDialog();
        }

        private void BtnTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards.Count == 0 || _currentCardIndex == _userCards.Count) return;

            string currentCardNum = _userCards[_currentCardIndex].GetCardNumber();

            Window transferForm = new Window
            {
                Title = "Новий переказ коштів",
                Width = 600,
                Height = 520,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Content = new Banking_system.Views.TransferPage(currentCardNum)
            };

            transferForm.ShowDialog();

            using (var db = new Banking_system.Database.Database())
            {
                _userCards = db.FindAllCardsByUserId(_currentUser.ID);

                UpdateCardUI();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnDeposit_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}