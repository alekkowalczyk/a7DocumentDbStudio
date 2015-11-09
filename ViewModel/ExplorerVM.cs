using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Model.Config;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel.Explorer;
using a7ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.ViewModel
{
    public class ExplorerVM
    {
        public ObservableCollection<ExplorerItemBaseVM> RootItems { get; set; }

        private MainVM _main;

        public ExplorerVM(MainVM main)
        {
            this._main = main;
            this.RootItems = new ObservableCollection<ExplorerItemBaseVM>();
        }

        public void ConnectToAccount(AccountCredentialsModel credentials)
        {
            var model = new AccountModel(credentials);
            Config.Instance.AddAccount(credentials);
            var accountVM = new AccountExpItemVM(model, _main);
            this.RootItems.Add(accountVM);
        }

        public void ConnectToSavedAccount(SavedAccountModel savedAccount)
        {
            var model = new AccountModel(savedAccount.Credentials);
            var accountVM = new AccountExpItemVM(model, _main);
            this.RootItems.Add(accountVM);
            if(savedAccount.IsExpanded)
            {
                accountVM.Expand().ContinueWith<Task>(async (t) =>
                {
                    if (savedAccount.Databases != null)
                    {
                        foreach (var db in savedAccount.Databases)
                        {
                            var found = accountVM.Children.FirstOrDefault(item => item is DatabaseExpItemVM && (item as DatabaseExpItemVM).Name == db.Name);
                            if (found != null)
                            {
                                if (db.IsExpanded)
                                {
                                    await found.Expand();
                                    foreach(var savedColl in db.Collections)
                                    {
                                        var foundColl = found.Children.FirstOrDefault(item => item is CollectionExpItemVM && (item as CollectionExpItemVM).Caption == savedColl.Name);
                                        if(foundColl !=null)
                                        {
                                            if(savedColl.IsExpanded)
                                                await foundColl.Expand();
                                        }
                                        else
                                        {
                                            Config.Instance.RemoveCollection(db.Name, savedAccount.Credentials.Endpoint, savedColl.Name);
                                        }
                                    }
                                    //set selected
                                    
                                    if (Config.Instance.SelectedAccount == savedAccount.Credentials.Endpoint && Config.Instance.SelectedDatabase == db.Name && Config.Instance.SelectedCollection.IsNotEmpty())
                                    {
                                        var selectedCollection = found.Children.FirstOrDefault(item => item is CollectionExpItemVM && (item as CollectionExpItemVM).Caption == Config.Instance.SelectedCollection) as CollectionExpItemVM;
                                        if (selectedCollection != null)
                                        {
                                            if (Config.Instance.SelectedView.IsEmpty())
                                            {
                                                selectedCollection.View = Config.Instance.SelectedCollectionViewModel;
                                                //nope, we don't set selected, views are to save nice views, opening the app doesn't restore the last views.
                                                //await selectedCollection.Select();
                                            }
                                            else
                                            {
                                                var selectedView = selectedCollection.Children.FirstOrDefault(item => item is CollectionExpItemVM && (item as CollectionExpItemVM).Caption == Config.Instance.SelectedView) as CollectionExpItemVM;
                                                selectedView.View = Config.Instance.SelectedCollectionViewModel;
                                                //nope, we don't set selected, views are to save nice views, opening the app doesn't restore the last views.
                                                //await selectedView.Select();
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Config.Instance.RemoveDatabase(db.Name, savedAccount.Credentials.Endpoint);
                            }
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
        }
    }
}
