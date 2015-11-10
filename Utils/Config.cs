using a7DocumentDbStudio.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
                {
                    var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    appDataFolder = Path.Combine(appDataFolder, "a7DocumentDbStudio");
                    if (!Directory.Exists(appDataFolder))
                        Directory.CreateDirectory(appDataFolder);
                    var appDataConfigFile = Path.Combine(appDataFolder, "config.json");
                    _model = ConfigModel.Get(appDataConfigFile);
                }
                return _model;
            }
        }

        public static void Reload()
        {
            _model = null;
        }
    }
}
