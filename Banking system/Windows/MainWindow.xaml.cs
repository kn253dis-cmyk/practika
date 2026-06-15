using Banking_system.Entity;
using Banking_system.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;


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
            _ = LoadCurrencyRatesAsync();
            LoadUserData(_currentUser);

            // Завантажуємо картки користувача з бази
            if (_currentUser.Cards != null)
            {
                _userCards = _currentUser.Cards.ToList();
                UpdateCardUI();
            }
        }

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

            // Визначаємо назву, градієнт та наявність ліміту залежно від типу картки
            if (currentCard is CreditCard creditCard)
            {
                cardName = "Кредитна картка";
                Card.Background = GetCardGradient("Credit");

                // Показуємо поле ліміту на самій картці та вписуємо значення
                if (PanelCreditLimit != null)
                {
                    PanelCreditLimit.Visibility = Visibility.Visible;
                    TxtCardCreditLimit.Text = $"{creditCard.CreditLimit:N0} ₴";
                }
            }
            else if (currentCard.GetType().Name == "CurrencyCard" || currentCard.GetType().Name == "JuniorCard")
            {
                cardName = "Валютна карта";
                Card.Background = GetCardGradient("Currency");

                // Ховаємо поле ліміту для інших карток
                if (PanelCreditLimit != null) PanelCreditLimit.Visibility = Visibility.Collapsed;
            }
            else if (currentCard is DebitCard)
            {
                cardName = "Дебетова картка";
                Card.Background = GetCardGradient("Debit");

                // Ховаємо поле ліміту
                if (PanelCreditLimit != null) PanelCreditLimit.Visibility = Visibility.Collapsed;
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
                case "Currency":
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
                    // using (var db = new Banking_system.DataBase.DataBase()) { .. db.SaveChanges(); }
                }
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Ви впевнені, що хочете вийти з акаунту?",
                                                      "Підтвердження виходу",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                loginWindow loginForm = new loginWindow();
                loginForm.Show();
                this.Close();
            }
        }

        private void Ticker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                Banking_system.Windows.ExchangeWindow exchangeForm = new Banking_system.Windows.ExchangeWindow();
                exchangeForm.Owner = this;
                exchangeForm.ShowDialog();
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

            using (var db = new Banking_system.DataBase.Database())
            {
                 
                var newCard = new DebitCard { UserId = _currentUser.ID };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;
            }

            UpdateCardUI();
            MessageBox.Show("Дебетову картку успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCreateCredit_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards != null && _userCards.Any(c => c is CreditCard))
            {
                MessageBox.Show("Ви вже маєте кредитну картку. Одночасно можна мати лише одну.", "Обмеження");
                return;
            }
            if (_currentUser == null) return;

            var planWindow = new CreditPlanWindow(_currentUser);
            planWindow.Owner = this;
            planWindow.ShowDialog();

            if (planWindow.IsCardCreated)
            {
                using (var db = new Banking_system.DataBase.Database())
                {
                    _userCards = db.FindAllCardsByUserId(_currentUser.ID);
                    _currentCardIndex = _userCards.Count - 1;
                }
                UpdateCardUI();
            }
        }

        private void BtnCreateJunior_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null) return;

            int count = _userCards.Count(card => card.GetType().Name == "JuniorCard" || card.GetType().Name == "UniorCard");
            if (count >= 2)
            {
                MessageBox.Show("Ви вже маєте 2 валютні карти. Немає можливості відкрити більше.", "Обмеження");
                return;
            }

            using (var db = new Banking_system.DataBase.Database())
            {
                var newCard = new CurrencyCard
                {
                    UserId = _currentUser.ID
                };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;
            }

            UpdateCardUI();
            MessageBox.Show("Валютну карту успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
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
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                Width = 520,
                Height = 680,
                Background = new BrushConverter().ConvertFrom("#1A1A2E") as Brush,
                Content = new Banking_system.Views.TransferPage(currentCardNum)
            };
            transferForm.ShowDialog();

            using (var db = new Banking_system.DataBase.Database())
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
            // Перевіряємо, чи є обрана картка
            if (_userCards == null || _userCards.Count == 0 || _currentCardIndex >= _userCards.Count) return;

            string currentCardNum = _userCards[_currentCardIndex].GetCardNumber();

            // Відкриваємо вікно та передаємо номер поточної картки у поле
            Banking_system.Windows.DepositWindow depositForm = new Banking_system.Windows.DepositWindow();
            depositForm.TxtCardNumber.Text = currentCardNum;
            depositForm.Owner = this;
            depositForm.ShowDialog();

            // Оновлюємо дані інтерфейсу (включаючи новий баланс) після закриття вікна
            using (var db = new Banking_system.DataBase.Database())
            {
                _userCards = db.FindAllCardsByUserId(_currentUser.ID);
                UpdateCardUI();
            }
        }

        private void BtnStatistics_Click(object sender, RoutedEventArgs e)
        {
            // Перевіряємо, чи є взагалі картки
            if (_userCards == null || _userCards.Count == 0 || _currentCardIndex >= _userCards.Count) return;

            // Отримуємо номер поточної обраної картки
            string currentCardNum = _userCards[_currentCardIndex].GetCardNumber();

            // Відкриваємо наше нове вікно аналітики
            Banking_system.Windows.StatisticsWindow statsForm = new Banking_system.Windows.StatisticsWindow(currentCardNum);
            statsForm.Owner = this;
            statsForm.ShowDialog();
        }


        private async Task LoadCurrencyRatesAsync()
        {
            // 1. ОДРАЗУ запускаємо рух запасного тексту, не чекаючи на інтернет
            Application.Current.Dispatcher.Invoke(() =>
            {
                StartMarqueeAnimation();
            });

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    string url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
                    string jsonResponse = await client.GetStringAsync(url);

                    var allRates = JsonSerializer.Deserialize<List<CurrencyRate>>(jsonResponse);

                    if (allRates != null)
                    {
                        var popularCodes = new List<string> { "USD", "EUR", "PLN", "GBP", "XAU" };

                        var filteredRates = allRates.Where(rate => popularCodes.Contains(rate.cc ?? "")).ToList();

                        string tickerText = "";
                        foreach (var rate in filteredRates)
                        {
                            string flag = rate.cc switch
                            {
                                "USD" => "🇺🇸",
                                "EUR" => "🇪🇺",
                                "GBP" => "🇬🇧",
                                "PLN" => "🇵🇱",
                                "XAU" => "🥇",
                                _ => "🪙"
                            };
                            tickerText += $"{flag} {rate.cc}: {rate.rate:F2} ₴   |   ";
                        }

                        // 3. Якщо дані завантажились успішно — просто підміняємо текст, анімація продовжить йти
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (TxtTicker != null && !string.IsNullOrEmpty(tickerText))
                                TxtTicker.Text = tickerText;
                        });
                    }
                }
            }
            catch (Exception)
            {
               
            }
        }

        // Окремий метод, який гарантовано штовхає рядок
        private void StartMarqueeAnimation()
        {
            if (TxtTicker == null) return;

            var animation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 900,       // З'являється справа за межами екрана
                To = -1000,       // Їде далеко за лівий край
                Duration = TimeSpan.FromSeconds(30), // Швидкість руху
                RepeatBehavior = System.Windows.Media.Animation.RepeatBehavior.Forever
            };

            // Анімуємо фізичну координату відносно лівого краю. Це працює завжди.
            TxtTicker.BeginAnimation(Canvas.LeftProperty, animation);
        }
    }
}