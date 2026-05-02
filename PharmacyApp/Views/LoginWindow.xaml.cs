using PharmacyApp.Models.Dto;
using PharmacyApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PharmacyApp.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _vm = new LoginViewModel();

        public LoginWindow()
        {
            InitializeComponent();
            DataContext = _vm;

            _vm.LoginSucceeded += VmOnLoginSucceeded;
        }

        private void VmOnLoginSucceeded(SessionUser user)
        {
            var main = new MainWindow(user);
            main.Show();
            Close();
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox pb)
                _vm.Password = pb.Password;
        }
    }
}
