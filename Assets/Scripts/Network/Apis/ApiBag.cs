using Assets.Scripts.Settings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace Assets.Scripts.Network.Apis
{
    public class ApiBag
    {
        #region SingleTone

        private static readonly Lazy<ApiBag> _instance = new Lazy<ApiBag>(() => new ApiBag());

        public static ApiBag Instance { get { return _instance.Value; } }

        #endregion

        private int key;

        private Queue<ApiRequest> requests;

        private Dictionary<int, object> responces;

        private bool isNotRunning;

        public ApiBag()
        {
            isNotRunning = true;
            key = 0;
            requests = new Queue<ApiRequest>();
            responces = new Dictionary<int, object>();
        }

        #region Request

        public int Request(string uri)
        {
            key++;

            var req = new ApiRequest
            {
                Key = key,
                Uri = uri
            };

            requests.Enqueue(req);

            return key;
        }

        public IEnumerator Next() 
        {
            yield return null;

            if (isNotRunning)
            {
                if (requests.Any()) 
                {
                    RequestRunner();
                }
            }
        }

        private IEnumerator RequestRunner() 
        {
            isNotRunning = false;

            var req = requests.Dequeue();
           
            using (UnityWebRequest webRequest = UnityWebRequest.Get(req.Uri))
            {
                webRequest.timeout = AppGlobalSetting.Api_TimeOut;
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    // 실패시 
                    var res = new ApiResponce
                    {
                        Key = key,
                        Action = ApiAction.FailByNetwork,
                        Data = null,
                    };

                    responces.Add(key, res);
                }
                else 
                {
                    var res = new ApiResponce
                    {
                        Key = key,
                        Action = ApiAction.OK,
                        Data = webRequest.downloadHandler.text,
                    };

                    responces.Add(key, res);
                }
            }

            isNotRunning = true;
        }

        #endregion

        #region Responce

        public bool ContainResponce(int key)
        {
            return responces.ContainsKey(key);
        }

        public object Responce(int key) 
        {
            if (responces.TryGetValue(key, out object data))
            {
                return data;
            }
            return null;
        }
        
        #endregion



    }
}
