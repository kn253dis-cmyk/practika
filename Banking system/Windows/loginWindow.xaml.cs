using Banking_system.Windows;
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

        private void BtnClose_Click(object sender, RoutedEventArgs e)=>Application.Current.Shutdown();
        

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void BtnOpenRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 1. Спочатку створюємо вікно (якщо в XAML є помилка, вона впаде ТУТ)
                Banking_system.Windows.RegisterWindow registerWindow = new Banking_system.Windows.RegisterWindow();

                // 2. Ховаємо вікно логіну
                this.Hide();

                // 3. Показуємо реєстрацію
                registerWindow.ShowDialog();

                // 4. Повертаємо логін після закриття реєстрації
                this.Show();
            }
            catch (Exception ex)
            {
                // Цей блок спіймає аварійне закриття і покаже вікно з описом проблеми
                MessageBox.Show($"Сталася помилка при відкритті вікна реєстрації:\n\n{ex.Message}\n\nВнутрішня помилка: {ex.InnerException?.Message}",
                                "Помилка ініціалізації",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);

                // Повертаємо вікно авторизації, якщо воно встигло сховатися
                this.Show();
            }
        }
    }

}