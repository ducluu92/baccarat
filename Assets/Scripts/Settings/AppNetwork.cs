using Module.Apis.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Settings
{
    public class AppNetwork
    {
        public int Index { get; set; }

        public string Location { get; set; }

        public HttpType ApiByHttp { get; set; }

        public string ApiByHost { get; set; }

        public int ApiByPort { get; set; }

        public string NetworkByHost { get; set; }
        
        public int NetworkByPort { get; set; }

        public string PublicDns { get; set; }
    }


}
