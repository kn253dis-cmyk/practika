using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Banking_system.Models; // Підключаємо моделі, де лежить CurrencyRate

namespace Banking_system.Windows
{
    public partial class ExchangeWindow : Window
    {
        public ExchangeWindow()
        {
            InitializeComponent();
            // Запускаємо завантаження даних асинхронно при відкритті вікна
            _ = LoadExchangeRatesAsync();
        }

        private async Task LoadExchangeRatesAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = "https://bank.gov.ua/NBUStatService/v1/statdirectory/exchange?json";
                    string jsonResponse = await client.GetStringAsync(url);

                    // Перетворюємо JSON від Нацбанку в список об'єктів CurrencyRate
                    var allRates = JsonSerializer.Deserialize<List<CurrencyRate>>(jsonResponse);

                    if (allRates != null)
                    {
                        // Відбираємо тільки найважливіші валюти
                        var popularCodes = new List<string> { "USD", "EUR", "PLN", "GBP", "XAU", "XAG" };
                        var filteredRates = allRates.Where(rate => popularCodes.Contains(rate.cc)).ToList();

                        // Передаємо ці дані прямо в нашу таблицю
                        CurrencyGrid.ItemsSource = filteredRates;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити курси: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove(); // Дозволяє тягати вікно мишкою
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}