using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network.Apis
{
    public class ApiResponce
    {
        public int Key { get; set; }

        public ApiAction Action { get; set; }

        public string Data { get; set; }
    }
}
