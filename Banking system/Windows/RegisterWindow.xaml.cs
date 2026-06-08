using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Banking_system.Windows
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
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


        private void BtnBackToLogin_Click(object sender, RoutedEventArgs e)
        {

            this.Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {

            MessageBox.Show("Тут незабаром буде логіка перевірки полів та реєстрації!",
                            "Реєстрація",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
        }
    }
}