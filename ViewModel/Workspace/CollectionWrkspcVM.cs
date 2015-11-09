using a7DocumentDbStudio.Dialogs;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel.Explorer;
using a7ExtensionMethods;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace a7DocumentDbStudio.ViewModel.Workspace
{
    public class CollectionWrkspcVM : WrkspcBaseVM
    {
        private CollectionModel _model;
        public string Name => _model.Name;
        public CollectionVM Collection { get;set;}
        public CollectionExpItemVM ExpItem { get; private set; }

        private DocumentModel _selectedDocumentModel;
        public DocumentModel SelectedDocumentModel
        {
            get { return _selectedDocumentModel; }
            set
            {
                _selectedDocumentModel = value;
                if (value != null)
                    SelectedDocumentVM = new DocumentVM(this.Collection, value);
                else
                    SelectedDocumentVM = null;
            }
        }

        private DocumentVM _selectedDocumentVM;
        public DocumentVM SelectedDocumentVM
        {
            get { return _selectedDocumentVM; }
            set
            {
                _selectedDocumentVM = value;
                OnPropertyChanged();
            }
        }


        private bool _isSqlVisible;
        public bool IsSqlVisible
        {
            get { return _isSqlVisible; }
            set
            {
                _isSqlVisible = value;
                OnPropertyChanged();
                //Config.Instance.UpdateSelectedCollectionView(vm => vm.IsSqlVisible = value);
            }
        }

        private bool _isView;
        public bool IsView
        {
            get { return _isView; }
            set
            {
                _isView = value;
                OnPropertyChanged();
            }
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; this.ExpItem.IsBusy = value; OnPropertyChanged(); }
        }
        
        public ICommand EditSqlCommand => new LambdaCommand((o) => this.Collection.IsSqlEditMode = true);
        public ICommand CancelEditSqlCommand => new LambdaCommand((o) => this.Collection.IsSqlEditMode = false);
        public ICommand RefreshCommand => new LambdaCommand((o) => this.Refresh());
        public ICommand AddColumnCommand => new LambdaCommand((o) =>
        {
            if(o is PropertyDefinitionModel)
            {
                var pdm = o as PropertyDefinitionModel;
                this.Collection.AddColumn(pdm);
            }
        });
        public ICommand ShowHideSqlCommand => new LambdaCommand((o) =>
        {
            IsSqlVisible = !IsSqlVisible;
        });
        public ICommand CreateViewCommand => new LambdaCommand((o) =>
        {
            var dlg = new NameSelector("Collection view name");
            if(dlg.ShowDialog() == true && dlg.Value.IsNotEmpty())
            {
                var item = this.ExpItem;
                if (item.IsView)
                    item = item.ViewSourceCollection;
                if(item.Children.Any(c => (c is CollectionExpItemVM) && (c as CollectionExpItemVM).View?.ViewName == dlg.Value))
                {
                    MessageBox.Show("View with this name already exists for this collection!");
                }
                else
                {
                    this.ExpItem.AddAsView(dlg.Value);
                    MessageBox.Show("Current settings saved as seperate view (locally).");
                }
            }
        });
        public ICommand SaveViewCommand => new LambdaCommand((o) =>
        {
            if (this.ExpItem.IsView)
            {
                Config.Instance.SaveCollectionView(_model.AccountEndpoint, _model.DatabaseName, this.ExpItem.View);
                MessageBox.Show("Settings saved to current view.");
            }
        });
        public ICommand DeleteViewCommand => new LambdaCommand((o) =>
        {
            if(this.ExpItem.IsView)
            {
                MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    this.ExpItem.ViewSourceCollection.Children.Remove(this.ExpItem);
                    this.ExpItem.ViewSourceCollection.IsSelected = true;
                    Config.Instance.RemoveCollectionView(_model.AccountEndpoint, _model.DatabaseName, this.ExpItem.View);
                }
            }
        });
        public ICommand AddNewDocumentCommand => new LambdaCommand(async (o) =>
        {
            var emptyJson = @"{
    ""abc"": ""cde""
}";
            var dlg = new DocumentDialog(emptyJson);
            if(dlg.ShowDialog() == true)
            {
                await this.AddDocumentFromString(dlg.Value);
            }
        });


        public CollectionWrkspcVM(CollectionModel model, CollectionExpItemVM expItem): base()
        {
            _model = model;
            ExpItem = expItem;
            this.Collection = new CollectionVM(model, this);
            
        }

        public async Task AddDocumentFromString(string json)
        {
            IsBusy = true;
            try
            {
                JObject.Parse(json);
                var added = await _model.AddDocumentFromString(json);
                this.Collection.Documents.Insert(0, added);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Not possible to save changes:{Environment.NewLine}{e.ToString()}");
            }

            IsBusy = false;
        }

        public async Task Refresh() => await this.Collection.Refresh();
    }
}
