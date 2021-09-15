using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Network.Reddots
{
    public class ReddotTimer
    {
        public ReddotTimer(float refrash) 
        {
            Refrash = refrash;
        }

        public float Refrash { get; set; }

        private float Time { get; set; }

        public void Update(float deltaTime) 
        {
            Time += deltaTime;
        }

        public void Clear() 
        {
            Time = 0.0f;
        }

        public bool IsRefrash() 
        {
            return Time >= Refrash;
        }

    }
}
