using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Drafts;
using PharmacyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class SalesViewModel : BaseViewModel
    {
        private readonly int _employeeId;

        public string CashierName { get; }

        public ObservableCollection<object> Drugs { get; } = new ObservableCollection<object>();
        public ObservableCollection<SaleItemDraft> Items { get; } = new ObservableCollection<SaleItemDraft>();

        private object _selectedDrug;
        public object SelectedDrug { get => _selectedDrug; set => SetProperty(ref _selectedDrug, value); }

        private int _quantityToAdd = 1;
        public int QuantityToAdd { get => _quantityToAdd; set => SetProperty(ref _quantityToAdd, value); }

        private decimal _total;
        public decimal Total { get => _total; set => SetProperty(ref _total, value); }

        public IRelayCommand LoadDrugsCommand { get; }
        public IRelayCommand AddItemCommand { get; }
        public IRelayCommand<SaleItemDraft> RemoveItemCommand { get; }
        public IRelayCommand ClearCommand { get; }
        public IRelayCommand SaveSaleCommand { get; }

        public SalesViewModel(int employeeId, string cashierName)
        {
            _employeeId = employeeId;
            CashierName = cashierName;

            LoadDrugsCommand = new RelayCommand(LoadDrugs);
            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<SaleItemDraft>(RemoveItem);
            ClearCommand = new RelayCommand(Clear);
            SaveSaleCommand = new RelayCommand(SaveSale);

            Items.CollectionChanged += (_, __) => Recalc();
            LoadDrugs();
        }

        private void LoadDrugs()
        {
            try
            {
                Drugs.Clear();
                foreach (var d in DrugService.GetDrugsForSales())
                    Drugs.Add(d);

                SelectedDrug = Drugs.Count > 0 ? Drugs[0] : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки списка лекарств: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem()
        {
            try
            {
                if (SelectedDrug == null)
                {
                    MessageBox.Show("Выберите лекарство.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (QuantityToAdd <= 0)
                {
                    MessageBox.Show("Количество должно быть больше 0.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic d = SelectedDrug;
                int drugId = d.DrugID;
                string name = d.Name;
                decimal price = d.Price;

                var existing = Items.FirstOrDefault(x => x.DrugID == drugId);
                if (existing != null)
                {
                    existing.Quantity += QuantityToAdd;
                }
                else
                {
                    var item = new SaleItemDraft
                    {
                        DrugID = drugId,
                        DrugName = name,
                        Price = price,
                        Quantity = QuantityToAdd
                    };
                    item.PropertyChanged += (_, __) => Recalc();
                    Items.Add(item);
                }

                Recalc();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления позиции: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveItem(SaleItemDraft item)
        {
            if (item == null) return;
            Items.Remove(item);
            Recalc();
        }

        private void Clear()
        {
            Items.Clear();
            Recalc();
        }

        private void SaveSale()
        {
            try
            {
                if (Items.Count == 0)
                {
                    MessageBox.Show("Чек пуст.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var ok = SaleService.CreateSale(_employeeId, Items);
                if (!ok) return;

                MessageBox.Show("Продажа сохранена.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Information);
                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения продажи: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Recalc()
        {
            Total = Items.Sum(i => i.LineTotal);
        }
    }
}