using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace a7DocumentDbStudio.ViewModel.Explorer
{
    public class CollectionExpItemVM : ExplorerItemBaseVM
    {
        private CollectionModel _model;
        private CollectionWrkspcVM _wrkspc;
        private DatabaseExpItemVM _parentDbExpItem;
        public bool IsView { get; private set; }
        public CollectionExpItemVM ViewSourceCollection { get; private set; }
        public CollectionViewModel View { get; set; }

        public override string Caption => (IsView)?View.ViewName: _model.Name;
        public override ExpItemType Type => (IsView) ? ExpItemType.CollectionView: ExpItemType.Collection;


        public CollectionExpItemVM(DatabaseExpItemVM parentDatabaseExp, CollectionModel model, MainVM main, CollectionViewModel sourceView = null, CollectionExpItemVM viewSourceCollection = null) : base(main)
        {
            _parentDbExpItem = parentDatabaseExp;
            _model = model;
            if (sourceView != null)
            {
                this.View = sourceView;
                IsView = true;
                ViewSourceCollection = viewSourceCollection;
            }
            else
            {
                IsView = false;
            }
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Refresh",
                Command = new LambdaCommand((o) => _wrkspc?.Refresh())
            });
           

            if (sourceView == null)
            {
                var savedCollection = Config.Instance.GetCollection(model.AccountEndpoint, model.DatabaseName, model.Name);
                if (savedCollection != null)
                {
                    this.IsExpanded = savedCollection.IsExpanded;
                    if (savedCollection.Views != null)
                    {
                        foreach (var view in savedCollection.Views)
                        {
                            this.Children.Add(new CollectionExpItemVM(parentDatabaseExp, model, main, view, this));
                        }
                    }
                }
                this.ContextMenuItems.Add(new MenuItemVM()
                {
                    Caption = "Delete collection",
                    Command = new LambdaCommand(async (o) =>
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            await this._model.DeleteCollectionFromDatabase();
                            parentDatabaseExp.Children.Remove(this);
                        }
                    })
                });
            }
            else
            {
                this.ContextMenuItems.Add(new MenuItemVM()
                {
                    Caption = "Delete view",
                    Command = new LambdaCommand((o) =>
                    {
                        MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            this.ViewSourceCollection.Children.Remove(this);
                            this.ViewSourceCollection.IsSelected = true;
                            Config.Instance.RemoveCollectionView(_model.AccountEndpoint, _model.DatabaseName, this.View);
                        }
                    })
                });
            }
        }

        public void AddAsView(string viewName)
        {
            var newView = this.View.Clone();
            newView.ViewName = viewName;

            if (!this.IsView)
            {
                var newItem = new CollectionExpItemVM(_parentDbExpItem, _model, Main, newView, this);
                this.Children.Add(newItem);
                this.IsExpanded = true;
                newItem.IsSelected = true;
                Config.Instance.AddCollectionView(_model.AccountEndpoint, _model.DatabaseName, newView);
                this._wrkspc.Collection.Reset();
            }
            else
            {
                var newItem = new CollectionExpItemVM(_parentDbExpItem, _model, Main, newView, this.ViewSourceCollection);
                this.ViewSourceCollection.Children.Add(newItem);
                this.ViewSourceCollection.IsExpanded = true;
                newItem.IsSelected = true;
                Config.Instance.AddCollectionView(_model.AccountEndpoint, _model.DatabaseName, newView);
            }
            Config.Instance.ResetSelectedCollection();
        }

        protected override async Task onSelected()
        {
            _wrkspc = new CollectionWrkspcVM(this._model, this);
            _wrkspc.IsView = this.IsView;
            if (View != null && this.IsView)
            {
                _wrkspc.Collection.Filter = View.Filter;
                _wrkspc.Collection.Columns = new System.Collections.ObjectModel.ObservableCollection<PropertyDefinitionModel>(View.Columns);
                _wrkspc.Collection.SqlText = View.SqlText;
                _wrkspc.Collection.IsSqlEditMode = View.IsSqlEditMdoe;
                _wrkspc.IsSqlVisible = View.IsSqlVisible;
            }
            this.Main.CurrentWorkspace = _wrkspc;
            await _wrkspc.Refresh();
            if (this.IsView)
                Config.Instance.SelectedCollectionViewModel = this.View;
            Config.Instance.SelectCollection(_model.Name, _model.Database.Name, _model.Database.Account.Endpoint, (this.IsView) ? this.View.ViewName : null);
            await base.onSelected();
        }

        protected override Task onExpanded()
        {
            Config.Instance.ExpandCollection(this._model.DatabaseName, this._model.AccountEndpoint, this._model.Name);
            return base.onExpanded();
        }

        protected override Task onCollapsed()
        {
            Config.Instance.CollapseCollection(this._model.DatabaseName, this._model.AccountEndpoint, this._model.Name);
            return base.onCollapsed();
        }
    }
}
