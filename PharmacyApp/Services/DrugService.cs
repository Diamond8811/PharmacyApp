using PharmacyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class DrugService
    {
        public static object[] GetCategories()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                    return db.Categories.OrderBy(c => c.CategoryName)
                        .Select(c => new { c.CategoryID, c.CategoryName })
                        .ToArray<object>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки категорий: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static object[] GetForms()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                    return db.DrugForms.OrderBy(f => f.FormName)
                        .Select(f => new { f.FormID, f.FormName })
                        .ToArray<object>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки форм: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static object[] GetSuppliers()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                    return db.Suppliers.OrderBy(s => s.SupplierName)
                        .Select(s => new { s.SupplierID, s.SupplierName, s.ContactPerson, s.Phone, s.Address })
                        .ToArray<object>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки поставщиков: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static object[] GetDrugsForLists()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var q =
                        from d in db.Drugs
                        join c in db.Categories on d.CategoryID equals c.CategoryID
                        join f in db.DrugForms on d.FormID equals f.FormID
                        orderby d.Name
                        select new
                        {
                            d.DrugID,
                            d.Name,
                            CategoryName = c.CategoryName,
                            FormName = f.FormName,
                            d.Manufacturer,
                            d.Dosage,
                            d.Unit,
                            d.Price,
                            Stock = db.Batches.Where(b => b.DrugID == d.DrugID).Select(b => (int?)b.Quantity).Sum() ?? 0
                        };

                    return q.ToArray<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки лекарств: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static object[] GetDrugsForSales()
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    return db.Drugs.OrderBy(d => d.Name)
                        .Select(d => new { d.DrugID, d.Name, d.Price })
                        .ToArray<object>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списка лекарств: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return Array.Empty<object>();
            }
        }

        public static bool SaveCategory(int? id, string name)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {   
                    Categories c;
                    if (id.HasValue && id.Value > 0)
                    {
                        c = db.Categories.FirstOrDefault(x => x.CategoryID == id.Value);
                        if (c == null) throw new InvalidOperationException("Категория не найдена.");
                    }
                    else
                    {
                        c = new Categories();
                        db.Categories.Add(c);
                    }

                    c.CategoryName = name;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения категории: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeleteCategory(int id)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var c = db.Categories.FirstOrDefault(x => x.CategoryID == id);
                    if (c == null) return true;
                    db.Categories.Remove(c);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления категории: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool SaveForm(int? id, string name)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    DrugForms f;
                    if (id.HasValue && id.Value > 0)
                    {
                        f = db.DrugForms.FirstOrDefault(x => x.FormID == id.Value);
                        if (f == null) throw new InvalidOperationException("Форма не найдена.");
                    }
                    else
                    {
                        f = new DrugForms();
                        db.DrugForms.Add(f);
                    }

                    var exists = db.DrugForms.Any(x => x.FormName == name && x.FormID != f.FormID);
                    if (exists) throw new InvalidOperationException("Такая лекарственная форма уже существует.");

                    f.FormName = name;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения формы: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeleteForm(int id)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var f = db.DrugForms.FirstOrDefault(x => x.FormID == id);
                    if (f == null) return true;
                    db.DrugForms.Remove(f);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления формы: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool SaveSupplier(int? id, string name, string contact, string phone, string address)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    Suppliers s;
                    if (id.HasValue && id.Value > 0)
                    {
                        s = db.Suppliers.FirstOrDefault(x => x.SupplierID == id.Value);
                        if (s == null) throw new InvalidOperationException("Поставщик не найден.");
                    }
                    else
                    {
                        s = new Suppliers();
                        db.Suppliers.Add(s);
                    }

                    s.SupplierName = name;
                    s.ContactPerson = contact;
                    s.Phone = phone;
                    s.Address = address;

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения поставщика: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeleteSupplier(int id)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var s = db.Suppliers.FirstOrDefault(x => x.SupplierID == id);
                    if (s == null) return true;
                    db.Suppliers.Remove(s);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления поставщика: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool SaveDrug(int? drugId, string name, int categoryId, int formId, string manufacturer, string dosage, string unit, decimal price)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    Drugs d;
                    if (drugId.HasValue && drugId.Value > 0)
                    {
                        d = db.Drugs.FirstOrDefault(x => x.DrugID == drugId.Value);
                        if (d == null) throw new InvalidOperationException("Лекарство не найдено.");
                    }
                    else
                    {
                        d = new Drugs();
                        db.Drugs.Add(d);
                    }

                    d.Name = name;
                    d.CategoryID = categoryId;
                    d.FormID = formId;
                    d.Manufacturer = manufacturer;
                    d.Dosage = dosage;
                    d.Unit = unit;
                    d.Price = price;

                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения лекарства: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public static bool DeleteDrug(int id)
        {
            try
            {
                using (var db = new PharmacyDBEntities())
                {
                    var d = db.Drugs.FirstOrDefault(x => x.DrugID == id);
                    if (d == null) return true;
                    db.Drugs.Remove(d);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления лекарства: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}
