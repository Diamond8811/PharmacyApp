using PharmacyApp.Models;
using PharmacyApp.Models.Drafts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class DeliveryService
    {
        public static bool CreateDelivery(int supplierId, int employeeId, DateTime deliveryDate, IEnumerable<DeliveryItemDraft> items)
        {
            try
            {
                var list = items?.Where(i => i.Quantity > 0).ToList() ?? new List<DeliveryItemDraft>();
                if (list.Count == 0) throw new InvalidOperationException("Добавьте хотя бы одну позицию в поставку.");

                using (var db = new PharmacyDBEntities())
                using (var tx = db.Database.BeginTransaction())
                {
                    var delivery = new Deliveries
                    {
                        SupplierID = supplierId,
                        EmployeeID = employeeId,
                        DeliveryDate = deliveryDate
                    };
                    db.Deliveries.Add(delivery);
                    db.SaveChanges();

                    foreach (var it in list)
                    {
                        if (string.IsNullOrWhiteSpace(it.BatchNumber))
                            throw new InvalidOperationException($"Для товара \"{it.DrugName}\" не указан номер партии.");

                        if (it.ExpiryDate.Date < DateTime.Today)
                            throw new InvalidOperationException($"Для товара \"{it.DrugName}\" срок годности не может быть в прошлом.");

                        db.Delivery_Items.Add(new Delivery_Items
                        {
                            DeliveryID = delivery.DeliveryID,
                            DrugID = it.DrugID,
                            Quantity = it.Quantity,
                            CostPrice = it.CostPrice,
                            BatchNumber = it.BatchNumber.Trim(),
                            ExpiryDate = it.ExpiryDate.Date
                        });
                    }

                    db.SaveChanges(); 
                    tx.Commit();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения поставки: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}