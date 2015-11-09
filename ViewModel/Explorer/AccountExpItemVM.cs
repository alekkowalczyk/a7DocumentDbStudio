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
    public class AccountExpItemVM : ExplorerItemBaseVM
    {
        private AccountModel _model;
        public override string Caption => _model.Endpoint;
        public override ExpItemType Type => (IsBroken) ? ExpItemType.BrokenAccount : ExpItemType.Account;
        public bool IsBroken
        {
            get; private set;
        }

        public AccountExpItemVM(AccountModel model, MainVM main) : base(main)
        {
            IsBroken = false;
            _model = model;
            this.Children.Add(new DummyExpItemVM(main));
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Refresh",
                Command = new LambdaCommand(async (o) =>
                {
                    await this.LoadDatabases();
                })
            });
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Create database",
                Command = new LambdaCommand(async (o) =>
                {
                    if(IsBroken)
                    {
                        MessageBox.Show("Can't create database on a broken connect, check error message, try to reenter credentials.");
                        return;
                    }
                    var dlg = new NameSelector("New database name");
                    if (dlg.ShowDialog() == true && dlg.Value.IsNotEmpty())
                    {
                        var created = await  _model.CreateDatabase(dlg.Value);
                        this.Children.Insert(0,new DatabaseExpItemVM(this, created, this.Main));
                    }
                })
            });
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Change access data",
                Command = new LambdaCommand((o) =>
                {
                    var oldEndpoint = _model.Endpoint;
                    DocDbAccountCredentialsDlg.ShowDialog(_model.Credentials, async (creds) =>
                    {
                        await performWithIsBrokenTest(async () =>
                        {
                            this.Children.Clear();
                            await _model.RefreshCredentials();
                            foreach (var db in this._model.Databases)
                            {
                                this.Children.Add(new DatabaseExpItemVM(this, db, this.Main));
                            }
                            Config.Instance.ChangeAccountCredentials(oldEndpoint, _model.Credentials);
                        });
                    });
                })
            });
            this.ContextMenuItems.Add(new MenuItemVM()
            {
                Caption = "Remove",
                Command = new LambdaCommand((o) =>
                {
                    MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Removal Confirmation", System.Windows.MessageBoxButton.YesNo);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        main.Explorer.RootItems.Remove(this);
                        Config.Instance.RemoveAccount(_model.Endpoint);
                    }
                })
            });
        }

        public async Task LoadDatabases()
        {
            await performWithIsBrokenTest(async () =>
            {
                await this._model.RefreshDatabases();
                foreach (var db in this._model.Databases)
                {
                    this.Children.Add(new DatabaseExpItemVM(this, db, this.Main));
                }
            });
        }

        private async Task performWithIsBrokenTest(Func<Task> action)
        {
            this.IsBusy = true;
            this.Children.Clear();
            try
            {
                await action();
                this.IsBroken = false;
                OnPropertyChanged(() => this.Type);
                var showErrorMenuItem = this.ContextMenuItems.FirstOrDefault(mi => mi.Caption == "Show error");
                if (showErrorMenuItem != null)
                    this.ContextMenuItems.Remove(showErrorMenuItem);
            }
            catch (Exception e)
            {
                this.IsBroken = true;
                OnPropertyChanged(() => this.Type);
                var showErrorMenuItem = this.ContextMenuItems.FirstOrDefault(mi => mi.Caption == "Show error");
                if (showErrorMenuItem != null)
                    this.ContextMenuItems.Remove(showErrorMenuItem);
                this.ContextMenuItems.Insert(0, new MenuItemVM()
                {
                    Caption = "Show error",
                    Command = new LambdaCommand((o) =>
                    {
                        MessageBox.Show(e.Message);
                    })
                });
            }
            finally
            {
                this.IsBusy = false;
            }
        }

        protected override async Task onExpanded()
        {
            await LoadDatabases();
            Config.Instance.ExpandAccount(this._model.Endpoint);
        }

        protected override async Task onCollapsed()
        {
            Config.Instance.CollapseAccount(this._model.Endpoint);
            await base.onCollapsed();
        }
    }
}
