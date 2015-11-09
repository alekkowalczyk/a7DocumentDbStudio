using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace a7DocumentDbStudio.ViewModel
{
    public class MenuItemVM
    {
        public string Caption { get; set; }
        public ICommand Command { get; set; }
    }
}
