using a7DocumentDbStudio.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace a7DocumentDbStudio.Model
{
    public class PropertyDefinitionModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public PropertyType Type { get; set; }

        public override string ToString()
        {
            return this.Path;
        }

        public PropertyDefinitionModel()
        {

        }

        public static PropertyDefinitionModel GetEmpty()
            => new PropertyDefinitionModel
            {
                Name = "",
                Path = "",
                Type = PropertyType.String
            };
    }
}
