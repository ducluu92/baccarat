using Assets.Scripts.Settings;
using Assets.Scripts.Network.Seq;
using Module.Apis.ApiDefinition;
using Module.Apis.Bakaras;
using Module.Apis.Networks;
using Module.Apis.Rooms;
using Newtonsoft.Json;
//using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Baccarrat
{
    public class UIRoomItem : MonoBehaviour
    {
        public Text RoomID;
        public Text RoomName;
        public Text User;
        public Button EnterButton;

        public List<BkSimpleResult> boarddata;
        public ChinaSimpleBoard chinaboard;

        public GameObject ScoreBoard;
        public GameObject Obj_AlertBox;

        public int v_RoomID = 0;
        public string v_RoomName = "-";
        public int v_CurrentUserCount = 0;
        public int v_MaxUserCount = 0;

        public Image ButtonImage;
        public Sprite[] ButtonSprites = new Sprite[2];

        public KLNetwork_Lobby NetworkHandler;

        //룸 타입. 0 : 싱글룸, 1 : 멀티룸, 2 : 서브룸
        public int RoomType = 0;

        public int MinBetting { get; set; } = 0;

        public int MaxBetting { get; set; } = 0;

        private void OnEnable()
        {
          /*  switch (this.RoomType)
            {
                case 0:
                    EnterButton.onClick.AddListener(_clickEvent);
                    break;
                case 1:
                    EnterButton.onClick.AddListener(_MultiRoomclickEvent);
                    NetworkHandler = GameObject.FindGameObjectWithTag("KLNETWORK_LOBBY").GetComponent<KLNetwork_Lobby>();
                    break;
                case 2:
                    EnterButton.onClick.AddListener(_clickEvent);
                    chinaboard.gameObject.SetActive(false);
                    break;
            }*/

            ButtonImage.sprite = ButtonSprites[this.RoomType];

            Debug.Log ( $"<color=darkblue>Starting room</color>" );
            _clickEvent ();
        }

        void Update()
        {
            RoomID.text = Convert.ToString(v_RoomID);
            RoomName.text = v_RoomName;
            User.text = Convert.ToString(v_CurrentUserCount) + "/" + Convert.ToString(v_MaxUserCount);
        }

        internal void _clickEvent()
        {
            SequenceManger.SetDebug(true);

            var status = SequenceManger.Call(out var seq);

            switch (status)
            {
                case SequenceStatusByCall.Called:
                    return;
                case SequenceStatusByCall.OK:
                    break;
            }

            var request = new ObserveRequest
            {
                UUID = UserInfoManager.loginInfo.UUID,
                Token = UserInfoManager.loginInfo.Token,
                RoomIdx = v_RoomID,
                Sequence = seq
            };

            // 여기에 룸 정보 넣기
            UserInfoManager.RoomIdx = v_RoomID;

            var joinObserve = ApiUrlHelper.Get(UrlRoom.JoinObserve, JsonConvert.SerializeObject(request));

            StartCoroutine(JoinObserveResponseHandler(joinObserve));
        }
        public IEnumerator JoinObserveResponseHandler(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Debug.Log(pages[page] + ": Error: " + webRequest.error);
                    Application.Quit();
                }
                else
                {
                    ObserveResponce responseData = JsonConvert.DeserializeObject<ObserveResponce>(webRequest.downloadHandler.text);

                    //Debug.Log(responseData.Status);

                    if (responseData.Status == CommonError.OK)
                    {
                        //SequenceManger.SetDebug(true);

                        if (ExitManager.DisplayBySeq(responseData.Sequence))
                        {
                            Application.Quit();
                        }

                        SceneChanger.CallSceneLoader("Play_Baccarat");

                    }
                    else
                    {
                        switch (responseData.Status)
                        {
                            case CommonError.FailByNotFoundRoom:
                                Application.Quit();
                                break;
                            case CommonError.FailByUnknown:
                                Application.Quit();
                                break;
                            case CommonError.FailByToken:
                                Application.Quit();
                                break;
                            case CommonError.FailByJson:
                                Application.Quit();
                                break;
                            case CommonError.FailByRattingRange:
                                break;
                        }
                    }
                }
            }
        }


        void _MultiRoomclickEvent()
        {
            if(NetworkHandler != null)
            {
                NetworkHandler.SubRoomRequest(v_RoomID,v_RoomName, User.text,boarddata);
            }
        }
    }
}
