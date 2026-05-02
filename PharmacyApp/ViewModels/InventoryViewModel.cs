using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class InventoryViewModel : BaseViewModel
    {
        public ObservableCollection<object> Batches { get; } = new ObservableCollection<object>();

        private bool _onlyExpiring30;
        public bool OnlyExpiring30
        {
            get => _onlyExpiring30;
            set
            {
                if (SetProperty(ref _onlyExpiring30, value))
                    Load();
            }
        }

        public IRelayCommand LoadCommand { get; }

        public InventoryViewModel()
        {
            LoadCommand = new RelayCommand(Load);
            Load();
        }

        private void Load()
        {
            try
            {
                Batches.Clear();

                var data = OnlyExpiring30
                    ? BatchService.GetExpiringWithin(30)
                    : BatchService.GetBatches();

                foreach (var x in data) Batches.Add(x);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки склада: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}