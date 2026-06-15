using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Banking_system.DataBase;
using Banking_system.Models.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Banking_system.Windows
{
    // Клас для прив'язки даних до UI
    public class CategoryStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
        public string ColorHex { get; set; }
        public string IconKind { get; set; }

        public string CountText => $"{Count} транзакцій";
        public string TotalText => $"{Total:N0} ₴";
    }

    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(string cardNumber)
        {
            InitializeComponent();

            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 4)
            {
                string last4 = cardNumber.Substring(cardNumber.Length - 4);
                TxtCardNumber.Text = $"Статистика для: **** **** **** {last4}";
            }

            LoadRealStatistics(cardNumber);
        }

        private void LoadRealStatistics(string cardNumber)
        {
            try
            {
                using (var db = new Database())
                {
                    var card = db.Cards
                                 .Include(c => c.LastTransactions)
                                 .FirstOrDefault(c => c.CardNumber == cardNumber);

                    if (card != null && card.LastTransactions != null)
                    {
                        var allTransactions = card.LastTransactions.ToList();

                        decimal totalIncome = allTransactions.Where(t => t is DepositTransaction).Sum(t => t.Amount);
                        decimal totalExpense = allTransactions.Where(t => t is WithdrawTransaction || t is TransferTransaction).Sum(t => t.Amount);

                        TxtTotalIncome.Text = $"+ {totalIncome:N2} ₴";
                        TxtTotalExpense.Text = $"- {totalExpense:N2} ₴";

                        decimal totalTurnover = totalIncome + totalExpense;
                        if (totalTurnover > 0)
                        {
                            IncomeProgress.Value = (double)((totalIncome / totalTurnover) * 100);
                            ExpenseProgress.Value = (double)((totalExpense / totalTurnover) * 100);
                        }
                        else
                        {
                            IncomeProgress.Value = 0;
                            ExpenseProgress.Value = 0;
                        }

                        // === ГРУПУВАННЯ ЗА КАТЕГОРІЯМИ ===
                        // Беремо тільки витрати (WithdrawTransaction)
                        var expenseTransactions = allTransactions.OfType<WithdrawTransaction>().ToList();

                        var categoryStats = expenseTransactions
                            .GroupBy(t => string.IsNullOrWhiteSpace(t.TransactionTarget) ? "Інше" : t.TransactionTarget)
                            .Select(g =>
                            {
                                string categoryName = g.Key;

                                // Підбираємо іконку та колір залежно від назви категорії
                                string icon = "Cash";
                                string color = "#7F8C8D"; // Сірий за замовчуванням

                                if (categoryName.Contains("Супермаркети")) { icon = "Cart"; color = "#2E7D32"; }
                                else if (categoryName.Contains("Кафе")) { icon = "FoodForkDrink"; color = "#E65100"; }
                                else if (categoryName.Contains("Підписки")) { icon = "PlayCircle"; color = "#1565C0"; }
                                else if (categoryName.Contains("Готівка")) { icon = "Atm"; color = "#8E44AD"; }
                                else if (categoryName.Contains("Аптеки")) { icon = "Pharmacy"; color = "#D32F2F"; }
                                else if (categoryName.Contains("Транспорт")) { icon = "Bus"; color = "#F39C12"; }

                                return new CategoryStat
                                {
                                    Name = categoryName,
                                    Count = g.Count(),
                                    Total = g.Sum(x => x.Amount),
                                    IconKind = icon,
                                    ColorHex = color
                                };
                            })
                            .OrderByDescending(c => c.Total) // Сортуємо від найбільших витрат
                            .ToList();

                        // Прив'язуємо згруповані дані до інтерфейсу
                        CategoriesList.ItemsSource = categoryStats;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}