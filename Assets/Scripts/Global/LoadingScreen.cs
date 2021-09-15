using Assets.Scripts.Network;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Settings
{
    class LoadingScreen : MonoBehaviour
    {
        public GameObject ErrorPanel;
        public Text ErrorText;
        public KLNetwork NetworkHandler;
        public KLNetwork_Baccarat NetworkHandler_baccarat;
        public KLNetwork_Lobby NetworkHandler_Lobby;

        private float Timer;

        private void OnEnable()
        {
            Timer = 0f;
        }

        private void Update()
        {
            if(gameObject.activeInHierarchy)
            {
                Timer += Time.deltaTime;
                if(Timer >= 4.0f) // 타임아웃인 경우. 현재 테스트를 위해 4초로 지정.
                {
                    Timer = 0;
                    ErrorPanel.SetActive(true);
                    ErrorText.text = "데이터를 불러오지 못했습니다. (타임아웃)";

                    if (NetworkHandler != null)
                        NetworkHandler.Loading = false;

                    if (NetworkHandler_baccarat != null)
                        NetworkHandler_baccarat.LoadingRoomData = false;

                    if (NetworkHandler_Lobby != null)
                        NetworkHandler_Lobby.LoadingRoomData = false;

                    gameObject.SetActive(false);

                }
            }
        }


    }
}
