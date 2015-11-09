using a7DocumentDbStudio.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Utils
{
    public class Config
    {
        private static ConfigModel _model;
        public static ConfigModel Instance
        {
            get
            {
                if (_model == null)
                    _model = ConfigModel.Get("config.json");
                return _model;
            }
        }

        public static void Reload()
        {
            _model = null;
        }
    }
}
