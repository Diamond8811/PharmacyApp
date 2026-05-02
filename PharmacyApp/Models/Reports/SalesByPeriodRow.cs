using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Reports
{
    public class SalesByPeriodRow
    {
        public int SaleID { get; set; }
        public DateTime SaleDate { get; set; }
        public string EmployeeName { get; set; }
        public int ItemsCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalSum { get; set; }
    }
}
