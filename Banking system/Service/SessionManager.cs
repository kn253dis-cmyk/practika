using System;
using System.Linq;
using System.Windows;
using Banking_system.Entity;
using Banking_system.Models;
using Banking_system.DataBase;

namespace Banking_system.Service
{
    public enum UserRole
    {
        Client,
        Admin
    }

    public static class SessionManager
    {
        // Поточний авторизований користувач
        public static User? CurrentUser { get; private set; }

        // Роль поточного користувача
        public static UserRole CurrentRole { get; private set; }

        // Швидка перевірка: чи увійшов хтось у систему?
        public static bool IsLoggedIn => CurrentUser != null;

        // Швидка перевірка: чи є поточний користувач Адміном?
        public static bool IsAdmin => CurrentRole == UserRole.Admin;

        // Метод для входу в систему
        public static void Login(User user)
        {
            CurrentUser = user;

            if (user.Email == "admin@gmail.com")
            {
                CurrentRole = UserRole.Admin;
            }
            else
            {
                CurrentRole = UserRole.Client;

                CheckAndProcessCredits(user.ID);
            }
        }

        public static void Logout()
        {
            CurrentUser = null;
            CurrentRole = UserRole.Client;
        }

        // СПЕЦІАЛЬНИЙ МЕТОД: Керування видимістю кнопок/сторінок
        public static void RequireAdminAccess(UIElement uiElement)
        {
            if (IsAdmin)
            {
                uiElement.Visibility = Visibility.Visible; // Показуємо адміну
            }
            else
            {
                uiElement.Visibility = Visibility.Collapsed; // Повністю ховаємо від клієнта
            }
        }

        private static void CheckAndProcessCredits(int userId)
        {
            try
            {
                using (var db = new Database())
                {
                    bool changesMade = false;

                    var creditCards = db.Cards.OfType<CreditCard>().Where(c => c.UserId == userId).ToList();

                    foreach (var card in creditCards)
                    {
                        while (card.Balance < 0 && DateTime.Now > card.DueDate)
                        {
                            decimal debtAmount = Math.Abs(card.Balance);
                            decimal penalty = debtAmount * (card.InterestRate / 100m);

                            card.AccruedInterest += penalty;

                            card.MissedPaymentsCount++;

                            if (card.MissedPaymentsCount >= 2)
                            {
                                card.IsBlocked = true;
                            }

                            card.DueDate = card.DueDate.AddMonths(1);

                            changesMade = true;
                        }
                    }

                    if (changesMade)
                    {
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Помилка під час автоматичної перевірки кредитів: {ex.Message}");
            }
        }
    }
}