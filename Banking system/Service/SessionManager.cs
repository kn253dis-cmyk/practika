using System.Windows;
using Banking_system.Entity;

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
    }
}



/*
ПОТРІБНО ДОДАТИ ДО СТОРІНКИ АВТОРИЗАЦІЇ
if (foundUser != null && foundUser.Password == db.HashPassword(TxtPassword.Password))
{
    // ЗАПАМ'ЯТОВУЄМО КОРИСТУВАЧА У СЕСІЇ!
    SessionManager.Login(foundUser);
    
    // Відкриваємо головне вікно програми...
}
 

 ЩОБ ДІЗНАТИСЬ ПОТОЧНОГО КОРИСТУВАЧА
 string currentEmail = SessionManager.CurrentUser.Email;
 string currentName = SessionManager.CurrentUser.Name;
 


ДЛЯ ПРИХОВУВАННЯ КНОПОК АДМІНА ВІД КЛІЄНТА
public ГОЛОВНА СТОРІНКА()
{
    InitializeComponent();

    // Якщо увійшов звичайний клієнт — кнопка просто зникне.
    // Якщо увійшов "admin@bank.com" — кнопка буде видимою.
    SessionManager.RequireAdminAccess(BtnAdminPanel);
}
 */