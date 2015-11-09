using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.ViewModel.Explorer
{
    class DummyExpItemVM : ExplorerItemBaseVM
    {
        public override ExpItemType Type => ExpItemType.Dummy;

        public DummyExpItemVM(MainVM main) : base(main)
        {
        }

        public override string Caption => "";
    }
}
