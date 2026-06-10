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
            if (_userCards.Count == 0) return;

            AbstractCard currentCard = _userCards[_currentCardIndex];
            string cardName = "Дебетова картка";
            if (currentCard is CreditCard) 
            {
                cardName = "Кредитна картка";
                Card.Background = Brushes.DarkRed;
            }
            else if (currentCard is UniorCard) 
            {
                cardName = "Картка Юніора";
                Card.Background = Brushes.LightGreen;
            }
            else if (currentCard is DebitCard)
                Card.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2C2C2C"); 

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

        private void BtnPrevCard_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards.Count == 0) return;

            _currentCardIndex--;
            if (_currentCardIndex < 0) _currentCardIndex = _userCards.Count - 1;

            UpdateCardUI();
        }

        private void BtnNextCard_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards.Count == 0) return;

            _currentCardIndex++;
            if (_currentCardIndex >= _userCards.Count) _currentCardIndex = 0;

            UpdateCardUI();
        }

        private void TxtCardNumber_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_userCards.Count == 0) return;

            string fullNumber = _userCards[_currentCardIndex].GetCardNumber();
            Clipboard.SetText(fullNumber);

            MessageBox.Show($"Номер картки\n{fullNumber}\nуспішно скопійовано в буфер обміну!",
                            "Копіювання",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)=>Application.Current.Shutdown();
    }
}