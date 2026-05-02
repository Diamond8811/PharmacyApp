using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Drafts
{
    public class EmployeeEditDraft : ObservableObject
    {
        public int EmployeeID { get; set; }

        private string _fullName;
        public string FullName { get => _fullName; set => SetProperty(ref _fullName, value); }

        private int _roleID;
        public int RoleID { get => _roleID; set => SetProperty(ref _roleID, value); }

        private string _login;
        public string Login { get => _login; set => SetProperty(ref _login, value); }

        private string _newPassword;
        public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }

        private bool _isActive = true;
        public bool IsActive { get => _isActive; set => SetProperty(ref _isActive, value); }
    }
}
