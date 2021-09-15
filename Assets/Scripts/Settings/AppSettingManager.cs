using Module.Apis.Networks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public static class AppSettingManager
    {
        private static AppLoads appLoad;

        public static AppLoads Get() 
        {
            if (appLoad == null) 
            {
                TextAsset bindata = Resources.Load("Appsetting") as TextAsset;
                var json = Encoding.Default.GetString(bindata.bytes);
                appLoad = JsonConvert.DeserializeObject<AppLoads>(json);
            }

            return appLoad;
        }

        public static AppNetwork GetNetwork() 
        {
            var data = Get();
            return data.Networks.Find(c => c.Location == data.NetworkTarget);
        }


        public static string GetHost() 
        {
            var data = Get();
            var url = "";

            foreach (var network in data.Networks)
            {
                if (network.Location == data.NetworkTarget)
                {
                    url = $"{network.ApiByHost}";
                    break;
                }
            }

            return url;
        }


        public static bool GetApi(out HttpType type, out string url) 
        {
            var data = Get();

            foreach (var network in data.Networks)
            {
                if (network.Location == data.NetworkTarget) 
                {
                    url = $"{network.ApiByHost}:{network.ApiByPort}";
                    type = network.ApiByHttp;
                    return true;
                }

            }

            url = "";
            type = 0;
            return false;
        }

        public static bool GetSocket(out string url, out int port)
        {
            var data = Get();

            foreach (var network in data.Networks)
            {
                if (network.Location == data.NetworkTarget)
                {
                    url = $"{network.NetworkByHost}";
                    port = network.NetworkByPort;
                    return true;
                }

            }
            url = "";
            port = 0;
            return false;
        }

        public static string GetVersion()
        {
            var data = Get();
            return data.Version;
        }

        public static EnvironmentType GetEnv() 
        {
            var data = Get();
            return data.Environment;
        }
    }
}
