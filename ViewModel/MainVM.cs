using a7DocumentDbStudio.Dialogs;
using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel.Workspace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace a7DocumentDbStudio.ViewModel
{
    public class MainVM : BaseVM
    {
        public ExplorerVM Explorer { get; set; }
        public ICommand AddAccountCommand { get; set; }

        private WrkspcBaseVM _currentWorkspace;
        public WrkspcBaseVM CurrentWorkspace
        {
            get { return _currentWorkspace; }
            set
            {
                _currentWorkspace = value;
                OnPropertyChanged();
            }
        }

        public MainVM()
        {
            this.Explorer = new ExplorerVM(this);
            this.CurrentWorkspace = new HomeWrkspcVM();

            foreach (var savedAccount in Config.Instance.DocDbAccounts)
            {
                this.Explorer.ConnectToSavedAccount(savedAccount);
            }

            this.AddAccountCommand = new LambdaCommand((o) =>
            {
                DocDbAccountCredentialsDlg.ShowDialog((credentials) =>
                {
                    this.Explorer.ConnectToAccount(credentials);
                });
            });
        }
    }
}
