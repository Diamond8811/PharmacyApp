using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Reports
{
    public class MovementRow
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } 
        public string EmployeeName { get; set; }
        public decimal Sum { get; set; }
    }
}
