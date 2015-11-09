using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Collections;
using a7DocumentDbStudio.Utils;

namespace a7DocumentDbStudio.Controls
{
    public class a7ComboBox : ComboBox
    {


        public bool AddEmptyItem
        {
            get { return (bool)GetValue(AddEmptyItemProperty); }
            set { SetValue(AddEmptyItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddEmptyItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddEmptyItemProperty =
            DependencyProperty.Register("AddEmptyItem", typeof(bool), typeof(a7ComboBox), new PropertyMetadata(true));

        
        public a7ComboBox()
            : base()
        {
            this.ItemContainerStyle = ResourcesManager.Instance.GetStyle("CustomComboItemStyle");
        }

        public static DataTemplate CustomItemTemplate
        {
            get { return ResourcesManager.Instance.GetResource<DataTemplate>("CustomComboItemTemplate"); }
        }


        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
        }

        protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
        {
            //if (newValue is a7DataSourceItemList && AddEmptyItem)
            //{
            //    a7DataSourceItemList iil = newValue as a7DataSourceItemList;
            //    iil.AddEmptyItem();
            //}
            base.OnItemsSourceChanged(oldValue, newValue);
        }

        protected void BaseOnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            base.OnItemsSourceChanged(oldValue, newValue);
        }
    }
}
