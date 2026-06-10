using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Banking_system.Entity;
using Banking_system.Models;

namespace Banking_system.Windows
{
    public partial class MainWindow : Window
    {
        private User _currentUser;

        // Список карток користувача та індекс поточної картки на екрані
        private List<AbstractCard> _userCards = new List<AbstractCard>;
        private int _currentCardIndex = 0;

        public MainWindow(User authenticatedUser)
        {
            InitializeComponent();
            _currentUser = authenticatedUser;
            LoadUserData(_currentUser);

            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();
                foreach (var card in _userCards)
                {
                    
                }
            }

            //// Ініціалізуємо список карток (У майбутньому це буде завантажуватись з БД)
            //_userCards = new List<AbstractCard>();

            //// Створюємо три різні картки для демонстрації перемикання
            //DebitCard debitCard = new DebitCard();
            //debitCard.Deposit(15420.50m); // Поповнюємо на певну суму

            //CreditCard creditCard = new CreditCard();
            //creditCard.Deposit(2500.00m); // Свої кошти на кредитці

            //UniorCard uniorCard = new UniorCard();
            //uniorCard.Deposit(840.00m); // Кишенькові гроші

            //// Додаємо картки в наш "гаманець"
            //_userCards.Add(debitCard);
            //_userCards.Add(creditCard);
            //_userCards.Add(uniorCard);

            //// Виводимо першу картку на екран

            UpdateCardUI();
        }
        }

        // Метод, який оновлює екран залежно від того, яка картка зараз обрана
        private void UpdateCardUI()
        {
            if (_userCards.Count == 0) return;

            AbstractCard currentCard = _userCards[_currentCardIndex];

            // Визначаємо назву типу картки
            string cardName = "Дебетова картка";
            if (currentCard is CreditCard) cardName = "Кредитна картка";
            else if (currentCard is UniorCard) cardName = "Картка Юніора";

            LoadCardData(currentCard, cardName);
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
        // ОБРОБНИКИ СТРІЛОЧОК ТА КОПІЮВАННЯ
        // ==========================================

        private void BtnPrevCard_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards.Count == 0) return;

            _currentCardIndex--;
            // Якщо дійшли до початку - перекидаємо в кінець (циклічність)
            if (_currentCardIndex < 0) _currentCardIndex = _userCards.Count - 1;

            UpdateCardUI();
        }

        private void BtnNextCard_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards.Count == 0) return;

            _currentCardIndex++;
            // Якщо дійшли до кінця - перекидаємо на початок (циклічність)
            if (_currentCardIndex >= _userCards.Count) _currentCardIndex = 0;

            UpdateCardUI();
        }

        private void TxtCardNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_userCards.Count == 0) return;

            // Беремо ПОВНИЙ номер поточної картки без зірочок
            string fullNumber = _userCards[_currentCardIndex].GetCardNumber();

            // Копіюємо в буфер обміну Windows
            Clipboard.SetText(fullNumber);

            MessageBox.Show($"Номер картки\n{fullNumber}\nуспішно скопійовано в буфер обміну!",
                            "Копіювання",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        // ==========================================
        // КЕРУВАННЯ ВІКНОМ
        // ==========================================

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}