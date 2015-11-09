using a7DocumentDbStudio.Dialogs;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace a7DocumentDbStudio.ViewModel.Explorer
{
    public class DatabaseExpItemVM : ExplorerItemBaseVM
    {
        private DatabaseModel _model;
        public override string Caption => _model.Name;
        public string Name => _model.Name;
        public override ExpItemType Type => ExpItemType.Database;

        public DatabaseExpItemVM(AccountExpItemVM parentAccount, DatabaseModel model, MainVM main) : base(main)
        {
            _model = model;
            this.Children.Add(new DummyExpItemVM(main));
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Refresh",
                Command = new LambdaCommand(async (o) =>
                {
                    await refresh();
                })
            });
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Create collection",
                Command = new LambdaCommand(async (o) =>
                {
                    var dlg = new NameSelector("New collection name");
                    if (dlg.ShowDialog() == true && dlg.Value.IsNotEmpty())
                    {
                        var created = await _model.CreateCollection(dlg.Value);
                        this.Children.Insert(0, new CollectionExpItemVM(this, created, main));
                    }
                })
            });
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Delete database",
                Command = new LambdaCommand(async (o) =>
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        await this._model.DeleteDabaseFormAccount();
                        parentAccount.Children.Remove(this);
                    }
                })
            });
        }

        protected override async Task onExpanded()
        {
            await refresh();
        }

        private async Task refresh()
        {
            this.IsBusy = true;
            this.Children.Clear();
            await this._model.RefreshCollections();
            foreach (var collection in this._model.Collections)
            {
                this.Children.Add(new CollectionExpItemVM(this, collection, this.Main));
            }
            Config.Instance.ExpandDatabase(_model.Name, _model.Account.Endpoint);
            this.IsBusy = false;
        }

        protected override Task onCollapsed()
        {
            Config.Instance.CollapseDatabase(_model.Name, _model.Account.Endpoint);
            return base.onCollapsed();
        }
    }
}
