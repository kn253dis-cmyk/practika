using Banking_system.Entity;
using Banking_system.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Banking_system.Pages
{
    public partial class CurrencyExchangePage : Page
    {
        private User _user;
        private CurrencyCard _currencyCard;
        private decimal _currentRate = 41.50m;
        private List<CurrencyRate> _exchangeRates = new List<CurrencyRate>();
        private static readonly HttpClient _httpClient = new HttpClient();

        private List<AbstractCard> _uahCards = new List<AbstractCard>();
        private bool isBuying = true;

        public CurrencyExchangePage(User user, CurrencyCard currencyCard)
        {
            InitializeComponent();
            _user = user;
            _currencyCard = currencyCard;

            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            using (var db = new DataBase.Database())
            {
                _uahCards = db.Cards
                    .Where(c => c.UserId == _user.ID && c.CardNumber != _currencyCard.CardNumber && !(c is CurrencyCard))
                    .ToList();
            }

            if (!string.IsNullOrEmpty(_currencyCard.CurrencyType))
            {
                bool currencyFound = false;
                foreach (ComboBoxItem item in CboCurrency.Items)
                    if (item.Content.ToString() == _currencyCard.CurrencyType)
                    {
                        CboCurrency.SelectedItem = item;
                        currencyFound = true;
                        break;
                    }

                if (currencyFound)
                    CboCurrency.IsEnabled = false;
                else
                    CboCurrency.SelectedIndex = 0;
            }
            else
            {
                CboCurrency.SelectedIndex = 0;
            }

            SetUIForDirection();
            await LoadExchangeRatesAsync();
            UpdateRate();
        }

        private void BtnSwapDirection_Click(object sender, RoutedEventArgs e)
        {
            isBuying = !isBuying;
            SetUIForDirection();
            CalculateTotal();
        }

        private void SetUIForDirection()
        {
            CboTopCard.Items.Clear();
            CboBottomCard.Items.Clear();

            if (isBuying) // КУПІВЛЯ
            {
                TxtTitle.Text = "Купівля валюти";
                HintAssist.SetHint(CboTopCard, "Списання (Гривнева картка)");
                HintAssist.SetHint(CboBottomCard, "Зарахування (Валютна картка)");

                foreach (var card in _uahCards)
                    CboTopCard.Items.Add($"{card.CardNumber} (Баланс: {card.Balance:F2} ₴)");
                CboTopCard.IsEnabled = true;

                CboBottomCard.Items.Add($"{_currencyCard.CardNumber} (Баланс: {_currencyCard.Balance:F2} {CboCurrency.Text})");
                CboBottomCard.IsEnabled = false;

                LblTotalText.Text = "До сплати:";
                LblTotalPayable.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                BtnConfirmExchange.Content = "КУПИТИ";
            }
            else // ПРОДАЖ
            {
                TxtTitle.Text = "Продаж валюти";
                HintAssist.SetHint(CboTopCard, "Списання (Валютна картка)");
                HintAssist.SetHint(CboBottomCard, "Зарахування (Гривнева картка)");

                CboTopCard.Items.Add($"{_currencyCard.CardNumber} (Баланс: {_currencyCard.Balance:F2} {CboCurrency.Text})");
                CboTopCard.IsEnabled = false;

                foreach (var card in _uahCards)
                    CboBottomCard.Items.Add($"{card.CardNumber} (Баланс: {card.Balance:F2} ₴)");
                CboBottomCard.IsEnabled = true; // Дозволяємо обрати, на яку картку скинути гривні!

                LblTotalText.Text = "Ви отримаєте:";
                LblTotalPayable.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                BtnConfirmExchange.Content = "ПРОДАТИ";
            }

            if (CboTopCard.Items.Count > 0) CboTopCard.SelectedIndex = 0;
            if (CboBottomCard.Items.Count > 0) CboBottomCard.SelectedIndex = 0;
        }

        private async Task LoadExchangeRatesAsync()
        {
            try
            {
                string url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
                string jsonResponse = await _httpClient.GetStringAsync(url);

                var allRates = JsonSerializer.Deserialize<List<CurrencyRate>>(jsonResponse);
                _exchangeRates = allRates ?? new List<CurrencyRate>();
            }
            catch (Exception ex)
            {
                _exchangeRates = new List<CurrencyRate>();
            }
        }

        private void UpdateRate()
        {
            if (CboCurrency.SelectedItem is ComboBoxItem selectedItem)
            {
                string targetCurrency = selectedItem.Content.ToString();

                var rateEntity = _exchangeRates.FirstOrDefault(r => r.cc == targetCurrency);
                if (rateEntity != null)
                {
                    _currentRate = (decimal)rateEntity.rate;
                }
                else
                {
                    _currentRate = targetCurrency switch
                    {
                        "USD" => 41.30m,
                        "EUR" => 44.50m,
                        "PLN" => 10.20m,
                        "GBP" => 54.20m,
                        "XAU" => 108540.30m,
                        "XAG" => 1250.50m,
                        _ => 1.00m
                    };
                }

                LblCurrentRate.Text = _currentRate.ToString("F2", CultureInfo.InvariantCulture);
                CalculateTotal();
            }
        }

        private bool TryParseAmount(string text, out decimal amount)
        {
            string normalized = text.Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }

        private void CalculateTotal()
        {
            if (TryParseAmount(TxtAmount.Text, out decimal amount))
            {
                decimal totalUah = amount * _currentRate;
                LblTotalPayable.Text = totalUah.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
                LblTotalPayable.Text = "0.00";
        }

        private void CboCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateRate();
            if (!isBuying) SetUIForDirection();
        }

        private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e) => CalculateTotal();

        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9,.]$")) { e.Handled = true; return; }
            if ((e.Text == "." || e.Text == ",") && (TxtAmount.Text.Contains(".") || TxtAmount.Text.Contains(","))) e.Handled = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Window.GetWindow(this)?.Close();

        private void BtnConfirmExchange_Click(object sender, RoutedEventArgs e)
        {
            if (CboTopCard.SelectedIndex == -1 || CboBottomCard.SelectedIndex == -1)
            {
                MessageBox.Show("Оберіть обидві картки для обміну!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseAmount(TxtAmount.Text, out decimal foreignAmount) || foreignAmount <= 0)
            {
                MessageBox.Show("Введіть коректну суму!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string targetCurrency = (CboCurrency.SelectedItem as ComboBoxItem).Content.ToString();

            if (!string.IsNullOrEmpty(_currencyCard.CurrencyType) && _currencyCard.CurrencyType != targetCurrency)
            {
                MessageBox.Show($"Ця картка закріплена за валютою {_currencyCard.CurrencyType}. Обмін на {targetCurrency} заборонено.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string sourceCardStr = CboTopCard.SelectedItem.ToString().Split(' ')[0];
            string targetCardStr = CboBottomCard.SelectedItem.ToString().Split(' ')[0];

            try
            {
                using (var db = new Banking_system.DataBase.Database())
                {
                    var uahCardNum = isBuying ? sourceCardStr : targetCardStr;
                    var valCardNum = isBuying ? targetCardStr : sourceCardStr;

                    var uahCard = db.Cards.FirstOrDefault(c => c.CardNumber == uahCardNum);
                    var valCard = db.Cards.FirstOrDefault(c => c.CardNumber == valCardNum);

                    if (uahCard == null || valCard == null)
                    {
                        MessageBox.Show("Помилка зчитування карток з бази даних.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    decimal totalUah = foreignAmount * _currentRate;

                    if (isBuying)
                    {
                        if (uahCard.Balance < totalUah)
                        {
                            MessageBox.Show($"Недостатньо коштів на гривневій картці!\nНеобхідно: {totalUah:F2} ₴\nДоступно: {uahCard.Balance:F2} ₴", "Відмова", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        uahCard.Balance -= totalUah;
                        valCard.Balance += foreignAmount;
                    }
                    else
                    {
                        if (valCard.Balance < foreignAmount)
                        {
                            MessageBox.Show($"Недостатньо валюти на картці!\nНеобхідно: {foreignAmount:F2} {targetCurrency}\nДоступно: {valCard.Balance:F2} {targetCurrency}", "Відмова", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        valCard.Balance -= foreignAmount;
                        uahCard.Balance += totalUah;
                    }

                    if (valCard is CurrencyCard cc && string.IsNullOrEmpty(cc.CurrencyType))
                        cc.CurrencyType = targetCurrency;

                    db.SaveChanges();

                    string actionName = isBuying ? "Купівля" : "Продаж";
                    Logger.AppendSystemLog(_user.Email, $"{actionName} валюти: {foreignAmount:F2} {targetCurrency} за курсом {_currentRate:F2}. Зміна балансу гривні: {totalUah:F2} ₴.");
                }

                string actionMsg = isBuying ? "придбано" : "продано";
                MessageBox.Show($"Успішно {actionMsg} {foreignAmount:F2} {targetCurrency}!", "Операція успішна", MessageBoxButton.OK, MessageBoxImage.Information);
                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Внутрішня помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}