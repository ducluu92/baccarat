using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Network.Apis
{
    class ApiBehaviour : MonoBehaviour
    {
        ApiBag bag;

        private void Start()
        {
            bag = ApiBag.Instance;
        }

        private void Update()
        {
            // Api 요청을 계속해서 처리한다.
            bag.Next();
        }

    }
}
