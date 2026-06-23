using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Banking_system.DataBase;
using Banking_system.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Banking_system.Windows
{
    public class CategoryStat
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public decimal Total { get; set; }
        public string ColorHex { get; set; }
        public string IconKind { get; set; }
        public double Percentage { get; set; }

        public string PercentageText => $"{Percentage:F1}% від витрат ({Count} транз.)";
        public string TotalText => $"{Total:N0} ₴";
    }

    public class PieSegment
    {
        public Brush ColorBrush { get; set; }
        public Geometry Geometry { get; set; }
        public string ToolTipText { get; set; } 
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

                        var expenseTransactions = allTransactions.Where(t => t is WithdrawTransaction || t is TransferTransaction).ToList();

                        var categoryStats = expenseTransactions
                            .GroupBy(t =>
                            {

                                if (t is TransferTransaction) return "Перекази іншим";

                                return string.IsNullOrWhiteSpace(t.TransactionTarget) ? "Інше" : t.TransactionTarget;
                            })
                            .Select(g =>
                            {
                                string categoryName = g.Key;
                                string icon = "Cash";
                                string color = "#7F8C8D";

                                if (categoryName == "Перекази іншим") { icon = "SwapHorizontal"; color = "#9C27B0"; }
                                else if (categoryName.Contains("Супермаркети")) { icon = "Cart"; color = "#2E7D32"; }
                                else if (categoryName.Contains("Кафе")) { icon = "FoodForkDrink"; color = "#E65100"; }
                                else if (categoryName.Contains("Підписки")) { icon = "PlayCircle"; color = "#1565C0"; }
                                else if (categoryName.Contains("Готівка")) { icon = "Atm"; color = "#8E44AD"; }
                                else if (categoryName.Contains("Аптеки")) { icon = "Pharmacy"; color = "#D32F2F"; }
                                else if (categoryName.Contains("Транспорт")) { icon = "Bus"; color = "#F39C12"; }

                                decimal sum = g.Sum(x => x.Amount);
                                double percentage = totalExpense > 0 ? (double)(sum / totalExpense * 100) : 0;

                                return new CategoryStat
                                {
                                    Name = categoryName,
                                    Count = g.Count(),
                                    Total = sum,
                                    IconKind = icon,
                                    ColorHex = color,
                                    Percentage = percentage
                                };
                            })
                            .OrderByDescending(c => c.Total)
                            .ToList();

                        CategoriesList.ItemsSource = categoryStats;
                        DrawPieChart(categoryStats);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження: {ex.Message}");
            }
        }

        private void DrawPieChart(List<CategoryStat> stats)
        {
            var segments = new List<PieSegment>();
            double currentAngle = 0;
            double radius = 60;
            Point center = new Point(radius, radius);

            foreach (var stat in stats)
            {
                if (stat.Percentage <= 0) continue;

                double sweepAngle = (stat.Percentage / 100.0) * 360.0;

                if (sweepAngle >= 360) sweepAngle = 359.99;

                double startAngleRad = (currentAngle - 90) * Math.PI / 180.0;
                double endAngleRad = (currentAngle + sweepAngle - 90) * Math.PI / 180.0;

                Point startPoint = new Point(center.X + radius * Math.Cos(startAngleRad), center.Y + radius * Math.Sin(startAngleRad));
                Point endPoint = new Point(center.X + radius * Math.Cos(endAngleRad), center.Y + radius * Math.Sin(endAngleRad));

                var pathFigure = new PathFigure { StartPoint = center, IsClosed = true };
                pathFigure.Segments.Add(new LineSegment(startPoint, false));
                pathFigure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0, sweepAngle > 180, SweepDirection.Clockwise, false));

                var pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                segments.Add(new PieSegment
                {
                    Geometry = pathGeometry,
                    ColorBrush = (Brush)new BrushConverter().ConvertFrom(stat.ColorHex),
                    ToolTipText = $"{stat.Name}\n{stat.Total:N2} ₴ ({stat.Percentage:F1}%)"
                });

                currentAngle += sweepAngle;
            }

            PieChartControl.ItemsSource = segments;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}