using PharmacyApp.Models;
using PharmacyApp.Models.Drafts;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class SaleService
    {
        public static bool CreateSale(int employeeId, IEnumerable<SaleItemDraft> items)
        {
            try
            {
                var list = items?.Where(i => i != null && i.Quantity > 0).ToList() ?? new List<SaleItemDraft>();
                if (list.Count == 0)
                    throw new InvalidOperationException("Добавьте хотя бы одну позицию в чек.");

                using (var db = new PharmacyDBEntities())
                using (var tx = db.Database.BeginTransaction())
                {
                    foreach (var it in list)
                    {
                        var available = db.Batches
                            .Where(b => b.DrugID == it.DrugID)
                            .Select(b => (int?)b.Quantity)
                            .Sum() ?? 0;

                        if (available < it.Quantity)
                            throw new InvalidOperationException(
                                $"Недостаточно товара на складе: \"{it.DrugName}\". Доступно: {available}, требуется: {it.Quantity}.");
                    }

                    var sale = new Sales
                    {
                        EmployeeID = employeeId,
                        SaleDate = DateTime.Now
                    };

                    db.Sales.Add(sale);
                    db.SaveChanges();

                    foreach (var it in list)
                    {
                        db.Sale_Items.Add(new Sale_Items
                        {
                            SaleID = sale.SaleID,
                            DrugID = it.DrugID,
                            Quantity = it.Quantity,
                            Price = it.Price
                        });
                    }

                    db.SaveChanges(); 
                    tx.Commit();

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения продажи: " + ex.Message, "Аптека",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}