using Banking_system.Entity;
using Banking_system.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;

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
            if (_currentUser.Cards != null) { 
                _userCards = _currentUser.Cards.ToList();
                UpdateCardUI();
            }
        }
        private void UpdateCardUI()
        {
            if (_userCards == null || _userCards.Count == 0)
            {
                _currentCardIndex = 0;
                ShowAddCardUI();
                return;
            }

            // Якщо індекс дійшов до кінця (кількість карток), показуємо "Додати картку"
            if (_currentCardIndex == _userCards.Count)
                ShowAddCardUI();
            else
            {
                // Відображаємо інформацію про звичайну картку
                GridCardInfo.Visibility = Visibility.Visible;
                GridAddCard.Visibility = Visibility.Collapsed;

                AbstractCard currentCard = _userCards[_currentCardIndex];
                string cardName = "Дебетова картка";

                if (currentCard is CreditCard)
                {
                    cardName = "Кредитна картка";
                    Card.Background = GetCardGradient("Credit");
                }
                else if (currentCard is JuniorCard)
                {
                    cardName = "Картка Юніора";
                    Card.Background = GetCardGradient("Junior");
                }
                else if (currentCard is DebitCard)
                    Card.Background = GetCardGradient("Debit");

                LoadCardData(currentCard, cardName);
            }
        }
        // Метод для створення стильних градієнтів карток
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
                default: 
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#141E30"), 0.0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#243B55"), 1.0));
                    break;
            }

            return gradient;
        }
        private void ShowAddCardUI()
        {
            GridCardInfo.Visibility = Visibility.Collapsed;
            GridAddCard.Visibility = Visibility.Visible;

            LinearGradientBrush gradient = new LinearGradientBrush();
            gradient.StartPoint = new Point(0, 0);
            gradient.EndPoint = new Point(1, 1);
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#0F2027"), 0.0));
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#203A43"), 0.5));
            gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2C5364"), 1.0));

            Card.Background = gradient;
        }

        private void BtnPrevCard_Click(object sender, RoutedEventArgs e)
        {
            int totalItems = _userCards.Count; 

            _currentCardIndex--;
            if (_currentCardIndex < 0)
                _currentCardIndex = totalItems; 

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
            if (_userCards?.Count == 0 || _currentCardIndex == _userCards?.Count) return;

            string fullNumber = _userCards[_currentCardIndex].GetCardNumber();
            Clipboard.SetText(fullNumber);

            MessageBox.Show($"Номер картки\n{fullNumber}\nуспішно скопійовано в буфер обміну!",
                            "Копіювання",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
        private void BtnCreateCard_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Тут відкриватиметься вікно замовлення нової картки.");
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

        private void BtnCreateDebit_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            int count = 0;
            foreach (var card in _userCards)
                if (card is DebitCard) count++;

            if(count >= 3)
            {
                MessageBox.Show("Ви вже маєте 3 дебетові картки. Немає можливості відкрити більше.", "Обмеження");
                return;
            }

            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();

                var newCard = new DebitCard
                {
                    UserId = _currentUser.ID
                };

                db.Cards.Add(newCard);
                db.SaveChanges(); 

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;

                 Logger.LogUserAction(_currentUser.Email, "Створив дебетову картку"); 
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

                Logger.LogUserAction(_currentUser.Email, "Створив кредитну картку");
            }

            UpdateCardUI();
            MessageBox.Show("Кредитну картку успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCreateJunior_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            foreach (var card in _userCards)
                if (card is JuniorCard) count++;

            if (count >= 2)
            {
                MessageBox.Show("Ви вже маєте 2 карти юніора. Немає можливості відкрити більше.", "Обмеження");
                return;
            }
            if (_currentUser == null) return;

            using (var db = new Banking_system.Database.Database())
            {
                db.Database.EnsureCreated();

                var newCard = new JuniorCard
                {
                    UserId = _currentUser.ID,
                    TransactionLimit = 2000m
                };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;

                Logger.LogUserAction(_currentUser.Email, "Створив картку Юніора");
            }

            UpdateCardUI();
            MessageBox.Show("Картку Юніора успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)=>Application.Current.Shutdown();
    }
}