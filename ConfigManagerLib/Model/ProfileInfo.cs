using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigManagerLib.Model
{
    public class ProfileInfo
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string ProfileDirectory { get; set; }
        public List<ConfigFileInfo> Files { get; set; }
    }
}
