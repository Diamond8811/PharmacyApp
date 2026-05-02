using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Dto;
using System;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public SessionUser CurrentEmployee { get; }

        public bool IsManager =>
            string.Equals(CurrentEmployee?.RoleName, "Заведующий", StringComparison.OrdinalIgnoreCase);

        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                    ApplyTheme(value);
            }
        }

        private object _currentViewModel;
        public object CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (SetProperty(ref _currentViewModel, value))
                    CurrentViewModelChanged?.Invoke();
            }
        }

        public event Action CurrentViewModelChanged;

        public IRelayCommand<string> NavigateCommand { get; }
        public IRelayCommand LogoutCommand { get; }

        public MainViewModel(SessionUser employee)
        {
            CurrentEmployee = employee ?? throw new ArgumentNullException(nameof(employee));
            NavigateCommand = new RelayCommand<string>(Navigate);
            LogoutCommand = new RelayCommand(Logout);
            Navigate("Продажи");
        }

        private void ApplyTheme(bool isDark)
        {
            var dict = new ResourceDictionary();
            dict.Source = new Uri(isDark ? "/Resources/DarkTheme.xaml" : "/Resources/LightTheme.xaml", UriKind.Relative);
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(dict);
        }

        private void Navigate(string module)
        {
            switch (module)
            {
                case "Сотрудники":
                    if (!IsManager) return;
                    CurrentViewModel = new StaffViewModel();
                    break;
                case "Справочники":
                    if (!IsManager) return;
                    CurrentViewModel = new DrugsViewModel();
                    break;
                case "Поставки":
                    if (!IsManager) return;
                    CurrentViewModel = new DeliveryViewModel(CurrentEmployee.EmployeeID);
                    break;
                case "Продажи":
                    CurrentViewModel = new SalesViewModel(CurrentEmployee.EmployeeID, CurrentEmployee.FullName);
                    break;
                case "Склад":
                    CurrentViewModel = new InventoryViewModel();
                    break;
                case "Отчёты":
                    if (!IsManager) return;
                    CurrentViewModel = new ReportsViewModel();
                    break;
                case "Аналитика":
                    if (!IsManager) return;
                    CurrentViewModel = new AnalyticsViewModel();
                    break;
                default:
                    CurrentViewModel = new SalesViewModel(CurrentEmployee.EmployeeID, CurrentEmployee.FullName);
                    break;
            }
        }

        private void Logout() => Application.Current.Shutdown();
    }
}