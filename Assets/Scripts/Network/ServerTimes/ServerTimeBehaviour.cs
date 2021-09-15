using Assets.Scripts.Network.Apis;
using Assets.Scripts.Settings;
using Module.Apis.ApiDefinition;
using Module.Apis.Etcs;
using Module.Apis.Networks;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Network.ServerTimes
{
    public class ServerTimeBehaviour : MonoBehaviour
    {
        public DateTime ServerTime { get; private set; }

        private int reqCounter = 0;
        const int reqMax = 3;
        bool isSuccessRequest = false;

        private void Start()
        {
            // 서버 타임을 우선 내부 타임으로 초기화
            ServerTime = DateTime.UtcNow;
            StartCoroutine(SyncServer());
        }

        private void Update()
        {
            // 동기화 된 서버 타임 업데이트
            ServerTime = ServerTime.AddSeconds(Time.deltaTime);
        }

        private IEnumerator SyncServer() 
        {
            reqCounter++;

            var req = new TimeRequest
            {
                UUID = UserInfoManager.loginInfo.UUID,
                Token = UserInfoManager.loginInfo.Token
            };

            var uri = ApiUrlHelper.Get(EtcDefinition.Uri.ServerTime, JsonConvert.SerializeObject(req));
            var bef = DateTime.UtcNow;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.timeout = AppGlobalSetting.Api_TimeOut;
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    if (reqCounter <= reqMax) 
                    {
                        StartCoroutine(SyncServer());
                    }
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<TimeResponce>(webRequest.downloadHandler.text);

                    if (res.Status == CommonError.OK)
                    {
                        var afr = DateTime.UtcNow;
                        var milies = (afr - bef).TotalMilliseconds;
                        ServerTime = res.ServerUtcTime.AddMilliseconds(milies);

                        isSuccessRequest = true;
                    }
                    else 
                    {
                        if (reqCounter <= reqMax)
                        {
                            StartCoroutine(SyncServer());
                        }
                    }
                    
                }
            }
        }

        /// <summary>
        /// 예상되는 서버시간과, 패킷시간의 차를 이용하여 값을 계산한다. 
        /// </summary>
        public double TimeSpanMilies(DateTime packetTime) 
        {
            if (!isSuccessRequest) 
            {
                return 0;   
            }

            return (ServerTime - packetTime).TotalMilliseconds;
        }

    }
}
