using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Dto
{
    public class EmployeeRow
    {
        public int EmployeeID { get; set; }
        public string FullName { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public string Login { get; set; }
        public bool IsActive { get; set; }
    }
}
