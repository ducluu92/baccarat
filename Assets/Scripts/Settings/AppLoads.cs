using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Settings
{
    public class AppLoads
    {
        public string Version { get; set; }

        public EnvironmentType Environment { get; set; }

        public string NetworkTarget { get; set; }

        public bool IsOpenAll { get; set; }

        public List<AppNetwork> Networks { get; set; }
    }
}
