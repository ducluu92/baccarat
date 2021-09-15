using Module.Apis.ApiDefinition;
using Module.Apis.Reddots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network.Reddots
{
    public class ReddotBag {

        #region SingleTone

        private static readonly Lazy<ReddotBag> _instance = new Lazy<ReddotBag>(() => new ReddotBag());

        public static ReddotBag Instance { get { return _instance.Value; } }

        #endregion

        private List<ReddotData> _data ;

        private ReddotBag() 
        {
            _data = new List<ReddotData>();
        }

        public void Set(List<ReddotData> data) 
        {
            _data.Clear();
            _data.AddRange(data);
        }

        public List<ReddotData> Pop(ReddotType reddotType) 
        {
            var items = _data.Where(c => c.Types == reddotType).ToList();

            foreach (var item in items)
            {
                _data.Remove(item);
            }

            return items;
        }

        public void Removes(List<ReddotType> reddotTypes)
        {
            var items = _data.Where(c => reddotTypes.Contains(c.Types)).ToList();

            foreach (var item in items)
            {
                _data.Remove(item);
            }
        }

        public bool IsExist(ReddotType reddotType)
        {
            var items = _data.Where(c => c.Types == reddotType).ToList();
            return items.Count > 0;
        }

    }
}
