using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Reports;
using PharmacyApp.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private DateTime? _from = DateTime.Today.AddDays(-30);
        public DateTime? From
        {
            get => _from;
            set
            {
                if (SetProperty(ref _from, value))
                    OnPeriodChanged();
            }
        }

        private DateTime? _to = DateTime.Today;
        public DateTime? To
        {
            get => _to;
            set
            {
                if (SetProperty(ref _to, value))
                    OnPeriodChanged();
            }
        }

        public ObservableCollection<SalesByPeriodRow> Sales { get; } = new ObservableCollection<SalesByPeriodRow>();
        public ObservableCollection<InventoryByCategoryRow> InventoryByCategory { get; } = new ObservableCollection<InventoryByCategoryRow>();
        public ObservableCollection<MovementRow> Movement { get; } = new ObservableCollection<MovementRow>();

        public IRelayCommand BuildSalesCommand { get; }
        public IRelayCommand BuildInventoryByCategoryCommand { get; }
        public IRelayCommand BuildMovementCommand { get; }
        public IRelayCommand ExportSalesCommand { get; }

        public ReportsViewModel()
        {
            BuildSalesCommand = new RelayCommand(BuildSales);
            BuildInventoryByCategoryCommand = new RelayCommand(BuildInventoryByCategory);
            BuildMovementCommand = new RelayCommand(BuildMovement);
            ExportSalesCommand = new RelayCommand(ExportSales);

            BuildInventoryByCategory();
            SafeBuild(() => BuildSales());
            SafeBuild(() => BuildMovement());
        }

        private async void OnPeriodChanged()
        {
            await Task.Delay(200);
            Application.Current.Dispatcher.Invoke(() =>
            {
                SafeBuild(() => BuildSales());
                SafeBuild(() => BuildMovement());
            });
        }

        private void SafeBuild(Action buildAction)
        {
            try { buildAction(); }
            catch (Exception ex) { MessageBox.Show("Ошибка построения отчёта: " + ex.Message); }
        }

        private void BuildSales()
        {
            if (From == null || To == null) return;
            var data = ReportService.GetSalesByPeriod(From.Value, To.Value);
            Sales.Clear();
            foreach (var r in data) Sales.Add(r);
        }

        private void BuildInventoryByCategory()
        {
            var data = ReportService.GetInventoryByCategory();
            InventoryByCategory.Clear();
            foreach (var r in data) InventoryByCategory.Add(r);
        }

        private void BuildMovement()
        {
            if (From == null || To == null) return;
            var data = ReportService.GetMovement(From.Value, To.Value);
            Movement.Clear();
            foreach (var r in data) Movement.Add(r);
        }

        private void ExportSales()
        {
            if (Sales.Count == 0)
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }
            var dlg = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "SalesReport.xlsx"
            };
            if (dlg.ShowDialog() == true)
            {
                ExportService.ExportToExcel(Sales, "Продажи за период", dlg.FileName);
            }
        }
    }
}