using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network.Bakaras
{
    public class BkSimpleResult
    {
        public int Index { get; set; }

        public BakaraWinner Winner { get; set; }

        public bool PlayerPair { get; set; }

        public bool BankerPair { get; set; }
    }
}
