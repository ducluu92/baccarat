using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public static class MyJsonHelper<T>
    {
        public static bool Deserialize(string json, out T data)
        {
            try
            {
                data = JsonConvert.DeserializeObject<T>(json);
            }
            catch
            {
                data = default(T);
                return false;
            }

            return true;
        }

    }
}
