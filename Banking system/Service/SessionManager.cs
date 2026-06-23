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
        public static User? CurrentUser { get; private set; }
        public static UserRole CurrentRole { get; private set; }
        public static bool IsLoggedIn => CurrentUser != null;
        public static bool IsAdmin => CurrentRole == UserRole.Admin;


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
        //public static void RequireAdminAccess(UIElement uiElement)
        //{
        //    if (IsAdmin)
        //    {
        //        uiElement.Visibility = Visibility.Visible; // Показуємо адміну
        //    }
        //    else
        //    {
        //        uiElement.Visibility = Visibility.Collapsed; // Повністю ховаємо від клієнта
        //    }
        //}

        public static void CheckAndProcessCredits(int userId)
        {
            try
            {
                using (var db = new Banking_system.DataBase.Database())
                {
                    var user = db.Users.Find(userId);
                    if (user == null) return;

                    bool changesMade = false;
                    var creditCards = db.Cards.OfType<CreditCard>().Where(c => c.UserId == userId).ToList();

                    var emailService = new Banking_system.Service.EmailService();

                    foreach (var card in creditCards)
                    {
                        var result = card.ProcessRoutine();

                        if (result.Action == CardAction.SendWarning)
                        {
                            var receiptData = new Dictionary<string, string>
                            {
                                { "UserName", $"{user.Surname} {user.Name}" },
                                { "CardNumber", card.CardNumber },
                                { "PlanName", card.CreditType },
                                { "CreditLimit", card.CreditLimit.ToString("F2") },
                                { "CurrentDebt", Math.Abs(card.Balance).ToString("F2") },
                                { "AccruedInterest", "0.00" },
                                { "CreditEndDate", card.DueDate.ToString("dd.MM.yyyy") },
                                { "Date", DateTime.Now.ToString("dd.MM.yyyy") }
                            };

                            Logger.AppendLog(
                                userEmail: user.Email,
                                templateName: "LoanReminderReceipt",
                                text: $"⚠️ Увага! До нарахування відсотків залишилось менше 7 днів. Залишилось місяців за планом: {result.MonthsLeft}.",
                                data: receiptData
                            );

                            string htmlContent = emailService.PrepareReceiptHtml("LoanReminderReceipt", receiptData);
                            Task.Run(() => emailService.SendEmailAsync(user.Email, "Увага! Наближається платіж за кредитом", htmlContent));

                            changesMade = true;
                        }
                        else if (result.Action == CardAction.InterestApplied)
                        {
                            var receiptData = new Dictionary<string, string>
                            {
                                { "UserName", $"{user.Surname} {user.Name}" },
                                { "CardNumber", card.CardNumber },
                                { "PlanName", card.CreditType },
                                { "CreditLimit", card.CreditLimit.ToString("F2") },
                                { "CurrentDebt", Math.Abs(card.Balance).ToString("F2") },
                                { "AccruedInterest", result.InterestAmount.ToString("F2") },
                                { "CreditEndDate", card.DueDate.ToString("dd.MM.yyyy") },
                                { "Date", DateTime.Now.ToString("dd.MM.yyyy") }
                            };

                            Logger.AppendLog(
                                userEmail: user.Email,
                                templateName: "LoanReminderReceipt",
                                text: $"⚠️ Нараховано відсотки. Ваш поточний борг: {Math.Abs(card.Balance):N2} ₴",
                                data: receiptData
                            );

                            string htmlContent = emailService.PrepareReceiptHtml("LoanReminderReceipt", receiptData);
                            Task.Run(() => emailService.SendEmailAsync(user.Email, "Нараховано відсотки за кредитною лінією", htmlContent));

                            changesMade = true;
                        }
                        else if (result.Action == CardAction.Blacklisted)
                        {
                            if (!user.IsBlacklisted)
                            {
                                user.IsBlacklisted = true;
                                Logger.Log($"Користувач {user.Email} потрапив до Чорного списку банку.");
                            }
                            changesMade = true;
                        }
                        else if (result.Action == CardAction.Blocked)
                        {
                            var receiptData = new Dictionary<string, string>
                            {
                                { "UserName", $"{user.Surname} {user.Name}" },
                                { "CardNumber", card.CardNumber },
                                { "PlanName", card.CreditType },
                                { "CreditLimit", card.CreditLimit.ToString("F2") },
                                { "CurrentDebt", Math.Abs(card.Balance).ToString("F2") },
                                { "AccruedInterest", "0.00" }, 
                                { "CreditEndDate", card.TermEndDate.AddMonths(1).ToString("dd.MM.yyyy") },
                                { "Date", DateTime.Now.ToString("dd.MM.yyyy") }
                            };

                            Logger.AppendLog(
                                userEmail: user.Email,
                                templateName: "LoanReminderReceipt",
                                text: $"⛔ Ваш кредитний план завершився. У Вас є 1 місяць (до {card.TermEndDate.AddMonths(1):dd.MM.yyyy}) для погашення боргу, інакше Вас буде внесено до Чорного списку.",
                                data: receiptData
                            );

                            string htmlContent = emailService.PrepareReceiptHtml("LoanReminderReceipt", receiptData);
                            Task.Run(() => emailService.SendEmailAsync(user.Email, "⛔ Картку заблоковано! Останнє попередження", htmlContent));

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
                var user = db.Users.Find(userId);
                return user != null && user.IsBlacklisted;
            }
        }
    }
}