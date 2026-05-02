using CommunityToolkit.Mvvm.Input;
using PharmacyApp.Models.Drafts;
using PharmacyApp.Services;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace PharmacyApp.ViewModels
{
    public class DrugsViewModel : BaseViewModel
    {
        public ObservableCollection<object> Drugs { get; } = new ObservableCollection<object>();
        public ObservableCollection<object> Categories { get; } = new ObservableCollection<object>();
        public ObservableCollection<object> Forms { get; } = new ObservableCollection<object>();
        public ObservableCollection<object> Suppliers { get; } = new ObservableCollection<object>();

        private object _selectedDrug;
        public object SelectedDrug
        {
            get => _selectedDrug;
            set
            {
                if (SetProperty(ref _selectedDrug, value))
                    FillDrugEditFromSelected();
            }
        }

        private object _selectedCategory;
        public object SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    try { CategoryName = value == null ? "" : (string)((dynamic)value).CategoryName; }
                    catch { CategoryName = ""; }
                }
            }
        }

        private object _selectedForm;
        public object SelectedForm
        {
            get => _selectedForm;
            set
            {
                if (SetProperty(ref _selectedForm, value))
                {
                    try { FormName = value == null ? "" : (string)((dynamic)value).FormName; }
                    catch { FormName = ""; }
                }
            }
        }

        private object _selectedSupplier;
        public object SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                if (SetProperty(ref _selectedSupplier, value))
                    FillSupplierEditFromSelected();
            }
        }

        private DrugEditDraft _drugEdit = new DrugEditDraft { Price = 1m };
        public DrugEditDraft DrugEdit { get => _drugEdit; set => SetProperty(ref _drugEdit, value); }

        private SupplierEditDraft _supplierEdit = new SupplierEditDraft();
        public SupplierEditDraft SupplierEdit { get => _supplierEdit; set => SetProperty(ref _supplierEdit, value); }

        private string _categoryName;
        public string CategoryName { get => _categoryName; set => SetProperty(ref _categoryName, value); }

        private string _formName;
        public string FormName { get => _formName; set => SetProperty(ref _formName, value); }

        public IRelayCommand LoadAllCommand { get; }
        public IRelayCommand NewDrugCommand { get; }
        public IRelayCommand SaveDrugCommand { get; }
        public IRelayCommand DeleteDrugCommand { get; }

        public IRelayCommand NewCategoryCommand { get; }
        public IRelayCommand SaveCategoryCommand { get; }
        public IRelayCommand DeleteCategoryCommand { get; }

        public IRelayCommand NewFormCommand { get; }
        public IRelayCommand SaveFormCommand { get; }
        public IRelayCommand DeleteFormCommand { get; }

        public IRelayCommand NewSupplierCommand { get; }
        public IRelayCommand SaveSupplierCommand { get; }
        public IRelayCommand DeleteSupplierCommand { get; }

        public DrugsViewModel()
        {
            LoadAllCommand = new RelayCommand(LoadAll);

            NewDrugCommand = new RelayCommand(NewDrug);
            SaveDrugCommand = new RelayCommand(SaveDrug);
            DeleteDrugCommand = new RelayCommand(DeleteDrug);

            NewCategoryCommand = new RelayCommand(NewCategory);
            SaveCategoryCommand = new RelayCommand(SaveCategory);
            DeleteCategoryCommand = new RelayCommand(DeleteCategory);

            NewFormCommand = new RelayCommand(NewForm);
            SaveFormCommand = new RelayCommand(SaveForm);
            DeleteFormCommand = new RelayCommand(DeleteForm);

            NewSupplierCommand = new RelayCommand(NewSupplier);
            SaveSupplierCommand = new RelayCommand(SaveSupplier);
            DeleteSupplierCommand = new RelayCommand(DeleteSupplier);

            LoadAll();
        }

        private void LoadAll()
        {
            try
            {
                Drugs.Clear();
                Categories.Clear();
                Forms.Clear();
                Suppliers.Clear();

                foreach (var c in DrugService.GetCategories()) Categories.Add(c);
                foreach (var f in DrugService.GetForms()) Forms.Add(f);
                foreach (var s in DrugService.GetSuppliers()) Suppliers.Add(s);
                foreach (var d in DrugService.GetDrugsForLists()) Drugs.Add(d);

                SelectedDrug = Drugs.Count > 0 ? Drugs[0] : null;
                SelectedCategory = Categories.Count > 0 ? Categories[0] : null;
                SelectedForm = Forms.Count > 0 ? Forms[0] : null;
                SelectedSupplier = Suppliers.Count > 0 ? Suppliers[0] : null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FillDrugEditFromSelected()
        {
            try
            {
                if (SelectedDrug == null)
                {
                    DrugEdit = new DrugEditDraft { Price = 1m };
                    return;
                }

                dynamic d = SelectedDrug;
                DrugEdit = new DrugEditDraft
                {
                    DrugID = d.DrugID,
                    Name = d.Name,
                    Manufacturer = d.Manufacturer,
                    Dosage = d.Dosage,
                    Unit = d.Unit,
                    Price = d.Price,
                    CategoryID = FindCategoryIdByName((string)d.CategoryName),
                    FormID = FindFormIdByName((string)d.FormName)
                };
            }
            catch
            {
                DrugEdit = new DrugEditDraft { Price = 1m };
            }
        }

        private void FillSupplierEditFromSelected()
        {
            try
            {
                if (SelectedSupplier == null)
                {
                    SupplierEdit = new SupplierEditDraft();
                    return;
                }

                dynamic s = SelectedSupplier;
                SupplierEdit = new SupplierEditDraft
                {
                    SupplierID = s.SupplierID,
                    SupplierName = s.SupplierName,
                    ContactPerson = s.ContactPerson,
                    Phone = s.Phone,
                    Address = s.Address
                };
            }
            catch
            {
                SupplierEdit = new SupplierEditDraft();
            }
        }

        private int FindCategoryIdByName(string name)
        {
            foreach (var o in Categories)
            {
                dynamic c = o;
                if ((string)c.CategoryName == name) return (int)c.CategoryID;
            }
            return 0;
        }

        private int FindFormIdByName(string name)
        {
            foreach (var o in Forms)
            {
                dynamic f = o;
                if ((string)f.FormName == name) return (int)f.FormID;
            }
            return 0;
        }

        private void NewDrug()
        {
            SelectedDrug = null;
            DrugEdit = new DrugEditDraft { Price = 1m };
        }

        private void SaveDrug()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(DrugEdit.Name) || DrugEdit.CategoryID <= 0 || DrugEdit.FormID <= 0 || DrugEdit.Price <= 0)
                {
                    MessageBox.Show("Заполните обязательные поля: Название, Категория, Форма, Цена > 0.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!DrugService.SaveDrug(
                        DrugEdit.DrugID > 0 ? (int?)DrugEdit.DrugID : null,
                        DrugEdit.Name.Trim(),
                        DrugEdit.CategoryID,
                        DrugEdit.FormID,
                        DrugEdit.Manufacturer,
                        DrugEdit.Dosage,
                        DrugEdit.Unit,
                        DrugEdit.Price))
                    return;

                LoadAll();
                MessageBox.Show("Сохранено.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения лекарства: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteDrug()
        {
            try
            {
                if (SelectedDrug == null)
                {
                    MessageBox.Show("Выберите лекарство.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                dynamic d = SelectedDrug;
                if (MessageBox.Show("Удалить лекарство?", "Аптека", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                if (!DrugService.DeleteDrug((int)d.DrugID)) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления лекарства: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewCategory()
        {
            SelectedCategory = null;
            CategoryName = "";
        }

        private void SaveCategory()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CategoryName))
                {
                    MessageBox.Show("Введите название категории.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? id = null;
                if (SelectedCategory != null) id = (int)((dynamic)SelectedCategory).CategoryID;

                if (!DrugService.SaveCategory(id, CategoryName.Trim())) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения категории: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteCategory()
        {
            try
            {
                if (SelectedCategory == null)
                {
                    MessageBox.Show("Выберите категорию.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var id = (int)((dynamic)SelectedCategory).CategoryID;
                if (MessageBox.Show("Удалить категорию?", "Аптека", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                if (!DrugService.DeleteCategory(id)) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления категории: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewForm()
        {
            SelectedForm = null;
            FormName = "";
        }

        private void SaveForm()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(FormName))
                {
                    MessageBox.Show("Введите название формы.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? id = null;
                if (SelectedForm != null) id = (int)((dynamic)SelectedForm).FormID;

                if (!DrugService.SaveForm(id, FormName.Trim())) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения формы: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteForm()
        {
            try
            {
                if (SelectedForm == null)
                {
                    MessageBox.Show("Выберите форму.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var id = (int)((dynamic)SelectedForm).FormID;
                if (MessageBox.Show("Удалить форму?", "Аптека", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                if (!DrugService.DeleteForm(id)) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления формы: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NewSupplier()
        {
            SelectedSupplier = null;
            SupplierEdit = new SupplierEditDraft();
        }

        private void SaveSupplier()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SupplierEdit.SupplierName))
                {
                    MessageBox.Show("Введите название поставщика.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int? id = SupplierEdit.SupplierID > 0 ? (int?)SupplierEdit.SupplierID : null;

                if (!DrugService.SaveSupplier(id,
                        SupplierEdit.SupplierName.Trim(),
                        SupplierEdit.ContactPerson,
                        SupplierEdit.Phone,
                        SupplierEdit.Address))
                    return;

                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения поставщика: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteSupplier()
        {
            try
            {
                if (SelectedSupplier == null)
                {
                    MessageBox.Show("Выберите поставщика.", "Аптека", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var id = (int)((dynamic)SelectedSupplier).SupplierID;
                if (MessageBox.Show("Удалить поставщика?", "Аптека", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                    return;

                if (!DrugService.DeleteSupplier(id)) return;
                LoadAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления поставщика: " + ex.Message, "Аптека", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}