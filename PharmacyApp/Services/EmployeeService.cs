using PharmacyApp.Helpers;
using PharmacyApp.Models;
using PharmacyApp.Models.Dto;
using System;
using System.Linq;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class EmployeeService
    {
        public static SessionUser Authenticate(string login, string password)
        {
            try
            {
                var hash = PasswordHelper.ComputeHash(password);

                using (var db = new PharmacyDBEntities())
                {
                    var user =
                        (from e in db.Employees
                         join r in db.Roles on e.RoleID equals r.RoleID
                         where e.Login == login
                               && e.PasswordHash == hash
                               && e.IsActive == true   
                         select new SessionUser
                         {
                             EmployeeID = e.EmployeeID,
                             FullName = e.FullName,
                             RoleID = e.RoleID,
                             RoleName = r.RoleName
                         }).FirstOrDefault();

                    return user;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка авторизации: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public static object[] GetRoles()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    return db.Roles
                        .Select(r => new { r.RoleID, r.RoleName })
                        .ToArray<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки ролей: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static EmployeeRow[] GetEmployees()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    return
                        (from e in db.Employees
                         join r in db.Roles on e.RoleID equals r.RoleID
                         orderby e.FullName
                         select new EmployeeRow
                         {
                             EmployeeID = e.EmployeeID,
                             FullName = e.FullName,
                             RoleID = e.RoleID,
                             RoleName = r.RoleName,
                             Login = e.Login,
                             IsActive = (e.IsActive ?? false)
                         }).ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки сотрудников: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<EmployeeRow>();
            }
        }

        public static bool SaveEmployee(int? employeeId, string fullName, int roleId, string login, string newPassword, bool isActive)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var isNew = !(employeeId.HasValue && employeeId.Value > 0);

                    var e = isNew
                        ? new Employees()
                        : db.Employees.FirstOrDefault(x => x.EmployeeID == employeeId.Value);

                    if (!isNew && e == null)
                        throw new InvalidOperationException("Сотрудник не найден.");

                    if (isNew)
                    {
                        db.Employees.Add(e);
                        if (string.IsNullOrWhiteSpace(newPassword))
                            throw new InvalidOperationException("Для нового сотрудника необходимо задать пароль.");
                    }

                    var loginExists = db.Employees.Any(x => x.Login == login && x.EmployeeID != e.EmployeeID);
                    if (loginExists) throw new InvalidOperationException("Логин уже используется другим сотрудником.");

                    e.FullName = fullName;
                    e.RoleID = roleId;
                    e.Login = login;
                    e.IsActive = isActive;

                    if (!string.IsNullOrWhiteSpace(newPassword))
                        e.PasswordHash = PasswordHelper.ComputeHash(newPassword);

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения сотрудника: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeleteEmployee(int employeeId)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var e = db.Employees.FirstOrDefault(x => x.EmployeeID == employeeId);
                    if (e == null) return true;

                    db.Employees.Remove(e);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления сотрудника: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}