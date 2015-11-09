using a7DocumentDbStudio.Filter;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel.Explorer;
using a7DocumentDbStudio.ViewModel.Workspace;
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

namespace a7DocumentDbStudio.ViewModel
{
    public class CollectionVM : BaseVM
    {
        private CollectionModel _model;
        public CollectionWrkspcVM Workspace
        {
            get; private set;
        }

        private ObservableCollection<DocumentModel> _documents;
        public ObservableCollection<DocumentModel> Documents {
            get
            {
                return _documents;
            }
            set
            {
                _documents = value;
                OnPropertyChanged();
            }
        }

        private string _sqlText;

        public string SqlText
        {
            get { return _sqlText; }
            set { _sqlText = value; OnPropertyChanged(); }
        }

        private bool _isSqlEditMode;
        public bool IsSqlEditMode
        {
            get { return _isSqlEditMode; }
            set
            {
                _isSqlEditMode = value;
                OnPropertyChanged();
                updateSelectedViewInConfig();
            }
        }

        private int _maxItems;

        public int MaxItems
        {
            get { return _maxItems; }
            set {
                _maxItems = value;
                OnPropertyChanged();
            }
        }


        private CollectionFilterData _filter;
        public CollectionFilterData Filter
        {
            get
            {
                return _filter;
            }
            set
            {
                _filter = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PropertyDefinitionModel> Columns { get; set; }
        public ObservableCollection<PropertyDefinitionModel> AvailableProperties { get; private set; }
        public ICommand AddColumnFromJPropertyCommand => new LambdaCommand((o) =>
        {
            if (o.IsNotEmpty() && o is JProperty)
            {
                var jp = o as JProperty;
                this.AddColumn(new PropertyDefinitionModel { Name = jp.Name, Type = jp.Value.Type.ToPropertyType(), Path = jp.Path });
            }
        });

        public CollectionVM(CollectionModel model, CollectionWrkspcVM wrkspc)
        {
            _model = model;
            Workspace = wrkspc;
            Filter = new CollectionFilterData();
            Columns = new ObservableCollection<PropertyDefinitionModel> { new PropertyDefinitionModel { Name = "id", Path="id", Type = Enums.PropertyType.String } };
            this.AvailableProperties = new ObservableCollection<PropertyDefinitionModel>();
            this.MaxItems = 20;
        }

        public async Task DeleteDocument(DocumentModel documentModel)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                await this._model.DeleteDocument(documentModel);
                this.Documents.Remove(documentModel);
            }
        }

        public void AddColumn(PropertyDefinitionModel columnName)
        {
            this.Columns.Add(columnName);
            updateSelectedViewInConfig();
        }

        public async Task RemoveColumn(string path)
        {
            var cols = this.Columns.Where(c => c.Path == path).ToList();
            foreach (var col in cols)
                this.Columns.Remove(col);
            // Check if column filter exists, if yes, deactivate it.
            if (this.Filter.FieldsFilter.FieldFilters.ContainsKey(path) && this.Filter.FieldsFilter.FieldFilters[path].IsActive)
            {
                this.Filter.FieldsFilter.SetFieldFilter(path, null);
                await this.Refresh();
            }
            updateSelectedViewInConfig();
        }

        private void updateSelectedViewInConfig()
        {
            // Commented out because we don't need to save the current view settings anymore. (it was used when current view settings where saved to restore on next run)
            //Config.Instance.UpdateSelectedCollectionView(vm =>
            //{
            //    vm.Filter = this.Filter;
            //    vm.SqlText = this.SqlText;
            //    vm.Columns = this.Columns.ToList();
            //    vm.CollectionName = this._model.Name;
            //    vm.IsSqlEditMdoe = this.IsSqlEditMode;
            //    if(this.IsSqlEditMode)
            //        vm.IsSqlVisible = true;
            //});
        }

        //if useSqlText = false - filter is used
        public async Task Refresh()
        {
            Workspace.IsBusy = true;
            DocumentListModel docList = null;
            try
            {
                if (this.IsSqlEditMode)
                    docList = await _model.GetDocuments(this.SqlText, null, this.MaxItems);
                else
                    docList = await _model.GetDocuments(this.Filter.JoinedFilter, this.MaxItems);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error on performing query:{Environment.NewLine}{e.Message}");
            }
            if (docList != null)
            {
                var docs = new ObservableCollection<DocumentModel>(docList.Documents);
                this.AvailableProperties.Clear();
                foreach (var doc in docs)
                {
                    foreach (var jp in doc.Json.Children())
                    {
                        addAvailableProperty(jp);
                    }
                }
                this.Documents = docs; //later - the Collection view is attached to PropertyChanged of this and needs the available properties
                this.SqlText = docList.Sql;
                updateSelectedViewInConfig();
            }
            Workspace.IsBusy = false;
        }

        public void Reset()
        {
            this.Filter.AdvancedFilter = null;
            this.Filter.FieldsFilter.Reset();
        }

        private void addAvailableProperty(JToken jt)
        {
            if (jt is JProperty)
            {
                var jp = jt as JProperty;
                if (jp.Value.Type == JTokenType.Array)
                    return; //TODO: array not supported yet
                if (jp.Value.Type == JTokenType.Object)
                {
                    foreach (var childJt in jp.Children())
                    {
                            addAvailableProperty(childJt);
                    }
                }
                else
                {
                    if (!this.AvailableProperties.Any(p => p.Path == jp.Path))
                    {
                        this.AvailableProperties.Add(new PropertyDefinitionModel
                        {
                            Name = jp.Name,
                            Path = jp.Path,
                            Type = jp.Value.Type.ToPropertyType()
                        });
                    }
                }
            }
            else if(jt is JObject)
            {
                foreach(var childJr in jt.Children())
                {
                    addAvailableProperty(childJr);
                }
            }
        }
    }
}
