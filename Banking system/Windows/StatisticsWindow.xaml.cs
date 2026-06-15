using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace Banking_system.Windows
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(string cardNumber)
        {
            InitializeComponent();

            // Підставляємо номер картки (показуємо лише останні 4 цифри для безпеки)
            if (!string.IsNullOrEmpty(cardNumber) && cardNumber.Length >= 4)
            {
                string last4 = cardNumber.Substring(cardNumber.Length - 4);
                TxtCardNumber.Text = $"Статистика для: **** **** **** {last4}";
            }

            // Викликаємо метод завантаження даних з бази
            LoadRealStatistics(cardNumber);
        }

        private void LoadRealStatistics(string cardNumber)
        {
            try
            {
                using (var db = new Banking_system.DataBase.Database())
                {
                // УВАГА: Якщо у вашій базі даних таблиця називається не Transactions, 
                // або властивості транзакцій мають інші назви, зміни їх тут!


                //using (var db = new Banking_system.DataBase.Database())
                //{
                //    // УВАГА: Якщо у вашій базі даних таблиця називається не Transactions, 
                //    // або властивості транзакцій мають інші назви, зміни їх тут!


                //    /* ТИМЧАСОВО ЗАКОМЕНТОВАНО (розкоментуй, коли таблиця транзакцій буде готова):
                    
                //    var allTransactions = db.Transactions.Where(t => t.CardNumber == cardNumber).ToList();

                //    // Рахуємо доходи (поповнення) та витрати (зняття, перекази іншим)
                //    // Заміни "Amount" та "TransactionType" на ті назви, які ви прописали в AbstractTransaction.cs
                //    decimal totalIncome = allTransactions.Where(t => t.TransactionType == "Deposit").Sum(t => t.Amount);
                //    decimal totalExpense = allTransactions.Where(t => t.TransactionType != "Deposit").Sum(t => t.Amount);

                //    // Оновлюємо інтерфейс реальними цифрами
                //    TxtTotalIncome.Text = $"+ {totalIncome:N2} ₴";
                //    TxtTotalExpense.Text = $"- {totalExpense:N2} ₴";
                //    TxtMainBalance.Text = $"{totalExpense:N0} ₴"; // Витрачено за місяць

                //    // Розраховуємо прогрес-бари (співвідношення)
                //    decimal totalTurnover = totalIncome + totalExpense;
                //    if (totalTurnover > 0)
                //    {
                //        IncomeProgress.Value = (double)((totalIncome / totalTurnover) * 100);
                //        ExpenseProgress.Value = (double)((totalExpense / totalTurnover) * 100);
                //    }
                //    */
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не вдалося завантажити статистику з бази: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}