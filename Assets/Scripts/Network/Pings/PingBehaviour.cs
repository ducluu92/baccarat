using Assets.Scripts.Settings;
using Module.Apis.Logins;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Network.Pings
{
    class PingBehaviour : MonoBehaviour
    {
        string url;
        const float refrash = 1.0f;
        float time = 0;

        public Text Txt_Ping;

        private void Start()
        {
            // Host 의 Url 을 체크한다. 
            var network = AppSettingManager.GetNetwork();
            url = network.PublicDns;
        }

        /// <summary>
        /// 업데이트 로직
        /// </summary>
        private void Update()
        {
            time += Time.deltaTime;

            if (time > refrash) 
            {
                // 핑을 보낸다.
                StartCoroutine(Ping(url));
                time = 0;
            }
        }

        const int overTime = 1000; // ms
        const int avgOver = 200; // ms
        const float notResponce = 3000.0f;

        int notResCounter = 0;

        /// <summary>
        /// 핑 보내기 로직
        /// </summary>
        public IEnumerator Ping(string hostAddr)
        {
            yield return null;

            float delta = 0.0f;
            Ping p = new Ping(hostAddr);

            while (p.isDone == false)
            {
                delta += 0.05f;
                yield return new WaitForSeconds(0.05f);

                if (delta > notResponce)
                {
                    break;
                }
            }

            if (p.time == -1)
            {
                notResCounter++;
            }
            else if (p.time > overTime)
            {
                Exit();
            }
            else
            {
                notResCounter = 0;
            }

            if (notResCounter > 3)
            {
                Exit();
            }

            var bag = PingBag.Instance;

            bag.Enqueue(p.time);

            if (AppSettingManager.GetEnv() == EnvironmentType.Development) { 
                Txt_Ping.text = $"{p.time} <size=\"44\"><color=\"#283f99\">(pkt/ms)</color></size>";

                if (!Txt_Ping.gameObject.activeSelf)
                    Txt_Ping.gameObject.SetActive(true);
            }

            if (bag.IsFull())
            {
                var avg = bag.Avg();

                if(AppSettingManager.GetEnv() == EnvironmentType.Development)
                    Txt_Ping.text = $"{avg} <size=\"44\"><color=\"#283f99\">(pkt/ms)</color></size>";

                if (avg > avgOver)
                {
                    Exit();
                }
            }
        }



        /// <summary>
        /// 게임 나가기 처리
        /// </summary>
        private void Exit() 
        {
            // TODO :
        }
    }
}
