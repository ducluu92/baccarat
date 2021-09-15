using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Network.Seq
{

    public class TokenManager
    {
        const string key = "last-token";
        const string def = null;

        public static void Set(string token)
        {
            PlayerPrefs.SetString(key, token);
        }

        public static string Get()
        {
            return PlayerPrefs.GetString(key, def);
        }
    }
}
