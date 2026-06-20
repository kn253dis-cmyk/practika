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
using Banking_system.Pages;

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
            // 1. Перевірка на екран додавання нової картки
            if (_currentCardIndex == _userCards.Count)
            {
                ShowAddCardUI();
                return;
            }

            GridCardInfo.Visibility = Visibility.Visible;
            GridAddCard.Visibility = Visibility.Collapsed;

            if (_userCards == null || _userCards.Count == 0) return;

            AbstractCard currentCard = _userCards[_currentCardIndex];
            string cardName = "";

            // Очищаємо колір тексту назви картки (на випадок, якщо попередня картка була заблокована)
            TxtCardType.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));

            // Знаходимо кнопку у розмітці
            var btnCurrencyExchange = this.FindName("BtnCurrencyExchange") as Button;

            // 2. Встановлюємо дизайн залежно від типу картки
            if (currentCard is CreditCard creditCard)
            {
                cardName = "Кредитна картка";
                Card.Background = GetCardGradient("Credit");

                // ХОВАЄМО кнопку валюти
                if (btnCurrencyExchange != null) btnCurrencyExchange.Visibility = Visibility.Collapsed;

                // Показуємо панель кредитної інформації та ПОВЗУНОК
                if (PanelCreditInfo != null)
                {
                    PanelCreditInfo.Visibility = Visibility.Visible;
                    TxtCardCreditLimit.Text = $"{creditCard.CreditLimit:N0} ₴";

                    // Динамічний стиль для дати боргу
                    if (creditCard.Balance < 0)
                    {
                        TxtDebtDate.Text = creditCard.DueDate.ToString("dd.MM.yyyy");
                        TxtDebtDate.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8A8A"));
                        IconDebtDate.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF8A8A"));
                        IconDebtDate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CalendarAlert;
                    }
                    else
                    {
                        TxtDebtDate.Text = "Борг відсутній";
                        TxtDebtDate.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38EF7D"));
                        IconDebtDate.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38EF7D"));
                        IconDebtDate.Kind = MaterialDesignThemes.Wpf.PackIconKind.CalendarCheck;
                    }
                }

                //// ПОКАЗУЄМО повзунок ліміту
                //var creditLimitPanel = this.FindName("CreditLimitPanel") as UIElement;
                //if (creditLimitPanel != null) creditLimitPanel.Visibility = Visibility.Visible;

                //var creditLimitSlider = this.FindName("CreditLimitSlider") as Slider;
                //if (creditLimitSlider != null) creditLimitSlider.Value = creditCard.CreditLimit;
            }
            else if (currentCard.GetType().Name == "CurrencyCard" || currentCard.GetType().Name == "JuniorCard")
            {
                bool isCurrencyCard = currentCard.GetType().Name == "CurrencyCard";

                cardName = isCurrencyCard ? "Валютна карта" : "Картка Юніора";
                Card.Background = GetCardGradient(isCurrencyCard ? "Currency" : "Junior");

                // ПОКАЗУЄМО кнопку тільки для Валютної карти, для Юніора - ховаємо
                if (btnCurrencyExchange != null)
                    btnCurrencyExchange.Visibility = isCurrencyCard ? Visibility.Visible : Visibility.Collapsed;

                // ХОВАЄМО кредитну панель і повзунок
                if (PanelCreditInfo != null) PanelCreditInfo.Visibility = Visibility.Collapsed;

                var creditLimitPanel = this.FindName("CreditLimitPanel") as UIElement;
                if (creditLimitPanel != null) creditLimitPanel.Visibility = Visibility.Collapsed;
            }
            else if (currentCard is DebitCard)
            {
                cardName = "Дебетова картка";
                Card.Background = GetCardGradient("Debit");

                // ХОВАЄМО кнопку валюти
                if (btnCurrencyExchange != null) btnCurrencyExchange.Visibility = Visibility.Collapsed;

                // ХОВАЄМО кредитну панель і повзунок
                if (PanelCreditInfo != null) PanelCreditInfo.Visibility = Visibility.Collapsed;

                var creditLimitPanel = this.FindName("CreditLimitPanel") as UIElement;
                if (creditLimitPanel != null) creditLimitPanel.Visibility = Visibility.Collapsed;
            }

            // 3. Завантажуємо загальні дані (баланс, номер, дата)
            LoadCardData(currentCard, cardName);
        }

        private void ShowAddCardUI()
        {
            GridCardInfo.Visibility = Visibility.Collapsed;
            BtnCurrencyExchange.Visibility = Visibility.Collapsed;
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
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#4A0E1A"), 0.0));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#2A050A"), 0.5));
                    gradient.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#1A0000"), 1.0));
                    break;
                case "Currency":
                case "Junior": // Додано випадок для юніора
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

            var currencySymbolBlock = (TextBlock)((StackPanel)TxtBalance.Parent).Children[1];
            if (card is CurrencyCard currCard)
            {
                string symbol = currCard.CurrencyType switch
                {
                    "USD" => "$",
                    "EUR" => "€",
                    "GBP" => "£",
                    "PLN" => "zł",
                    "XAU" => "Au",
                    "XAG" => "Ag",
                    _ => currCard.CurrencyType
                };
                currencySymbolBlock.Text = $" {symbol}";
            }
            else
                currencySymbolBlock.Text = " ₴";
        }
        private void BtnCurrencyExchange_Click(object sender, RoutedEventArgs e)
        {
            // Перевіряємо, чи існують картки і чи обрана картка коректна
            if (_userCards == null || _userCards.Count == 0 || _currentCardIndex >= _userCards.Count) return;

            var currentCard = _userCards[_currentCardIndex];

            // Якщо поточна картка - валютна, відкриваємо сторінку обміну
            if (currentCard is CurrencyCard currencyCard)
            {
                Window exchangeForm = new Window
                {
                    Title = "Обмін та конвертація валюти",
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = this,
                    Width = 520,
                    Height = 720,
                    Background = new BrushConverter().ConvertFrom("#0F172A") as Brush,
                    Content = new Banking_system.Pages.CurrencyExchangePage(_currentUser, currencyCard)
                };
                exchangeForm.ShowDialog();

                // Оновлюємо дані після закриття вікна обміну
                using (var db = new Banking_system.DataBase.Database())
                {
                    _userCards = db.FindAllCardsByUserId(_currentUser.ID);
                    UpdateCardUI();
                }
            }
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

        //private void CreditLimitSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        //{
        //    // Оновлюємо текст під час перетягування повзунка
        //    var txtCreditLimitValue = this.FindName("TxtCreditLimitValue") as TextBlock;
        //    if (txtCreditLimitValue != null)
        //    {
        //        txtCreditLimitValue.Text = $"{e.NewValue:N0} ₴";

        //        // Зберігаємо нове значення в об'єкт картки
        //        if (_userCards != null && _userCards.Count > 0 && _currentCardIndex < _userCards.Count && _userCards[_currentCardIndex] is CreditCard creditCard)
        //            creditCard.CreditLimit = (int)e.NewValue;
        //    }
        //}

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

            if (Banking_system.Service.SessionManager.IsUserBlacklisted(_currentUser.ID))
            {
                MessageBox.Show("Дія заблокована! Ви занесені до Чорного Списку банку.", "Відмова в обслуговуванні", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

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


            int count = _userCards.Count(card => card.GetType().Name == "CurrencyCard" || card.GetType().Name == "CurrencyCard");
            if (count >= 2)
            {
                MessageBox.Show("Ви вже маєте 2 валютні карти. Немає можливості відкрити більше.", "Обмеження");
                return;
            }

            using (var db = new Banking_system.DataBase.Database())
            {
                var newCard = new CurrencyCard { UserId = _currentUser.ID };

                db.Cards.Add(newCard);
                db.SaveChanges();

                _userCards.Add(newCard);
                _currentCardIndex = _userCards.Count - 1;
            }

            UpdateCardUI();
            MessageBox.Show("Валютну карту успішно відкрито!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void BtnCloseCard_Click(object sender, RoutedEventArgs e)
        {
            if (_userCards == null || _userCards.Count == 0 || _currentCardIndex >= _userCards.Count) return;

            AbstractCard currentCard = _userCards[_currentCardIndex];

            // 1. ПЕРЕВІРКА: Якщо це кредитна картка
            if (currentCard is CreditCard creditCard)
            {
                // Оскільки всі відсотки тепер нараховуються прямо в баланс,
                // перевірка на мінусовий баланс одночасно перевіряє і тіло кредиту, і штрафи
                if (creditCard.Balance < 0)
                {
                    MessageBox.Show("Неможливо закрити кредитну картку, поки у Вас є непогашений борг. Будь ласка, поповніть рахунок для виходу в нуль.", "Відмова банку", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (creditCard.Balance > 0)
                {
                    MessageBox.Show($"На картці залишились Ваші власні кошти ({creditCard.Balance:N2} ₴). Будь ласка, перекажіть їх на інший рахунок перед закриттям.", "Відмова банку", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            else
            {
                if (currentCard.Balance > 0)
                {
                    MessageBox.Show($"На картці є залишок коштів ({currentCard.Balance:N2} ₴). Спочатку виведіть їх на інший рахунок.", "Відмова банку", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            MessageBoxResult result = MessageBox.Show(
                $"Ви дійсно бажаєте назавжди закрити картку {currentCard.GetCardNumber()}?\n\nЦю дію неможливо скасувати, історія картки стане недоступною.",
                "Підтвердження закриття рахунку",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Отримуємо номер картки для пошуку в БД
                    string targetCardNumber = currentCard.GetCardNumber();
                    using (var db = new Banking_system.DataBase.Database())
                    {
                        var cardToDelete = db.Cards.FirstOrDefault(c => c.CardNumber == targetCardNumber);

                        if (cardToDelete != null)
                        {
                            db.Cards.Remove(cardToDelete);
                            db.SaveChanges();
                        }

                        _userCards = db.FindAllCardsByUserId(_currentUser.ID);

                        if (_currentCardIndex >= _userCards.Count)
                        {
                            _currentCardIndex = Math.Max(0, _userCards.Count - 1);
                        }
                    }

                    UpdateCardUI();
                    MessageBox.Show("Картку успішно закрито та видалено з системи.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Виникла помилка при закритті картки: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnTimeTravel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new Banking_system.DataBase.Database())
                {
                    var cards = db.Cards.OfType<CreditCard>().Where(c => c.UserId == _currentUser.ID && c.Balance < 0).ToList();

                    if (cards.Count == 0)
                    {
                        MessageBox.Show("Для тестування спочатку оформіть кредит та зніміть кошти (зробіть мінус на балансі).", "QA Тест", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    foreach (var card in cards)
                    {
                        // ЕТАП 1: Перевірка попередження (за 7 днів)
                        if (card.LastWarningSentDate == DateTime.MinValue)
                        {
                            card.DueDate = DateTime.Now.AddDays(5); // Штучно робимо 5 днів до платежу
                            MessageBox.Show("ЕТАП 1: Час зсунуто.\nДо платежу залишилося 5 днів. Зараз система має надіслати лист-попередження на вашу пошту.", "QA Тест - Крок 1", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        // ЕТАП 2: Нарахування штрафу
                        else if (card.InterestAppliedCount < card.PlanDurationMonths)
                        {
                            card.DueDate = DateTime.Now.AddDays(-1); // Штучно робимо прострочення на 1 день
                            MessageBox.Show("ЕТАП 2: Час зсунуто.\nПлатіж прострочено. Зараз система нарахує штраф (додасть до мінуса) та надішле квитанцію.", "QA Тест - Крок 2", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        // ЕТАП 3: Блокування картки (кінець терміну плану)
                        else if (!card.IsBlocked)
                        {
                            card.TermEndDate = DateTime.Now.AddDays(-1); // План закінчився вчора
                            MessageBox.Show("ЕТАП 3: Час зсунуто.\nКредитний план повністю завершився. Зараз картка буде заблокована.", "QA Тест - Крок 3", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        // ЕТАП 4: Чорний список (минув 1 місяць після блокування)
                        else
                        {
                            card.TermEndDate = DateTime.Now.AddMonths(-1).AddDays(-1); // Пройшов місяць з кінця плану
                            MessageBox.Show("ЕТАП 4: Час зсунуто.\nМинув 1 місяць після завершення плану. Зараз на ваш профіль буде накладено перманентний бан (Чорний список).", "QA Тест - Крок 4", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    db.SaveChanges();
                }

                // 2. Запускаємо банківські перевірки
                Banking_system.Service.SessionManager.CheckAndProcessCredits(_currentUser.ID);

                // 3. Оновлюємо інтерфейс
                using (var db = new Banking_system.DataBase.Database())
                {
                    _userCards = db.FindAllCardsByUserId(_currentUser.ID);
                    if (_currentCardIndex >= _userCards.Count) _currentCardIndex = Math.Max(0, _userCards.Count - 1);
                }
                UpdateCardUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка тестування: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            string safeEmail = Banking_system.Service.SessionManager.CurrentUser?.Email ?? string.Empty;

            Window historyForm = new Window
            {
                Title = "Історія транзакцій",
                Width = 800,
                Height = 900,
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
                        var popularCodes = new List<string> { "USD", "EUR", "PLN", "GBP", "XAU", "XAG" };

                        var filteredRates = allRates.Where(rate => popularCodes.Contains(rate.cc ?? "")).ToList();

                        string tickerText = "";
                        foreach (var rate in filteredRates)
                        {
                            // ВИПРАВЛЕНО: Використовуємо символи замість багованих системних прапорів
                            string symbol = rate.cc switch
                            {
                                "USD" => "$",
                                "EUR" => "€",
                                "GBP" => "£",
                                "PLN" => "zł",
                                "XAU" => "Au",
                                "XAG" => "Ag",
                                _ => "¤"
                            };
                            tickerText += $"{symbol} {rate.cc}: {rate.rate:F2} ₴   |   ";
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