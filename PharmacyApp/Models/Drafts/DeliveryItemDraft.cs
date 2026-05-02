using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace PharmacyApp.Models.Drafts
{
    public class DeliveryItemDraft : ObservableObject
    {
        public int DrugID { get; set; }

        private string _drugName;
        public string DrugName
        {
            get => _drugName;
            set => SetProperty(ref _drugName, value);
        }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        private decimal _costPrice;
        public decimal CostPrice
        {
            get => _costPrice;
            set
            {
                if (SetProperty(ref _costPrice, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        private string _batchNumber;
        public string BatchNumber
        {
            get => _batchNumber;
            set => SetProperty(ref _batchNumber, value);
        }

        private DateTime _expiryDate = DateTime.Today.AddYears(1);
        public DateTime ExpiryDate
        {
            get => _expiryDate;
            set => SetProperty(ref _expiryDate, value);
        }

        public decimal LineTotal => Quantity * CostPrice;
    }
}
