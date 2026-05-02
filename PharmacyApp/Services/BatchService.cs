using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class BatchService
    {
        public static object[] GetBatches()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var q = from b in db.Batches
                            join d in db.Drugs on b.DrugID equals d.DrugID
                            orderby d.Name, b.ExpiryDate
                            select new
                            {
                                b.BatchID,
                                d.Name,
                                b.BatchNumber,
                                b.ExpiryDate,
                                b.Quantity
                            };

                    return q.ToArray<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки партий: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static object[] GetExpiringWithin(int days)
        {
            try
            {
                var startDate = DateTime.Today;
                var endDate = startDate.AddDays(days);

                using (var db = new PharmacyDBEntities())
                {
                    var q = from b in db.Batches
                            join d in db.Drugs on b.DrugID equals d.DrugID
                            where b.ExpiryDate >= startDate
                                  && b.ExpiryDate <= endDate
                                  && b.Quantity > 0
                            orderby b.ExpiryDate
                            select new
                            {
                                b.BatchID,
                                d.Name,
                                b.BatchNumber,
                                b.ExpiryDate,
                                b.Quantity
                            };

                    return q.ToArray<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка поиска истекающих партий: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }
    }
}
