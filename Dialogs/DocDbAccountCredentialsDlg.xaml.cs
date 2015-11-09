using a7DocumentDbStudio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace a7DocumentDbStudio.Dialogs
{
    /// <summary>
    /// Interaction logic for ConnectToDocDbAccount.xaml
    /// </summary>
    public partial class DocDbAccountCredentialsDlg : Window
    {
        private Action<AccountCredentialsModel> _action;
        private Func<AccountCredentialsModel, Task> _actionAsync;
        private AccountCredentialsModel _existingCredentials;

        private DocDbAccountCredentialsDlg(Action<AccountCredentialsModel> action)
        {
            _action = action;
            InitializeComponent();
        }

        private DocDbAccountCredentialsDlg(AccountCredentialsModel credentials, Func<AccountCredentialsModel, Task> actionAsync)
        {
            this._actionAsync = actionAsync;
            this._existingCredentials = credentials;
            InitializeComponent();
            this.tbEndpoint.Text = credentials.Endpoint;
            this.tbKey.Text = credentials.Key;
        }

        private async void bOk_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.bOk.IsEnabled = false;
                if (_existingCredentials != null)
                {
                    this._existingCredentials.Endpoint = this.tbEndpoint.Text;
                    this._existingCredentials.Key = this.tbKey.Text;
                    if (this._action != null)
                        this._action(this._existingCredentials);
                    else if (this._actionAsync != null)
                        await this._actionAsync(this._existingCredentials);
                }
                else
                {
                    var credModel = new AccountCredentialsModel(this.tbEndpoint.Text, this.tbKey.Text);
                    if (this._action != null)
                        this._action(credModel);
                    else if (this._actionAsync != null)
                        await this._actionAsync(credModel);
                }
                this.DialogResult = true;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                this.bOk.IsEnabled = true;
            }
        }

        public static void ShowDialog(Action<AccountCredentialsModel> action)
        {
            new DocDbAccountCredentialsDlg(action).ShowDialog();
        }

        public static void ShowDialog(AccountCredentialsModel existingCreds, Func<AccountCredentialsModel, Task> action)
        {
            new DocDbAccountCredentialsDlg(existingCreds, action).ShowDialog();
        }
    }
}
