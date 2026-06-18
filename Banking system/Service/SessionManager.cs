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

        public static void CheckAndProcessCredits(int userId)
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
                            decimal interest = Math.Abs(card.Balance) * (card.InterestRate / 100m);

                            card.Balance -= interest;

                            card.MissedPaymentsCount++;
                            card.DueDate = card.DueDate.AddMonths(1); 
                            changesMade = true;

                            var user = db.Users.Find(card.UserId);
                            if (user != null)
                            {
                                var receiptData = new Dictionary<string, string>
                                {
                                    { "UserName", $"{user.Surname} {user.Name}" },
                                    { "CardNumber", card.CardNumber },
                                    { "PlanName", card.CreditType },
                                    { "CreditLimit", card.CreditLimit.ToString("F2") },
                                    { "CurrentDebt", Math.Abs(card.Balance).ToString("F2") },
                                    { "AccruedInterest", interest.ToString("F2") }, 
                                    { "CreditEndDate", card.DueDate.ToString("dd.MM.yyyy") },
                                    { "Date", DateTime.Now.ToString("dd.MM.yyyy") }
                                };

                                Logger.AppendLog(
                                    userEmail: user.Email,
                                    templateName: "LoanReminderReceipt",
                                    text: $"⚠️ Нарахування штрафу за кредитом. Ваш поточний борг: {Math.Abs(card.Balance):N2} ₴",
                                    data: receiptData
                                );
                            }
                        }

                        if (card.MissedPaymentsCount > 0 && !card.IsBlocked)
                        {
                            card.IsBlocked = true;
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
                Logger.Log($"Помилка перевірки кредитів: {ex.Message}");
            }
        }


        public static bool IsUserBlacklisted(int userId)
        {
            using (var db = new Database())
            {
                return db.Cards.OfType<CreditCard>()
                    .Any(c => c.UserId == userId && c.IsBlocked);
            }
        }
    }
}