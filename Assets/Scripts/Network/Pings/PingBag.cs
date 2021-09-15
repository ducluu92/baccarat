using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network.Pings
{
    public class PingBag
    {
        #region SingleTone

        private static readonly Lazy<PingBag> _instance = new Lazy<PingBag>(() => new PingBag());

        public static PingBag Instance { get { return _instance.Value; } }

        #endregion

        const int maxCount = 20;
        Queue<int> _queue;

        public PingBag()
        {
            _queue = new Queue<int>();
        }

        public void Enqueue(int time)
        {
            _queue.Enqueue(time);

            if (IsFull())
            {
                _queue.Dequeue();
            }
        }

        public bool IsFull() 
        {
            return _queue.Count > maxCount;
        }

        public double Avg() 
        {
            return _queue.Average();
        }




    }
}
