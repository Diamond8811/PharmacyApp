using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PharmacyApp.Models.Drafts
{
    public class SaleItemDraft : ObservableObject
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

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                    OnPropertyChanged(nameof(LineTotal));
            }
        }

        public decimal LineTotal => Quantity * Price;
    }
}
