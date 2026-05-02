using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace PharmacyApp.Services
{
    public static class ExportService
    {
        public static void ExportToExcel<T>(IEnumerable<T> items, string sheetName, string filePath)
        {
            try
            {
                DataTable dt = ToDataTable(items);
                using (var wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt, sheetName);
                    wb.SaveAs(filePath);
                }
                MessageBox.Show($"Данные сохранены в {filePath}", "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message);
            }
        }

        private static DataTable ToDataTable<T>(IEnumerable<T> items)
        {
            var dt = new DataTable();
            var props = typeof(T).GetProperties().Where(p => p.CanRead).ToArray();
            foreach (var prop in props)
                dt.Columns.Add(prop.Name, prop.PropertyType);

            foreach (var item in items)
            {
                var row = dt.NewRow();
                foreach (var prop in props)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                dt.Rows.Add(row);
            }
            return dt;
        }
    }
}