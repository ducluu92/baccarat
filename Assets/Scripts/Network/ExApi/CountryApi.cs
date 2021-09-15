using Assets.Scripts.Settings;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Network.ExApi
{
    public class CountryApi
    {
        // https://ip.pe.kr/api/

        public delegate void NextCallApi();

        public delegate void ExitCall();

        const string uri = "https://api.ip.pe.kr/json/";

        public IEnumerator Request(NextCallApi call, ExitCall exit)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    var json = JsonConvert.DeserializeObject<PeBody>(webRequest.downloadHandler.text);
                    var setting = AppSettingManager.Get();

                    if (IsAccess(json))
                    {
                        call();
                    }
                    else 
                    {
                        exit();
                    }
                }
            }
        }

        private bool IsAccess(PeBody json) 
        {
            var setting = AppSettingManager.Get();
            
            // 세팅값 중 전체 열림
            if (setting.IsOpenAll) {
                return true;
            }

            // 위치정보 
            bool isAccess = false;

            switch (setting.NetworkTarget)
            {
                case "Local":
                    isAccess = true;
                    break;
                case "JP":
                    isAccess = JpAccess(json.country_code);
                    break;
                case "US":
                    isAccess = UsAccess(json.country_code);
                    break;
                default:
                    break;
            }

            return isAccess;
        }

        private bool UsAccess(string country_code)
        {
            return country_code == "US";
        }

        private bool JpAccess(string location) 
        {
            bool isAccess = false;

            switch (location)
            {
                case "KR":
                case "JP":
                case "CN":
                case "PH":
                    isAccess = true;
                    break;
                default:
                    break;
            }

            return isAccess;
        }
    }

    public class PeBody
    {
        public bool result { get; set; }

        public string ip { get; set; }

        public string country_code { get; set; }
    }




}
