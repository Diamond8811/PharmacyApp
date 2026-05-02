using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Reports
{
    public class EmployeePerformanceRow
    {
        public string FullName { get; set; }
        public int SalesCount { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
