using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Reports
{
    public class InventoryByCategoryRow
    {
        public string CategoryName { get; set; }
        public int TotalQuantity { get; set; }
        public int UniqueDrugs { get; set; }
    }
}
