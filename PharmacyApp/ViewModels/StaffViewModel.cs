using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Dto;
using PharmacyApp.Models.Drafts;
using PharmacyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class StaffViewModel : BaseViewModel
    {
        public ObservableCollection<EmployeeRow> Employees { get; } = new ObservableCollection<EmployeeRow>();
        public ObservableCollection<object> Roles { get; } = new ObservableCollection<object>();

        private EmployeeRow _selectedEmployee;
        public EmployeeRow SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                if (SetProperty(ref _selectedEmployee, value))
                    FromSelected();
            }
        }

        private EmployeeEditDraft _edit = new EmployeeEditDraft();
        public EmployeeEditDraft Edit { get => _edit; set => SetProperty(ref _edit, value); }

        public IRelayCommand LoadCommand { get; }
        public IRelayCommand NewCommand { get; }
        public IRelayCommand SaveCommand { get; }
        public IRelayCommand DeleteCommand { get; }

        public StaffViewModel()
        {
            LoadCommand = new RelayCommand(Load);
            NewCommand = new RelayCommand(New);
            SaveCommand = new RelayCommand(Save);
            DeleteCommand = new RelayCommand(Delete);

            Load();
        }

        private void Load()
        {
            try
            {
                Employees.Clear();
                Roles.Clear();

                foreach (var r in EmployeeService.GetRoles()) Roles.Add(r);
                foreach (var e in EmployeeService.GetEmployees()) Employees.Add(e);

                SelectedEmployee = Employees.FirstOrDefault();
                FromSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FromSelected()
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    Edit = new EmployeeEditDraft { IsActive = true };
                    return;
                }

                Edit = new EmployeeEditDraft
                {
                    EmployeeID = SelectedEmployee.EmployeeID,
                    FullName = SelectedEmployee.FullName,
                    RoleID = SelectedEmployee.RoleID,
                    Login = SelectedEmployee.Login,
                    IsActive = SelectedEmployee.IsActive
                };
            }
            catch
            {
                Edit = new EmployeeEditDraft { IsActive = true };
            }
        }

        private void New()
        {
            SelectedEmployee = null;
            Edit = new EmployeeEditDraft { IsActive = true };
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Edit.FullName) ||
                    string.IsNullOrWhiteSpace(Edit.Login) ||
                    Edit.RoleID <= 0)
                {
                    MessageBox.Show("Заполните обязательные поля: ФИО, Роль, Логин.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var ok = EmployeeService.SaveEmployee(
                    Edit.EmployeeID > 0 ? (int?)Edit.EmployeeID : null,
                    Edit.FullName.Trim(),
                    Edit.RoleID,
                    Edit.Login.Trim(),
                    Edit.NewPassword,
                    Edit.IsActive);

                if (!ok) return;

                Load();
                MessageBox.Show("Сохранено.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete()
        {
            try
            {
                if (SelectedEmployee == null)
                {
                    MessageBox.Show("Выберите сотрудника.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Удалить сотрудника?", "Аптека", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                if (!EmployeeService.DeleteEmployee(SelectedEmployee.EmployeeID)) return;

                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}