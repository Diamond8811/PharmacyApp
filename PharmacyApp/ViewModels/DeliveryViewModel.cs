using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Drafts;
using PharmacyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class DeliveryViewModel : BaseViewModel
    {
        private readonly int _employeeId;

        public ObservableCollection<object> Suppliers { get; } = new ObservableCollection<object>();
        public ObservableCollection<object> Drugs { get; } = new ObservableCollection<object>();
        public ObservableCollection<DeliveryItemDraft> Items { get; } = new ObservableCollection<DeliveryItemDraft>();

        private object _selectedSupplier;
        public object SelectedSupplier { get => _selectedSupplier; set => SetProperty(ref _selectedSupplier, value); }

        private object _selectedDrug;
        public object SelectedDrug { get => _selectedDrug; set => SetProperty(ref _selectedDrug, value); }

        private int _quantityToAdd = 1;
        public int QuantityToAdd { get => _quantityToAdd; set => SetProperty(ref _quantityToAdd, value); }

        private decimal _costPriceToAdd = 1m;
        public decimal CostPriceToAdd { get => _costPriceToAdd; set => SetProperty(ref _costPriceToAdd, value); }

        private string _batchNumberToAdd;
        public string BatchNumberToAdd { get => _batchNumberToAdd; set => SetProperty(ref _batchNumberToAdd, value); }

        private DateTime? _expiryToAdd = DateTime.Today.AddYears(1);
        public DateTime? ExpiryToAdd { get => _expiryToAdd; set => SetProperty(ref _expiryToAdd, value); }

        private DateTime? _deliveryDate = DateTime.Today;
        public DateTime? DeliveryDate { get => _deliveryDate; set => SetProperty(ref _deliveryDate, value); }

        private decimal _total;
        public decimal Total { get => _total; set => SetProperty(ref _total, value); }

        public IRelayCommand LoadListsCommand { get; }
        public IRelayCommand AddItemCommand { get; }
        public IRelayCommand<DeliveryItemDraft> RemoveItemCommand { get; }
        public IRelayCommand ClearCommand { get; }
        public IRelayCommand SaveDeliveryCommand { get; }

        public DeliveryViewModel(int employeeId)
        {
            _employeeId = employeeId;

            LoadListsCommand = new RelayCommand(LoadLists);
            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<DeliveryItemDraft>(RemoveItem);
            ClearCommand = new RelayCommand(Clear);
            SaveDeliveryCommand = new RelayCommand(SaveDelivery);

            Items.CollectionChanged += (_, __) => Recalc();
            LoadLists();
        }

        private void LoadLists()
        {
            try
            {
                Suppliers.Clear();
                Drugs.Clear();

                foreach (var s in DrugService.GetSuppliers()) Suppliers.Add(s);
                foreach (var d in DrugService.GetDrugsForSales()) Drugs.Add(d);

                SelectedSupplier = Suppliers.Count > 0 ? Suppliers[0] : null;
                SelectedDrug = Drugs.Count > 0 ? Drugs[0] : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddItem()
        {
            try
            {
                if (SelectedDrug == null)
                {
                    MessageBox.Show("Выберите товар.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (QuantityToAdd <= 0 || CostPriceToAdd <= 0)
                {
                    MessageBox.Show("Количество и себестоимость должны быть больше 0.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrWhiteSpace(BatchNumberToAdd))
                {
                    MessageBox.Show("Укажите номер партии.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (ExpiryToAdd == null)
                {
                    MessageBox.Show("Укажите срок годности.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic d = SelectedDrug;

                var item = new DeliveryItemDraft
                {
                    DrugID = d.DrugID,
                    DrugName = d.Name,
                    Quantity = QuantityToAdd,
                    CostPrice = CostPriceToAdd,
                    BatchNumber = BatchNumberToAdd.Trim(),
                    ExpiryDate = ExpiryToAdd.Value.Date
                };

                item.PropertyChanged += (_, __) => Recalc();
                Items.Add(item);
                Recalc();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления позиции: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RemoveItem(DeliveryItemDraft item)
        {
            if (item == null) return;
            Items.Remove(item);
            Recalc();
        }

        private void Clear()
        {
            Items.Clear();
            Total = 0;
        }

        private void SaveDelivery()
        {
            try
            {
                if (SelectedSupplier == null)
                {
                    MessageBox.Show("Выберите поставщика.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DeliveryDate == null)
                {
                    MessageBox.Show("Укажите дату поставки.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic s = SelectedSupplier;
                var supplierId = (int)s.SupplierID;

                var ok = DeliveryService.CreateDelivery(supplierId, _employeeId, DeliveryDate.Value, Items);
                if (!ok) return;

                MessageBox.Show("Поставка сохранена.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Information);
                Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения поставки: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Recalc()
        {
            Total = Items.Sum(i => i.LineTotal);
        }
    }
}