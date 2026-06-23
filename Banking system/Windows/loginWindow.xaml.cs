using Banking_system.Entity;
using Banking_system.Views;
using Banking_system.Windows;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Banking_system
{
    public partial class loginWindow : Window
    {
        public loginWindow()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void BtnOpenRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Banking_system.Windows.RegisterWindow registerWindow = new Banking_system.Windows.RegisterWindow();
                this.Hide();
                registerWindow.ShowDialog();
                this.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка при відкритті вікна реєстрації:\n\n{ex.Message}\n\nВнутрішня помилка: {ex.InnerException?.Message}",
                                "Помилка ініціалізації",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                this.Show();
            }
        }

        private void logInButt_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PassBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Будь ласка, заповніть усі поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            User? user = null;

            try
            {
                // Підключаємося до БД і шукаємо користувача
                using (var db = new Banking_system.DataBase.Database())
                {
                    // ВИДАЛЕНО: db.Database.Migrate(); - База даних вже існує на сервері

                    string hashPassword = db.HashPassword(password);

                    // Шукаємо збіг по Email та хешованому паролю
                    user = db.Users
                        .Include(u => u.Cards)
                        .FirstOrDefault(u => u.Email == login && u.Password == hashPassword);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка бази даних під час входу: " + ex.Message, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (user != null)
            {
                Banking_system.Service.SessionManager.Login(user);
                MainWindow mainForm = new MainWindow(user);
                mainForm.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Невірний Email або пароль. Спробуйте ще раз.", "Помилка входу", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}