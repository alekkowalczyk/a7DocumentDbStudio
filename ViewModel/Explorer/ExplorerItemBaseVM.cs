using a7DocumentDbStudio.Utils;
using a7DocumentDbStudio.ViewModel.Explorer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace a7DocumentDbStudio.ViewModel.Explorer
{
    public abstract class ExplorerItemBaseVM : BaseVM
    {
        public ObservableCollection<ExplorerItemBaseVM> Children { get; private set; }
        public ObservableCollection<MenuItemVM> ContextMenuItems { get; private set; }
        public abstract string Caption { get; }
        public abstract ExpItemType Type { get; }
        protected MainVM Main { get; private set; }
        private bool _isExpanded;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set {
                if (value)
                {
                    onExpanded();
                }
                else
                {
                    onCollapsed();
                }
                _isExpanded = value;
                OnPropertyChanged();
            }
        }
        public async Task Expand()
        {
            await onExpanded();
            _isExpanded = true;
            OnPropertyChanged<bool>(() => this.IsExpanded);
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value)
                {
                    onSelected();
                }
                _isSelected = value;
                OnPropertyChanged();
            }
        }
        public async Task Select()
        {
            await onSelected();
            _isSelected = true;
            OnPropertyChanged<bool>(() => this.IsSelected);
        }

        private bool _isBusy;

        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged(); }
        }


        public ExplorerItemBaseVM(MainVM main)
        {
            this.Main = main;
            this.Children = new ObservableCollection<ExplorerItemBaseVM>();
            this.ContextMenuItems = new ObservableCollection<MenuItemVM>();
        }

        protected virtual Task onExpanded()
        {
            return Task.Run(()=> { });
        }

        protected virtual Task onCollapsed()
        {
            return Task.Run(() => { });
        }

        protected virtual Task onSelected()
        {
            return Task.Run(() => { });
        }
    }
}
