using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using PharmacyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace PharmacyApp.ViewModels
{
    public class AnalyticsViewModel : BaseViewModel
    {
        private decimal _todayRevenue;
        public decimal TodayRevenue { get => _todayRevenue; set => SetProperty(ref _todayRevenue, value); }

        private int _todaySalesCount;
        public int TodaySalesCount { get => _todaySalesCount; set => SetProperty(ref _todaySalesCount, value); }

        private int _expiringCount;
        public int ExpiringCount { get => _expiringCount; set => SetProperty(ref _expiringCount, value); }

        private decimal _monthRevenue;
        public decimal MonthRevenue { get => _monthRevenue; set => SetProperty(ref _monthRevenue, value); }

        private SeriesCollection _revenueSeries;
        public SeriesCollection RevenueSeries { get => _revenueSeries; set => SetProperty(ref _revenueSeries, value); }
        private string[] _revenueLabels;
        public string[] RevenueLabels { get => _revenueLabels; set => SetProperty(ref _revenueLabels, value); }
        public Func<double, string> RevenueFormatter { get; set; }

        private SeriesCollection _topDrugsSeries;
        public SeriesCollection TopDrugsSeries { get => _topDrugsSeries; set => SetProperty(ref _topDrugsSeries, value); }
        private string[] _topDrugsLabels;
        public string[] TopDrugsLabels { get => _topDrugsLabels; set => SetProperty(ref _topDrugsLabels, value); }

        private SeriesCollection _categoryColumnSeries;
        public SeriesCollection CategoryColumnSeries { get => _categoryColumnSeries; set => SetProperty(ref _categoryColumnSeries, value); }
        private string[] _categoryColumnLabels;
        public string[] CategoryColumnLabels { get => _categoryColumnLabels; set => SetProperty(ref _categoryColumnLabels, value); }

        private SeriesCollection _staffPerformanceSeries;
        public SeriesCollection StaffPerformanceSeries { get => _staffPerformanceSeries; set => SetProperty(ref _staffPerformanceSeries, value); }
        private string[] _staffLabels;
        public string[] StaffLabels { get => _staffLabels; set => SetProperty(ref _staffLabels, value); }

        public IRelayCommand RefreshCommand { get; }

        public AnalyticsViewModel()
        {
            RefreshCommand = new RelayCommand(LoadData);
            RevenueFormatter = value => value.ToString("N2") + " ₽";
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                IsBusy = true;

                TodayRevenue = ReportService.GetTodayRevenue();
                TodaySalesCount = ReportService.GetTodaySalesCount();
                ExpiringCount = ReportService.GetExpiringCount(30);
                var monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                MonthRevenue = ReportService.GetSalesByPeriod(monthStart, DateTime.Today).Sum(s => s.TotalSum);

                var from = DateTime.Today.AddDays(-6);
                var to = DateTime.Today;
                var salesData = ReportService.GetSalesByPeriod(from, to);
                var daily = salesData.GroupBy(x => x.SaleDate.Date)
                                     .Select(g => new { Date = g.Key, Revenue = g.Sum(s => s.TotalSum) })
                                     .OrderBy(x => x.Date)
                                     .ToList();

                RevenueLabels = daily.Select(x => x.Date.ToString("dd.MM")).ToArray();
                RevenueSeries = new SeriesCollection
                {
                    new LineSeries
                    {
                        Title = "Выручка",
                        Values = new ChartValues<decimal>(daily.Select(x => x.Revenue)),
                        PointGeometrySize = 10,
                        StrokeThickness = 2,
                        Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B6CB0")),
                        Fill = Brushes.Transparent
                    }
                };

                var top = ReportService.GetTopDrugs(from, to, 5);
                TopDrugsLabels = top.Select(x => x.EmployeeName).ToArray();
                TopDrugsSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Продажи",
                        Values = new ChartValues<decimal>(top.Select(x => x.TotalSum)),
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B6CB0")),
                        DataLabels = true
                    }
                };

                var catData = ReportService.GetCategoriesSummary();
                CategoryColumnLabels = catData.Select(c => c.CategoryName).ToArray();
                CategoryColumnSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Остаток, шт.",
                        Values = new ChartValues<decimal>(catData.Select(c => (decimal)c.TotalQuantity)),
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C7A4D")),
                        DataLabels = true
                    }
                };

                var staffData = ReportService.GetEmployeePerformance(from, to);
                StaffLabels = staffData.Select(x => x.FullName).ToArray();
                StaffPerformanceSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Выручка, ₽",
                        Values = new ChartValues<decimal>(staffData.Select(x => x.TotalRevenue)),
                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D97706")),
                        DataLabels = true
                    }
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки аналитики: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}