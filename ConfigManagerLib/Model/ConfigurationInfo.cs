using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManagerLib.Model
{
    public class ConfigurationInfo
    {
        public string ProfileFolder { get; set; }
        public List<ProfileInfo> Profiles { get; set; }
    }
}
