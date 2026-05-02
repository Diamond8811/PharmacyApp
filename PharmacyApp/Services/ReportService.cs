using PharmacyApp.Models;
using PharmacyApp.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class ReportService
    {
        public static List<SalesByPeriodRow> GetSalesByPeriod(DateTime from, DateTime to)
        {
            try
            {
                var fromDt = from.Date;
                var toDt = to.Date.AddDays(1).AddSeconds(-1);

                using (var db = new PharmacyDBEntities())
                {
                    var query = from s in db.Sales
                                join e in db.Employees on s.EmployeeID equals e.EmployeeID
                                where s.SaleDate >= fromDt && s.SaleDate <= toDt
                                orderby s.SaleDate
                                select new SalesByPeriodRow
                                {
                                    SaleID = s.SaleID,
                                    SaleDate = s.SaleDate,
                                    EmployeeName = e.FullName,
                                    ItemsCount = db.Sale_Items.Count(i => i.SaleID == s.SaleID),
                                    TotalQuantity = db.Sale_Items
                                        .Where(i => i.SaleID == s.SaleID)
                                        .Sum(i => (int?)i.Quantity) ?? 0,
                                    TotalSum = db.Sale_Items
                                        .Where(i => i.SaleID == s.SaleID)
                                        .Sum(i => (decimal?)i.Price * i.Quantity) ?? 0m
                                };
                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отчёта по продажам: " + ex.Message,
                                "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<SalesByPeriodRow>();
            }
        }

        public static List<InventoryByCategoryRow> GetInventoryByCategory()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var q = from d in db.Drugs
                            join c in db.Categories on d.CategoryID equals c.CategoryID
                            let qty = db.Batches
                                        .Where(b => b.DrugID == d.DrugID)
                                        .Select(b => (int?)b.Quantity)
                                        .Sum() ?? 0
                            group new { d, qty } by c.CategoryName into g
                            orderby g.Key
                            select new InventoryByCategoryRow
                            {
                                CategoryName = g.Key,
                                TotalQuantity = g.Sum(x => x.qty),
                                UniqueDrugs = g.Count()
                            };
                    return q.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отчёта по остаткам: " + ex.Message,
                                "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<InventoryByCategoryRow>();
            }
        }

        public static List<MovementRow> GetMovement(DateTime from, DateTime to)
        {
            try
            {
                var fromDt = from.Date;
                var toDt = to.Date.AddDays(1).AddSeconds(-1);

                using (var db = new PharmacyDBEntities())
                {
                    var deliveries = from d in db.Deliveries
                                     join e in db.Employees on d.EmployeeID equals e.EmployeeID
                                     where d.DeliveryDate >= fromDt && d.DeliveryDate <= toDt
                                     select new MovementRow
                                     {
                                         Date = d.DeliveryDate,
                                         Type = "Поставка",
                                         EmployeeName = e.FullName,
                                         Sum = db.Delivery_Items
                                             .Where(i => i.DeliveryID == d.DeliveryID)
                                             .Sum(i => (decimal?)i.CostPrice * i.Quantity) ?? 0m
                                     };

                    var sales = from s in db.Sales
                                join e in db.Employees on s.EmployeeID equals e.EmployeeID
                                where s.SaleDate >= fromDt && s.SaleDate <= toDt
                                select new MovementRow
                                {
                                    Date = s.SaleDate,
                                    Type = "Продажа",
                                    EmployeeName = e.FullName,
                                    Sum = db.Sale_Items
                                        .Where(i => i.SaleID == s.SaleID)
                                        .Sum(i => (decimal?)i.Price * i.Quantity) ?? 0m
                                };

                    return deliveries.Concat(sales).OrderBy(x => x.Date).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка отчёта движения товара: " + ex.Message,
                                "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<MovementRow>();
            }
        }

        public static List<SalesByPeriodRow> GetTopDrugs(DateTime from, DateTime to, int topN = 5)
        {
            try
            {
                var fromDt = from.Date;
                var toDt = to.Date.AddDays(1).AddSeconds(-1);
                using (var db = new PharmacyDBEntities())
                {
                    var query = db.Sale_Items
                        .Where(i => i.Sales.SaleDate >= fromDt && i.Sales.SaleDate <= toDt)
                        .GroupBy(i => new { i.DrugID, DrugName = i.Drugs.Name })
                        .Select(g => new
                        {
                            DrugName = g.Key.DrugName,
                            TotalQuantity = g.Sum(i => (int?)i.Quantity) ?? 0,
                            TotalSum = g.Sum(i => (decimal?)i.Quantity * (decimal?)i.Price) ?? 0m
                        })
                        .OrderByDescending(g => g.TotalSum)
                        .Take(topN)
                        .ToList();

                    return query.Select(x => new SalesByPeriodRow
                    {
                        SaleID = 0,
                        SaleDate = DateTime.MinValue,
                        EmployeeName = x.DrugName,       
                        ItemsCount = 0,
                        TotalQuantity = x.TotalQuantity,
                        TotalSum = x.TotalSum
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения топ-препаратов: " + ex.Message);
                return new List<SalesByPeriodRow>();
            }
        }

        public static List<EmployeePerformanceRow> GetEmployeePerformance(DateTime from, DateTime to)
        {
            try
            {
                var fromDt = from.Date;
                var toDt = to.Date.AddDays(1).AddSeconds(-1);
                using (var db = new PharmacyDBEntities())
                {
                    var query = from s in db.Sales
                                join e in db.Employees on s.EmployeeID equals e.EmployeeID
                                where s.SaleDate >= fromDt && s.SaleDate <= toDt
                                group new { s, e } by new { e.EmployeeID, e.FullName } into grp
                                select new EmployeePerformanceRow
                                {
                                    FullName = grp.Key.FullName,
                                    SalesCount = grp.Count(),
                                    TotalRevenue = grp.Sum(x =>
                                        (decimal?)(db.Sale_Items
                                            .Where(i => i.SaleID == x.s.SaleID)
                                            .Sum(i => i.Quantity * i.Price))
                                        ) ?? 0m
                                };
                    return query.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка получения данных по сотрудникам: " + ex.Message);
                return new List<EmployeePerformanceRow>();
            }
        }

        public static List<InventoryByCategoryRow> GetCategoriesSummary() => GetInventoryByCategory();

        public static decimal GetTodayRevenue()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                using (var db = new PharmacyDBEntities())
                    return db.Sale_Items
                        .Where(i => i.Sales.SaleDate >= today && i.Sales.SaleDate < tomorrow)
                        .Sum(i => (decimal?)(i.Quantity * i.Price)) ?? 0m;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка расчета дневной выручки: " + ex.Message);
                return 0;
            }
        }

        public static int GetTodaySalesCount()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                using (var db = new PharmacyDBEntities())
                    return db.Sales.Count(s => s.SaleDate >= today && s.SaleDate < tomorrow);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подсчёта чеков: " + ex.Message);
                return 0;
            }
        }

        public static int GetExpiringCount(int days = 30)
        {
            try
            {
                var threshold = DateTime.Today.AddDays(days);
                using (var db = new PharmacyDBEntities())
                    return db.Batches.Count(b =>
                        b.ExpiryDate <= threshold && b.ExpiryDate >= DateTime.Today && b.Quantity > 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подсчёта истекающих партий: " + ex.Message);
                return 0;
            }
        }
    }
}