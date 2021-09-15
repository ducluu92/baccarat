using Assets.Scripts.Baccarrat;
using Assets.Scripts.Settings;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Seq;
using Module.Apis.ApiDefinition;
using Module.Apis.Bakaras;
using Module.Apis.Bakaras.Builders;
using Module.Apis.Logouts;
using Module.Apis.Networks;
using Module.Apis.Rooms;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using Module.Utils.Currency;

public class KLNetwork_Baccarat : MonoBehaviour
{
    public GameObject loadingRoomPanel;
    public Text roundText;
    public UIBaccaratRoom BaccaratRoomItem;
    public ChinaSimpleBoard chinasimpleboard;
    public ChinaSimpleBoard chinasimpleboard_marker;
    public Text DeckCountUI_Text;
    public Text LimitBetting_Text;

    private List<UIBaccaratRoom> roomItemList;

    public bool LoadingRoomData = false;

    void Start()
    {
        roomItemList = new List<UIBaccaratRoom>();

        SetLimitBetting();
        BoardLoadingRequest();
    }

    #region SetLimitBetting
    public void SetLimitBetting()
    {
        var userSelectedRating = UserInfoManager.selectedRating;
       
        LimitBetting_Text.text = "보너스게임 당첨 상한가 : " + CurrencyConverter.Kor(userSelectedRating.UpperLimitBankerPair);
    }
    #endregion

    #region Board_Loading
    public void BoardLoadingRequest() //int RatingIndex)
    {
        Debug.Log ( $"Board loading request." );
        var status = SequenceManger.Call(out var seq);

        switch (status)
        {
            case SequenceStatusByCall.Called:
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new RoomBoardRequest 
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq,
            RoomIndex = UserInfoManager.RoomIdx
        };

        var getRoomURL = ApiUrlHelper.Get(UrlRoom.RoomBoard, JsonConvert.SerializeObject(request));

        StartCoroutine(BoardLoadingResponse(getRoomURL));
    }

    public IEnumerator BoardLoadingResponse(string uri)
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
                ApplicationKill();
            }
            else
            {
                Debug.Log ( $"<color=darkblue>Board Response received: {webRequest.downloadHandler.text}</color>" );
                RoomBoardResponce responseData = JsonConvert.DeserializeObject<RoomBoardResponce>(webRequest.downloadHandler.text);

                if (responseData.Status == CommonError.OK)
                {
                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    if(responseData.Datas.Count <= 0)
                    {
                        roundText.text = "Round 01";
                        DeckCountUI_Text.text = "416";
                    }
                    else
                    {
                        var res = BkBoardBuilder.Build(responseData.Datas);

                        BaccaratRoomItem.BigRoad.GenerateBigRoad(res.BigRoad);
                        BaccaratRoomItem.MarkerRoad.GenerateMarkerRoad(res.MarkerRoad);
                        BaccaratRoomItem.BigEye.GenerateBigEyeRoad(res.BigEyeRoad);
                        BaccaratRoomItem.SmallRoad.GenerateSmallRoad(res.SmallRoad);
                        BaccaratRoomItem.Cockroach.GenerateCockroachRoad(res.CockroachRoad);

                        BaccaratRoomItem.SetPlayerNext(res.PlayerNext);
                        BaccaratRoomItem.SetBankerNext(res.BankerNext);

                        BaccaratRoomItem.GenerateScore(responseData.Datas);

                        chinasimpleboard.GenerateSimpleBoard_Ingame(BkBoardBuilder.Build(responseData.Datas).BigRoad, responseData.Datas);
                        chinasimpleboard_marker.GenerateSimpleMarkerBoard_Ingame(res.MarkerRoad);

                        DeckCountUI_Text.text = Convert.ToString(responseData.CardRemain);

                        if (res.Origin.Count > 0)
                        {
                            var mTurn = res.Origin.Max(c => c.Turn);
                            var tSource = res.Origin.Where(c => c.Turn == mTurn).FirstOrDefault();

                            roundText.text = "Round " + (tSource.DeckSuffleNowCount + 1).ToString("D2");
                        }
                    }
                }
                else
                {
                    switch (responseData.Status)
                    {
                        case CommonError.FailByUnknown:
                            ApplicationKill();
                            break;
                        case CommonError.FailByToken:
                            ApplicationKill();
                            break;
                        case CommonError.FailByJson:
                            ApplicationKill();
                            break;
                        case CommonError.FailByRattingRange:
                            break;
                    }
                }
            }
        }
    }
    #endregion

    public void ApplicationKill()
    {
        if (UserInfoManager.loginInfo != null)
        {

            var request = new LogoutRequest { UUID = UserInfoManager.loginInfo.UUID, Token = UserInfoManager.loginInfo.Token };

            var logoutURL = ApiUrlHelper.Get(UrlAccount.Logout, JsonConvert.SerializeObject(request));

            StartCoroutine(LogoutResponse(logoutURL));
        }
        else Application.Quit();
    }

    public IEnumerator LogoutResponse(string uri)
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
            }
            else
            {
                LogoutResponce responseData = JsonConvert.DeserializeObject<LogoutResponce>(webRequest.downloadHandler.text);

                if (responseData.IsLogout)
                {
                    Application.Quit();
                }
            }
        }
    }

    public void setRoomLoading(bool isLoad)
    {
        LoadingRoomData = isLoad;
    }

    void Update()
    {
        loadingRoomPanel.SetActive(LoadingRoomData);
    }

    void OnApplicationQuit()
    {
        ApplicationKill();
    }
}
