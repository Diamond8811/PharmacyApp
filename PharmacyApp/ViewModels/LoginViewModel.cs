using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Dto;
using PharmacyApp.Services;
using System;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _login;
        public string Login { get => _login; set => SetProperty(ref _login, value); }

        private string _password;
        public string Password { get => _password; set => SetProperty(ref _password, value); }

        public IRelayCommand SignInCommand { get; }

        public event Action<SessionUser> LoginSucceeded;

        public LoginViewModel()
        {
            SignInCommand = new RelayCommand(SignIn);
        }

        private void SignIn()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Password))
                {
                    MessageBox.Show("Введите логин и пароль.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var user = EmployeeService.Authenticate(Login.Trim(), Password);
                if (user == null)
                {
                    MessageBox.Show("Неверный логин/пароль или пользователь отключён.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                LoginSucceeded?.Invoke(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка входа: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}