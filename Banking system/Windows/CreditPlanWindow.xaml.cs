using Banking_system.Entity;
using Banking_system.Models;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Banking_system.Windows
{
    public partial class CreditPlanWindow : Window
    {
        private User _currentUser;
        public bool IsCardCreated { get; private set; } = false;

        // Змінні для збереження вибору клієнта
        private int _selectedLimit = 0;
        private string _selectedPlanName = "";
        private decimal _selectedRate = 0;
        private int _selectedMonths = 0;

        public CreditPlanWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            if (ChkAgreement.IsChecked != true)
            {
                MessageBox.Show("Будь ласка, ознайомтеся з інформацією та поставте відмітку про згоду.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            PanelInfo.Visibility = Visibility.Collapsed;

            PanelAction.Visibility = Visibility.Visible;

            CardsGrid.IsEnabled = true;

            SelectPlan(CardStandard, 50000, "Стандартний", 8.0m, 3);
        }

        private void SelectPlan(MaterialDesignThemes.Wpf.Card selectedCard, int limit, string name, decimal rate, int months)
        {
            CardMicro.BorderThickness = new Thickness(0);
            CardStandard.BorderThickness = new Thickness(0);
            CardPremium.BorderThickness = new Thickness(0);

            selectedCard.BorderThickness = new Thickness(3);

            if (selectedCard == CardMicro)
                selectedCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#38EF7D"));
            else if (selectedCard == CardStandard)
                selectedCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
            else if (selectedCard == CardPremium)
                selectedCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F1C40F"));

            _selectedLimit = limit;
            _selectedPlanName = name;
            _selectedRate = rate;
            _selectedMonths = months;
        }

        private void CardMicro_Click(object sender, MouseButtonEventArgs e) => SelectPlan(CardMicro, 10000, "Мікрозайм", 10.0m, 1);
        private void CardStandard_Click(object sender, MouseButtonEventArgs e) => SelectPlan(CardStandard, 50000, "Стандартний", 8.0m, 3);
        private void CardPremium_Click(object sender, MouseButtonEventArgs e) => SelectPlan(CardPremium, 250000, "Преміум", 6.0m, 6);


        private void BtnSubmit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedLimit == 0)
            {
                MessageBox.Show("Оберіть один із кредитних планів.", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                CreditCard newCard;

                using (var db = new Banking_system.DataBase.Database())
                {
                    newCard = new CreditCard
                    {
                        UserId = _currentUser.ID,
                        CreditLimit = _selectedLimit,
                        CreditType = _selectedPlanName,
                        InterestRate = _selectedRate,
                        DueDate = DateTime.Now.AddMonths(_selectedMonths)
                    };

                    db.Cards.Add(newCard);
                    db.SaveChanges();
                }

                var loanData = new Dictionary<string, string>
                {
                    { "Amount", _selectedLimit.ToString("F2") },
                    { "Date", DateTime.Now.ToString("dd.MM.yyyy HH:mm") },
                    { "CardNumber", newCard.CardNumber },
                    { "Percentage", _selectedRate.ToString("F1") },
                    { "CreditEndDate", newCard.DueDate.ToString("dd.MM.yyyy") },
                    { "CreditLimit", _selectedLimit.ToString("F2") },
                    { "PlanName", _selectedPlanName }
                };

                Logger.AppendLog(
                    userEmail: _currentUser.Email,
                    templateName: "LoanReceipt",
                    text: $"Оформлення кредиту: план «{_selectedPlanName}» на суму {_selectedLimit:N0} ₴",
                    data: loanData
                );

                IsCardCreated = true;
                MessageBox.Show($"Кредитну картку за планом '{_selectedPlanName}' успішно оформлено!\nВаш ліміт: {_selectedLimit:N0} ₴", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка оформлення кредиту: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}