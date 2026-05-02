using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Drafts
{
    public class DrugEditDraft : ObservableObject
    {
        public int DrugID { get; set; }

        private string _name;
        public string Name { get => _name; set => SetProperty(ref _name, value); }

        private int _categoryID;
        public int CategoryID { get => _categoryID; set => SetProperty(ref _categoryID, value); }

        private string _manufacturer;
        public string Manufacturer { get => _manufacturer; set => SetProperty(ref _manufacturer, value); }

        private int _formID;
        public int FormID { get => _formID; set => SetProperty(ref _formID, value); }

        private string _dosage;
        public string Dosage { get => _dosage; set => SetProperty(ref _dosage, value); }

        private string _unit;
        public string Unit { get => _unit; set => SetProperty(ref _unit, value); }

        private decimal _price;
        public decimal Price { get => _price; set => SetProperty(ref _price, value); }
    }
}
