using Banking_system.Entity;
using Banking_system.Models;
using Banking_system.Models.Transactions;
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

namespace Banking_system.Pages
{
    public partial class CurrencyExchangePage : Page
    {
        private User _user;
        private CurrencyCard _currencyCard;
        private decimal _currentRate = 41.50m;
        private List<CurrencyRate> _exchangeRates = new List<CurrencyRate>();
        private static readonly HttpClient _httpClient = new HttpClient();

        public CurrencyExchangePage(User user, CurrencyCard currencyCard)
        {
            InitializeComponent();
            _user = user;
            _currencyCard = currencyCard;

            _ = InitializeDataAsync();
        }

        private async Task InitializeDataAsync()
        {
            // 1. Автоматично підставляємо номер валютної картки
            TxtTargetCard.Text = _currencyCard.CardNumber;

            // 2. Встановлюємо валюту картки як обрану за замовчуванням, якщо вона вже задана
            if (!string.IsNullOrEmpty(_currencyCard.CurrencyType))
            {
                bool currencyFound = false;
                foreach (ComboBoxItem item in CboCurrency.Items)
                {
                    if (item.Content.ToString() == _currencyCard.CurrencyType)
                    {
                        CboCurrency.SelectedItem = item;
                        currencyFound = true;
                        break;
                    }
                }

                // Якщо валюта картки не входить до списку (наприклад, GBP) - підставляємо перший пункт,
                // інакше ComboBox лишиться без вибору і курс/сума ніколи не розрахуються
                if (!currencyFound)
                    CboCurrency.SelectedIndex = 0;
            }
            else
                CboCurrency.SelectedIndex = 0; // Якщо пуста, ставимо USD

            // 3. Завантажуємо картки користувача для списання (тільки гривневі)
            using (var db = new DataBase.Database())
            {
                var availableCards = db.Cards
                    .Where(c => c.UserId == _user.ID && c.CardNumber != _currencyCard.CardNumber && !(c is CurrencyCard))
                    .ToList();

                foreach (var card in availableCards)
                    CboSourceCard.Items.Add($"{card.CardNumber} (Баланс: {card.Balance:F2} ₴)");
            }

            if (CboSourceCard.Items.Count > 0) CboSourceCard.SelectedIndex = 0;

            // 4. Завантажуємо реальні курси з НБУ - так само, як це робить ExchangeWindow
            await LoadExchangeRatesAsync();

            UpdateRate();
        }

        // Завантаження курсів валют з API Нацбанку (ідентично до ExchangeWindow.LoadExchangeRatesAsync)
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
                // Якщо НБУ недоступний - лишаємо порожній список, UpdateRate() підставить орієнтовні значення
                _exchangeRates = new List<CurrencyRate>();
                MessageBox.Show(
                    $"Не вдалося завантажити актуальний курс НБУ, буде використано орієнтовне значення: {ex.Message}",
                    "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Динамічне отримання курсу валют (тепер бере дані з кешу, завантаженого з НБУ)
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
                    // Фолбек-значення, якщо НБУ недоступний або валюти немає у відповіді
                    _currentRate = targetCurrency switch
                    {
                        "USD" => 41.30m,
                        "EUR" => 44.50m,
                        "PLN" => 10.20m,
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
                decimal totalPayable = amount * _currentRate;
                LblTotalPayable.Text = totalPayable.ToString("F2", CultureInfo.InvariantCulture);
            }
            else
                LblTotalPayable.Text = "0.00";
        }

        private void CboCurrency_SelectionChanged(object sender, SelectionChangedEventArgs e) =>UpdateRate();

        private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e)=>CalculateTotal();
        
        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9,.]$"))
            {
                e.Handled = true;
                return;
            }

            if ((e.Text == "." || e.Text == ",") &&(TxtAmount.Text.Contains(".") || TxtAmount.Text.Contains(",")))
                e.Handled = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnConfirmExchange_Click(object sender, RoutedEventArgs e)
        {
            if (CboSourceCard.SelectedIndex == -1)
            {
                MessageBox.Show("Оберіть картку для списання коштів!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseAmount(TxtAmount.Text, out decimal foreignAmount) || foreignAmount <= 0)
            {
                MessageBox.Show("Введіть коректну суму для обміну!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string selectedText = CboSourceCard.SelectedItem.ToString();
            string sourceCardNumber = selectedText.Split(' ')[0];
            string targetCurrency = (CboCurrency.SelectedItem as ComboBoxItem).Content.ToString();

            var exchangeTransaction = new CurrencyExchangeTransaction(
                sourceCardNumber: sourceCardNumber,
                targetCardNumber: _currencyCard.CardNumber,
                foreignAmount: foreignAmount,
                exchangeRate: _currentRate,
                currencyCode: targetCurrency
            );

            bool success = exchangeTransaction.Execute();

            if (success)
            {
                MessageBox.Show($"Успішно придбано {foreignAmount:F2} {targetCurrency}!", "Операція успішна", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.GoBack();
            }
            else
                MessageBox.Show("Недостатньо коштів на картці списання або сталася помилка.", "Помилка транзакції", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void CboSourceCard_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Можна додати додаткову логіку перевірки лімітів карти списання
        }
    }
}