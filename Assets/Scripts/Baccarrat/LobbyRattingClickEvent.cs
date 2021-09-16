using Assets.Scripts.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Baccarrat
{
    public class LobbyRattingClickEvent : MonoBehaviour
    {
        public int type ;                            // 1 = lookie, 2 = pro, 3 = master, 4 = grand master

        public GameObject Panel_Lobby;              // OFF
        public GameObject Panel_WatingRoom;         // ON
        public GameObject Panel_RoomList;           // ON
        public KLNetwork_Lobby Panel_MainCanvas;    // 1
        public AudioSource Audio_Open;              // Open Sound On
        
        public GameObject Obj_AlertBox;             // 알림 박스

        private void Start()
        {
            Invoke("LeagueIn", 2f);
        }
        public void LeagueIn()
        {
            if(type > 0 && type <5)
            {
                var ratingList = UserInfoManager.ratingDataList;

                UserInfoManager.selectedRating = ratingList[type - 1];

                if ((ratingList[type-1].MinPoint <= UserInfoManager.AccountInfo.Cash) && (UserInfoManager.AccountInfo.Cash <= ratingList[type - 1].MaxPoint) || AppSettingManager.GetEnv() == EnvironmentType.Development)
                {
                    Panel_Lobby.SetActive(false);
                    Panel_WatingRoom.SetActive(true);
                    Panel_RoomList.SetActive(true);
                    Panel_MainCanvas.RoomLoadingRequest(type);
                    Audio_Open.Play();
                }
                else
                {
                    Audio_Open.Play();
                    Obj_AlertBox.SetActive(true);
                }
            }
        }

    }
}
