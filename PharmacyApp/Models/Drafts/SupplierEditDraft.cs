using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PharmacyApp.Models.Drafts
{
    public class SupplierEditDraft : ObservableObject
    {
        public int SupplierID { get; set; }

        private string _supplierName;
        public string SupplierName { get => _supplierName; set => SetProperty(ref _supplierName, value); }

        private string _contactPerson;
        public string ContactPerson { get => _contactPerson; set => SetProperty(ref _contactPerson, value); }

        private string _phone;
        public string Phone { get => _phone; set => SetProperty(ref _phone, value); }

        private string _address;
        public string Address { get => _address; set => SetProperty(ref _address, value); }
    }
}
