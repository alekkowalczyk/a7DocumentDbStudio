using a7DocumentDbStudio.Dialogs;
using a7DocumentDbStudio.Model;
using a7DocumentDbStudio.Utils;
using a7ExtensionMethods;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace a7DocumentDbStudio.ViewModel
{
    public class DocumentVM : BaseVM
    {
        private DocumentModel _model;
        public CollectionVM SourceCollection { get; private set; }

        private string _stringified;
        public string Stringified
        {
            get { return _stringified; }
            set { _stringified = value; OnPropertyChanged(); }
        }

        private JToken _jToken;
        public JToken JToken
        {
            get { return _jToken; }
            set { _jToken = value;  OnPropertyChanged(); }
        }

        public ICommand EditCommand => new LambdaCommand((o) =>
        {
            IsEditMode = true;
        });
        public ICommand CancelCommand => new LambdaCommand((o) =>
        {
            IsEditMode = false;
        });
        public ICommand SaveCommand => new LambdaCommand(async (o) =>
        {
            IsBusy = true;
            try
            {
                JObject.Parse(this.Stringified);
                await _model.ReplaceFromString(this.Stringified);
            }
            catch(Exception e)
            {
                MessageBox.Show($"Not possible to save changes:{Environment.NewLine}{e.ToString()}");
            }
            
            this.Stringified = _model.ToString();
            this.JToken = _model.Json;
            IsEditMode = false;
            IsBusy = false;
        });
        public ICommand DeleteCommand => new LambdaCommand(async (o) =>
        {
            await this.SourceCollection.DeleteDocument(this._model);
        });
        public ICommand CloneCommand => new LambdaCommand(async (o) =>
        {
            var dlg = new DocumentDialog(this.Stringified);
            if (dlg.ShowDialog() == true)
            {
                await this.SourceCollection.Workspace.AddDocumentFromString(dlg.Value);
            }
        });
        public ICommand CopyValueFromJPropertyCommand => new LambdaCommand((o) =>
        {
            if (o.IsNotEmpty() && o is JProperty)
            {
                var jp = o as JProperty;
                Clipboard.SetText(jp.Value.ToString());
            }
        });
        public ICommand CopyNameFromJPropertyCommand => new LambdaCommand((o) =>
        {
            if (o.IsNotEmpty() && o is JProperty)
            {
                var jp = o as JProperty;
                Clipboard.SetText(jp.Name);
            }
        });
        public ICommand EditValueFromJPropertyCommand => new LambdaCommand(async (o) =>
        {
            if (o.IsNotEmpty() && o is JProperty)
            {
                var jp = o as JProperty;
                var dlg = new ChangeValue(jp.Value.ToObject<object>());
                if(dlg.ShowDialog() == true)
                {
                    await this._model.ChangePropertyValue(jp.Path, dlg.Value);
                    this.Stringified = _model.ToString();
                    this.JToken = _model.Json;
                }
            }
        });

        private bool _isEditMode;
        public bool IsEditMode
        {
            get { return _isEditMode; }
            set { _isEditMode = value; OnPropertyChanged(); if (!value) this.Stringified = _model.ToString(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(); }
        }



        public DocumentVM(CollectionVM sourceCollection, DocumentModel model)
        {
            SourceCollection = sourceCollection;
            _isBusy = false;
            _model = model;
            _isEditMode = false;
            Stringified = _model.ToString();
            JToken = _model.Json;
        }
    }
}
