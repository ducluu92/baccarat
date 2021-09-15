using Assets.Scripts.Baccarrat;
using Assets.Scripts.Settings;
using Assets.Scripts.Network;
using Assets.Scripts.Network.Seq;
using Module.Apis.ApiDefinition;
using Module.Apis.Bakaras.Builders;
using Module.Apis.Logouts;
using Module.Apis.Networks;
using Module.Apis.Rooms;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Module.Apis.Bankings;
using System.ComponentModel.Design.Serialization;
using UnityEngine.UI;
using Module.Apis.Messages;
using Assets.Scripts.Pages;
using Module.Apis.Notifys;
using Assets.Scripts.Utils;
using Assets.Scripts.Loggers;
using Assets.Scripts.Global;
using Module.Apis.Bakaras;
using System.Linq;
using Module.Utils.Currency;
using System.IO;
using Assets.Scripts.Model.Charge;
using Assets.Scripts.Services.Lobby;

public class KLNetwork_Lobby : MonoBehaviour
{
    public GameObject errorAlertPanel;
    public Text errorAlertPanel_Text;
    public bool AlertError;

    public GameObject loadingRoomPanel;
    
    public GameObject roomContent;
    public UIRoomItem roomItem;

    public GameObject ChargeListContent;
    public UIChargeListItem ChargeListItem;


    public UIBankingInfo BankingInfo_charge;
    public UIBankingInfo BankingInfo_excharge;

    public GameObject ExChargeListContent;

    public GameObject PointLogListContent;
    public UIPointLogListItem PointLogListItem;

    public GameObject NoticeContent;
    public UINoticeItem NoticeItem;
    public GameObject QuestionContent;
    public GameObject QuestionIndexContent;

    public GameObject NoticeIndexContent;
    public UIIndexButton IndexButtonItem;

    public Text ChargeAmount;
    public Text ExChargeAmount;
    public Text PIN;

    public GameObject Reddot_Notice;
    public GameObject Reddot_charge;

    public GameObject Roomlist;
    public UIMultiRoomItem MultiRoomItem;
    public Transform MultiRoomContent;

    public GameObject LogoutPanel;
    public GameObject Obj_AlertBox;

    public GameObject ObjChargeReddot;

    public GameObject ObjChangeListReddot;

    private List<UIRoomItem> roomItemList;

    // Notice 팝업 게임오브젝트와 표시되는 텍스트 개체.
    public GameObject Notice_Popup;
    public Text Notice_Text;

    public bool LoadingRoomData = false;

    private float roomContentOffset_T = 0.0f;
    private float roomContentOffset_B = 219.0f;

    const string location = "KLNetwork_Lobby";

    private ICashChargeService _cashChargeService;

    void Start()
    {
        roomItemList = new List<UIRoomItem>();
        _cashChargeService = new CashChargeService();

        RatingLoadingRequest();
        ChargeListRequest();

        Time.timeScale = 1.0f;
    }

    #region Clean Methods

    public void Clean_RoomList()
    {
        if(roomContent.transform.childCount > 0)
        {
            for (int i = 0; i < roomContent.transform.childCount; i++)
            {
                Destroy(roomContent.transform.GetChild(i).gameObject);
            }
            roomContentOffset_T = 0.0f;
            roomContentOffset_B = 219.0f;
        }
    }

    public void CLean_Lists(Transform content)
    {
        if(content.childCount > 0)
        {
            for (int i = 0; i < content.childCount; i++)
            {
                Destroy(content.GetChild(i).gameObject);
            }
        }
    }

    #endregion

    #region Rating_Loading
    public void RatingLoadingRequest()
    {
        LoadingRoomData = true;

        var request = new RatingRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            RatingIdx = "Rookies",
            Sequence = 0
        };

        var url = ApiUrlHelper.Get(UrlRoom.RatingList, JsonConvert.SerializeObject(request));

        StartCoroutine(RatingLoadingResponse(url));
    }

    public IEnumerator RatingLoadingResponse(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.timeout = AppGlobalSetting.Api_TimeOut;
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                LoadingRoomData = false;

                // 알림창
                ErrorByServer();

                // TODO : 알림창 확인 후 종료로 전환 
                Application.Quit();
            }
            else
            {
                RatingResponce responseData = JsonConvert.DeserializeObject<RatingResponce>(webRequest.downloadHandler.text);

                if(responseData.Status == CommonError.OK)
                {
                    UserInfoManager.ratingDataList = responseData.Datas;
                    ReddotCallRequest();
                }
                else
                {
                    SceneChanger.CallSceneLoader("Login");
                }
            }
        }
    }
    #endregion

    private void SaveCashCheckList(IOrderedEnumerable<ChargeApplicationData> enumerableData)
    {
        List<CashRequest> cashRequestList = new List<CashRequest>();

        foreach (ChargeApplicationData data in enumerableData)
        {
            cashRequestList.Add(new CashRequest()
            {
                Idx = data.Idx,
                Status = data.Status,
                IsRead = data.Status == "계좌 전송" ? false : true
            });
        }

        CashRequestList cashList = new CashRequestList()
        {
            CashList= cashRequestList
        };

        _cashChargeService.CreateList(cashList);
    }

    #region Room_Loading
    public void RoomLoadingRequest(int RatingIndex)
    {
        MyLog.Write(location, "RoomLoadingRequest");

        Clean_RoomList();

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new RoomListRequest { UUID = UserInfoManager.loginInfo.UUID, Token = UserInfoManager.loginInfo.Token, RatingIndex = RatingIndex, Sequence = seq };

        var getRoomURL = ApiUrlHelper.Get(UrlRoom.GetRoomList, JsonConvert.SerializeObject(request));

        StartCoroutine(RoomLoadingResponse(getRoomURL));
    }

    


    public IEnumerator RoomLoadingResponse(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {

            webRequest.timeout = AppGlobalSetting.Api_TimeOut;
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;


            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LoadingRoomData = false;

                // 알림창
                ErrorByServer();

                // 콜 실패
                SequenceManger.CallFail();

                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                Application.Quit();
            }
            else
            {
                MyLog.Write(location, "RoomLoadingResponce");

                RoomListResponce responseData = JsonConvert.DeserializeObject<RoomListResponce>(webRequest.downloadHandler.text);

                Debug.Log(responseData.Status);

                if(responseData.Status == CommonError.OK)
                {


                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        MyLog.Write(location, "RoomLoadingResponce - Error");
                        Application.Quit();
                    }

                    LoadingRoomData = false;
                    RoomInfoManager.roomdata = responseData;
                    foreach(RoomData data in RoomInfoManager.roomdata.Datas)
                    {
                        roomItem.RoomType = (int)data.Type;
                        roomItem.v_RoomID = data.Index;
                        roomItem.v_RoomName = data.RoomName; // "테스트입니다..."
                        roomItem.v_CurrentUserCount = data.PlayerNow;
                        roomItem.v_MaxUserCount = data.PlayerMax;
                        roomItem.MinBetting = data.Min;
                        roomItem.MaxBetting = data.Max;
                        roomItem.Obj_AlertBox = Obj_AlertBox;

                        roomItem.chinaboard.GenerateSimpleBoard(BkBoardBuilder.BuildBySimple(data.Board));

                        var newRoomItem = Instantiate(roomItem, new Vector3(0, 0, 0), Quaternion.identity);

                        newRoomItem.boarddata = data.Board;//.ConvertAll(o => new BkSimpleResult());

                        //Utils.SetTop(newRoomItem.GetComponent<RectTransform>(), roomContentOffset_T);
                        //Utils.SetBottom(newRoomItem.GetComponent<RectTransform>(), roomContentOffset_B);

                        newRoomItem.transform.SetParent(roomContent.transform, false);
                        
                        roomItemList.Add(newRoomItem);

                        //roomContentOffset_T += 80.0f;
                        //roomContentOffset_B -= 80.0f;
                    }
                }
                else
                {
                    MyLog.Write(location, $"RoomLoadingResponce2 -{responseData.Status.ToString()}");

                    LoadingRoomData = false;
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

    #region Charge
    public void ChargeRequest(int money)
    {
        Debug.Log("Charge-Request");

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }


        var request = new ChargeApplicationRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Money = money,
            Sequence = seq
        };

        var getBankingURL = ApiUrlHelper.Get(UriBanking.ChargeApplication, JsonConvert.SerializeObject(request));
        Debug.Log(getBankingURL);
        StartCoroutine(ChargeResponse(getBankingURL));
    }

    public IEnumerator ChargeResponse(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);

                ApplicationKill();
            }
            else
            {
                ChargeApplicationResponce responseData = JsonConvert.DeserializeObject<ChargeApplicationResponce>(webRequest.downloadHandler.text);

                if (responseData.Status == CommonError.OK)
                {
                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;

                    TurnOnNoticePopup("입금 요청이 완료되었습니다.");
                }
                else
                {
                    LoadingRoomData = false;
                    
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

    #region ExCharge
    public void ExChargeRequest(int point,string pin)
    {
        Debug.Log("ExCharge-Request");

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new ExChargeApplicationRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            PIN = pin,
            Point = point,
            Sequence = seq
        };

        var getBankingURL = ApiUrlHelper.Get(UriBanking.ExChargeApplication, JsonConvert.SerializeObject(request));
        Debug.Log(getBankingURL);
        StartCoroutine(ExChargeResponse(getBankingURL));
    }

    public IEnumerator ExChargeResponse(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();


            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);

                ApplicationKill();
            }
            else
            {
                ExChargeApplicationResponce responseData = JsonConvert.DeserializeObject<ExChargeApplicationResponce>(webRequest.downloadHandler.text);

                Debug.Log(responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    Debug.Log("ExChargeResponse");

                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;

                    TurnOnNoticePopup("환전 요청이 완료되었습니다.");
                }
                else
                {
                    LoadingRoomData = false;

                    TurnOnNoticePopup("환전 요청을 실패했습니다.");

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

    #region ChargeList

    public void ChargeListRequest()
    {
        CLean_Lists(ChargeListContent.transform);
        Debug.Log("ChargeList-Request");

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }


        var request = new ChargeListRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq
        };

        var getBankingURI = ApiUrlHelper.Get(UriBanking.ChargeList, JsonConvert.SerializeObject(request));

        StartCoroutine(ChargeListResponse(getBankingURI));
    }

    public IEnumerator ChargeListResponse(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);

                ApplicationKill();
            }
            else
            {

                ChargeListResponce responseData = JsonConvert.DeserializeObject<ChargeListResponce>(webRequest.downloadHandler.text);

                Debug.Log("ChangeList Response >> " + responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;

                    var Datas = from i in responseData.Datas orderby i.Idx descending select i;

                    SaveCashCheckList(Datas);

                    bool Reddot_chargeActive = false;
                    bool Obj_Charge_ReddotActive = false;

                    foreach (ChargeApplicationData data in Datas)
                    {
                        //아이템 생성.
                        ChargeListItem.number.text = data.Idx.ToString();
                        ChargeListItem.Amount.text = CurrencyConverter.Commma(data.Money) + " 원"; // String.Format("{0:#,0}",data.Money);
                        ChargeListItem.Status.text = data.Status;
                        ChargeListItem.Date.text = data.Date.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                        _cashChargeService.InsertItem(new CashRequest()
                        {
                            Idx = data.Idx,
                            Status = data.Status,
                            IsRead = !(data.Status == "계좌 전송"),
                        });

                        switch (data.Status)
                        {
                            case "입금 신청":
                                ChargeListItem.Status.color = new Color(1,0.75f,0);
                                ChargeListItem.AccountButton.SetActive(false);
                                ChargeListItem.RecordButton.SetActive(false);
                                ChargeListItem.AccountButton_Exchange.SetActive(false);
                                ChargeListItem.RecordButtton_Exchange.SetActive(false);
                                break;
                            case "계좌 전송":
                                ChargeListItem.Status.color = new Color(0.49f,0.58f,0.74f);
                                ChargeListItem.AccountButton.SetActive(true);
                                bool ChargeIsRead = !_cashChargeService.GetChargeIsRead(data.Idx);

                                ChargeListItem.AccountButtonReddot.SetActive(ChargeIsRead);
                                
                                if(ChargeIsRead)
                                {
                                    Reddot_chargeActive = true;
                                    Obj_Charge_ReddotActive = true;
                                }

                                ChargeListItem.CashChargeListReddot = Reddot_charge;
                                ChargeListItem.CashChargeReddot = ObjChargeReddot;
                                ChargeListItem.RecordButton.SetActive(false);
                                ChargeListItem.AccountButton_Exchange.SetActive(false);
                                ChargeListItem.RecordButtton_Exchange.SetActive(false);

                                ChargeListItem.CompanyAccountOwner = data.CompanyAccountOwner;
                                ChargeListItem.CompanyAccountName = data.CompanyAccountName;
                                ChargeListItem.CompanyAccountNumber = data.CompanyAccountNumber;

                                break;
                            case "충전 거절":
                                ChargeListItem.Status.color = new Color(0.83f,0.007f,0.007f);
                                ChargeListItem.AccountButton.SetActive(false);
                                ChargeListItem.RecordButton.SetActive(false);
                                ChargeListItem.AccountButton_Exchange.SetActive(false);
                                ChargeListItem.RecordButtton_Exchange.SetActive(false);
                                break;
                            case "충전 완료":
                                ChargeListItem.Status.color = new Color(0.43f,0.67f,0.27f);
                                ChargeListItem.AccountButton.SetActive(false);
                                ChargeListItem.RecordButton.SetActive(true);
                                ChargeListItem.AccountButton_Exchange.SetActive(false);
                                ChargeListItem.RecordButtton_Exchange.SetActive(false);

                                ChargeListItem.Money = data.Money;
                                ChargeListItem.transferredMoney = data.TransferredMoney;
                                ChargeListItem.Point = data.Point;
                                ChargeListItem.Bonus = data.Bonus;
                                var date = (DateTime)data.ProcessedDate;
                                ChargeListItem.ProcessedDate = date.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                                break;
                        }

                        var newRoomItem = Instantiate(ChargeListItem, new Vector3(0, 0, 0), Quaternion.identity);

                        //Utils.SetTop(newRoomItem.GetComponent<RectTransform>(), roomContentOffset_T);
                        //Utils.SetBottom(newRoomItem.GetComponent<RectTransform>(), roomContentOffset_B);

                        newRoomItem.transform.SetParent(ChargeListContent.transform, false);

                        //roomContentOffset_T += 80.0f;
                        //roomContentOffset_B -= 80.0f;
                    }

                    Debug.Log("Reddot_chargeActive >> " + Reddot_chargeActive);
                    Debug.Log("Obj_Charge_ReddotActive >> " + Obj_Charge_ReddotActive);

                    Reddot_charge.SetActive(Reddot_chargeActive);
                    ObjChargeReddot.SetActive(Obj_Charge_ReddotActive);
                }
                else
                {
                    LoadingRoomData = false;
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

    #region ExChargeList

    public void ExChargeListRequest()
    {
        Debug.Log("ExChargeList-Request");
        CLean_Lists(ExChargeListContent.transform);

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new ExChargeListRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq
        };

        var getBankingURI = ApiUrlHelper.Get(UriBanking.ExChargeList, JsonConvert.SerializeObject(request));

        StartCoroutine(ExChargeListResponse(getBankingURI));
    }

    public IEnumerator ExChargeListResponse(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                ApplicationKill();
            }
            else
            {
                ExChargeListResponce responseData = JsonConvert.DeserializeObject<ExChargeListResponce>(webRequest.downloadHandler.text);

                Debug.Log("ExChargeListResponce Response >> " + responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;

                    foreach (ExchangeApplicationData data in responseData.Datas)
                    {
                        //아이템 생성.
                        ChargeListItem.number.text = data.Idx.ToString();
                        ChargeListItem.Amount.text = String.Format("{0:#,0}", data.ApplicationExchange);
                        ChargeListItem.Status.text = data.Status;
                        ChargeListItem.Date.text = data.ApplicationDate.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                        switch (data.Status)
                        {
                            case "환전 완료":
                                ChargeListItem.Money = data.ApplicationExchange;
                                ChargeListItem.transferredMoney = data.ApplicationExchange + data.AdjustmentExchange;
                                ChargeListItem.AccountButton.SetActive(false);
                                ChargeListItem.RecordButton.SetActive(false);
                                ChargeListItem.AccountButton_Exchange.SetActive(false);
                                ChargeListItem.RecordButtton_Exchange.SetActive(true);
                                break;
                            default:
                                ChargeListItem.AccountButton.SetActive(false);
                                ChargeListItem.RecordButton.SetActive(false);
                                ChargeListItem.AccountButton_Exchange.SetActive(false);
                                ChargeListItem.RecordButtton_Exchange.SetActive(false);
                                break;
                        }

                        ChargeListItem.Status.color = new Color(1.0f, 1.0f, 1.0f);

                        var newRoomItem = Instantiate(ChargeListItem, new Vector3(0, 0, 0), Quaternion.identity);

                        newRoomItem.transform.SetParent(ExChargeListContent.transform, false);
                    }
                }
                else
                {
                    LoadingRoomData = false;

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

    #region UseList

    public void PointLogListRequest()
    {
        Debug.Log("PointLogList-Request");
        CLean_Lists(PointLogListContent.transform);

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new ApiPointLogRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq
        };

        var getBankingURI = ApiUrlHelper.Get(UriBanking.PointLogList, JsonConvert.SerializeObject(request));

        StartCoroutine(PointLogListResponse(getBankingURI));
    }

    public IEnumerator PointLogListResponse(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                ApplicationKill();
            }
            else
            {
                ApiPointLogResponce responseData = JsonConvert.DeserializeObject<ApiPointLogResponce>(webRequest.downloadHandler.text);

                Debug.Log(responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    Debug.Log("PointLogList-Responce");

                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;

                    foreach (ApiPointLog data in responseData.Datas)
                    {
                        //아이템 생성.
                        PointLogListItem.number.text = data.Idx.ToString();
                        PointLogListItem.RoomTurn.text = string.Format("{0:D4}", data.RoomTurn);
                        PointLogListItem.LogType.text = data.LogType.ToString();
                        PointLogListItem.BettingTarget.text = data.BattingTarget;
                        PointLogListItem.ChangePoint.text = data.ChangePoint.ToString();
                        PointLogListItem.AfterPoints.text = data.AfterPoint.ToString();
                        PointLogListItem.Date.text = data.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm");

                        var newRoomItem = Instantiate(PointLogListItem, new Vector3(0, 0, 0), Quaternion.identity);

                        //Utils.SetTop(newRoomItem.GetComponent<RectTransform>(), roomContentOffset_T);
                        //Utils.SetBottom(newRoomItem.GetComponent<RectTransform>(), roomContentOffset_B);

                        newRoomItem.transform.SetParent(PointLogListContent.transform, false);

                        //roomContentOffset_T += 80.0f;
                        //roomContentOffset_B -= 80.0f;
                    }
                }
                else
                {
                    LoadingRoomData = false;
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

    #region NoticeList

    public void NoticeListRequest(int pageIndex = 1)
    {
        CLean_Lists(NoticeContent.transform);;
        CLean_Lists(NoticeIndexContent.transform);;
        Debug.Log("NoticeList-Request");

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }



        var request = new MessageNoticeRequest
        {
            LastIndex = 0,
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq
        };

        var getNoticeURI = ApiUrlHelper.Get(UrlMessage.notice, JsonConvert.SerializeObject(request));

        StartCoroutine(NoticeListResponse(getNoticeURI, pageIndex));
    }

    public IEnumerator NoticeListResponse(string uri,int pageindex)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                ApplicationKill();
            }
            else
            {
                MessageNoticeResponce responseData = JsonConvert.DeserializeObject<MessageNoticeResponce>(webRequest.downloadHandler.text);

                Debug.Log(responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    Debug.Log("NoticeList-Responce");

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;


                    // 데이터 저장 
                    NoticePage.SetByData(responseData.Notices);
                    NoticePage.SetByPage(pageindex);

                    // UI 호출
                    foreach (var data in NoticePage.Data.Paged)
                    {
                        NoticeItem.Index.text = data.Index.ToString();
                        NoticeItem.Date.text = data.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                        NoticeItem.Type.text = "공지사항";
                        NoticeItem.Message.text = data.Header;
                        NoticeItem.NewImage.SetActive(data.IsNew);
                        NoticeItem.body = data.Body;

                        var newNoticeItem = Instantiate(NoticeItem, new Vector3(0, 0, 0), Quaternion.identity);
                        newNoticeItem.transform.SetParent(NoticeContent.transform, false);
                    }

                    foreach (int data in NoticePage.Data.MovePages)
                    {
                        IndexButtonItem.index = data;
                        IndexButtonItem.Initialize(this);
                        
                        var newIndexButton = Instantiate(IndexButtonItem, new Vector3(0, 0, 0), Quaternion.identity);

                        newIndexButton.transform.SetParent(NoticeIndexContent.transform, false);

                    }

                    //ClearReddot(6);

                }
                else
                {
                    LoadingRoomData = false;
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

    #region QuestionList

    public void QuestionListRequest_Init(int index = 0)
    {
        this.QuestionListRequest(index, 1);
    }

    public void QuestionListRequest(int index = 0, int pageIndex = 1)
    {
        CLean_Lists(QuestionIndexContent.transform);;
        CLean_Lists(QuestionContent.transform);;
        Debug.Log("QuestionList-Request");

        LoadingRoomData = true;

        SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }


        QuestionType questiontype;

        switch (index)
        {
            case 0: //Banking
                questiontype = QuestionType.Banking;
                break;
            case 1: //Account
                questiontype = QuestionType.Account;
                break;
            case 2: //Etcs
                questiontype = QuestionType.Etc;
                break;
            default:
                questiontype = QuestionType.Etc;
                break;
        }

        var request = new MessageQuestionListRequest
        {
            LastIndex = 0,
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Type = questiontype,
            Sequence = seq
        };

        var getListURI = ApiUrlHelper.Get(UrlMessage.questionList, JsonConvert.SerializeObject(request));

        StartCoroutine(QuestionListResponse(getListURI, pageIndex));
    }

    public IEnumerator QuestionListResponse(string uri, int pageindex)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                ApplicationKill();
            }
            else
            {
                MessageQuestionListResponce responseData = JsonConvert.DeserializeObject<MessageQuestionListResponce>(webRequest.downloadHandler.text);

                //Debug.Log(responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    Debug.Log("MessageList-Responce");
                    
                    QuestionContent.SetActive(false);
                    QuestionContent.SetActive(true);

                    QuestionIndexContent.SetActive(false);
                    QuestionIndexContent.SetActive(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;

                    // 데이터 저장 
                    QuestionPage.SetByData(responseData.Questions);
                    QuestionPage.SetByPage(pageindex);

                    // UI 호출
                    foreach (var data in QuestionPage.Data.Paged)
                    {
                        NoticeItem.Index.text = data.Index.ToString();
                        NoticeItem.Date.text = data.QuestionDate.ToString(); // data.Date.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                        NoticeItem.Message.text = data.HeaderByUser;

                        NoticeItem.body = "";

                        if (data.BodyByManager == null) {
                            NoticeItem.body += "\n <color=#fcba03>[내 문의 내역]</color>\n " + data.BodyByUser;
                            NoticeItem.body += "\n\n\n <color=#fcba03>[관리자 답변]</color>\n 등록된 답변이 없습니다.\n\n";
                        }
                        else {
                            NoticeItem.body += "\n <color=#fcba03>[내 문의 내역]</color>\n " + data.BodyByUser;
                            NoticeItem.body += "\n\n\n <color=#fcba03>[관리자 답변]</color> " + data.HeaderByManager; 
                            NoticeItem.body += "\n\n <color=#fcba03>[내용]</color>\n " + data.BodyByManager + "\n\n";
                        }

                        NoticeItem.NewImage.SetActive(data.IsReplid);

                        switch (data.Type)
                        {
                            case QuestionType.Banking: //Banking
                                NoticeItem.Type.text = "뱅킹";
                                IndexButtonItem.ListType = 1;
                                break;
                            case QuestionType.Account: //Account
                                NoticeItem.Type.text = "어카운트";
                                IndexButtonItem.ListType = 2;
                                break;
                            case QuestionType.Etc: //Etc
                                NoticeItem.Type.text = "기타";
                                IndexButtonItem.ListType = 3;
                                break;
                            default:
                                NoticeItem.Type.text = "기타";
                                IndexButtonItem.ListType = 3;
                                break;
                        }



                        var newNoticeItem = Instantiate(NoticeItem, new Vector3(0, 0, 0), Quaternion.identity);
                        newNoticeItem.transform.SetParent(QuestionContent.transform, false);
                    }

                    foreach (int data in NoticePage.Data.MovePages)
                    {

                        IndexButtonItem.index = data;
                        IndexButtonItem.Initialize(this);

                        var newIndexButton = Instantiate(IndexButtonItem, new Vector3(0, 0, 0), Quaternion.identity);

                        newIndexButton.transform.SetParent(QuestionIndexContent.transform, false);

                    }

                }
                else
                {
                    LoadingRoomData = false;
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


    #region SendMessage
    public void SendMessageRequest(string header, string body,int index)
    {
        Debug.Log("SendMessage-Request");

        LoadingRoomData = true;

        //SequenceManger.SetDebug(true);

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        QuestionType questiontype;

        switch(index)
        {
            case 0: //Banking
                questiontype = QuestionType.Banking;
                break;
            case 1: //Account
                questiontype = QuestionType.Account;
                break;
            case 2: //Etcs
                questiontype = QuestionType.Etc;
                break;
            default:
                questiontype = QuestionType.Etc;
                break;
        }


        var request = new MessageQuestionRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Header = header,
            Body = body,
            Type = questiontype,
            Sequence = seq
        };

        var getMessageURI = ApiUrlHelper.Get(UrlMessage.questionApplition, JsonConvert.SerializeObject(request));
        StartCoroutine(SendMessageResponse(getMessageURI));

    }

    public IEnumerator SendMessageResponse(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                ApplicationKill();
            }
            else
            {
                MessageQuestionResponce responseData = JsonConvert.DeserializeObject<MessageQuestionResponce>(webRequest.downloadHandler.text);

                //Debug.Log(responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    Debug.Log("SendMessage-Response");

                    TurnOnNoticePopup("메시지 전송을 완료했습니다.");

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;
                }
                else
                {
                    LoadingRoomData = false;

                    TurnOnNoticePopup("메시지를 전송할 수 없습니다.");

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

    #region Load User Infomation (For Charge/Excharge)

    public void UserInfoRequest()
    {
        Debug.Log("UserInfo-Request");

        LoadingRoomData = true;

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new BakingInfoRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq
        };

        var getBankingURL = ApiUrlHelper.Get(UriBanking.Info, JsonConvert.SerializeObject(request));
        StartCoroutine(UserInfoResponse(getBankingURL));
    }

    public IEnumerator UserInfoResponse(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.timeout = AppGlobalSetting.Api_TimeOut;

            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                ApplicationKill();
            }
            else
            {
                BakingInfoResponce responseData = JsonConvert.DeserializeObject<BakingInfoResponce>(webRequest.downloadHandler.text);

                Debug.Log(responseData.Status);

                if (responseData.Status == CommonError.OK)
                {
                    Debug.Log("UserInfo-Response");

                    BankingInfo_charge.ID.text = responseData.Data.ID;
                    BankingInfo_charge.Nickname.text = responseData.Data.Nicname;
                    BankingInfo_charge.Phone.text = responseData.Data.Phone;
                    BankingInfo_charge.Bankname.text = responseData.Data.BankName;
                    BankingInfo_charge.AccountOwner.text = responseData.Data.AccountOwner;
                    BankingInfo_charge.AccountNumber.text = responseData.Data.AccountNumber;

                    BankingInfo_excharge.ID.text = responseData.Data.ID;
                    BankingInfo_excharge.Nickname.text = responseData.Data.Nicname;
                    BankingInfo_excharge.Phone.text = responseData.Data.Phone;
                    BankingInfo_excharge.Bankname.text = responseData.Data.BankName;
                    BankingInfo_excharge.AccountOwner.text = responseData.Data.AccountOwner;
                    BankingInfo_excharge.AccountNumber.text = responseData.Data.AccountNumber;


                    BankingInfo_excharge.Rolling.text = CurrencyConverter.Commma(responseData.Data.Rolling);
                    BankingInfo_excharge.BuyCash.text = CurrencyConverter.Commma(responseData.Data.BuyCash);

                    SequenceManger.SetDebug(true);

                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;
                }
                else
                {
                    LoadingRoomData = false;
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


    #region Reddot

    public void ReddotCallRequest()
    {
        LoadingRoomData = true;

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new ReddotRefrashRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq
        };

        var getReddotURI = ApiUrlHelper.Get(UrlNotify.ReddotCall, JsonConvert.SerializeObject(request));
        StartCoroutine(ReddotCallResponce(getReddotURI));
    }

    public IEnumerator ReddotCallResponce(string uri)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.timeout = AppGlobalSetting.Api_TimeOut;
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            // 로딩 패널 종료
            EndLoading();

            if (webRequest.isNetworkError || webRequest.isHttpError)
            {
                // 알림창
                AlertError = true;
                errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                errorAlertPanel.SetActive(AlertError);

                Debug.Log(pages[page] + ": Error: " + webRequest.error);
                Application.Quit();
            }
            else
            {
                ReddotRefrashResponce responseData = JsonConvert.DeserializeObject<ReddotRefrashResponce>(webRequest.downloadHandler.text);

                if (responseData.Status == CommonError.OK)
                {
                    if (ExitManager.DisplayBySeq(responseData.Sequence))
                    {
                        Application.Quit();
                    }

                    LoadingRoomData = false;
                }
                else
                {
                    LoadingRoomData = false;
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


    #region 서브룸 

    /// <summary>
    /// 서브룸 리스트 호출
    /// </summary>
    /// <param name="roomIdx">멀티룸의 인덱스</param>
    public void SubRoomRequest(int roomIdx,string roomname, string roomusers, List<BkSimpleResult> boarddata) 
    {
        Debug.Log("SubRoomRequest");
        LoadingRoomData = true;

        // 시퀸스 처리
        var status = SequenceManger.Call(out var seq);
        switch (status)
        {
            case SequenceStatusByCall.Called:
                ErrorByCalled();
                return;
            case SequenceStatusByCall.OK:
                break;
        }

        var request = new RoomSubListRequest
        {
            UUID = UserInfoManager.loginInfo.UUID,
            Token = UserInfoManager.loginInfo.Token,
            Sequence = seq,
            MultiRoomIdx = roomIdx
        };

        StartCoroutine(SubRoomResponce(request, roomname, roomusers, boarddata));
    }

    public IEnumerator SubRoomResponce(RoomSubListRequest req,string roomname,string roomusers, List<BkSimpleResult> boarddata) 
    {
        var uri = ApiUrlHelper.Get(UrlRoom.GetSubRoomList, JsonConvert.SerializeObject(req));

        bool isSuccess = true;
        string json = "";

        // 웹 요청부
        using (UnityWebRequest web = UnityWebRequest.Get(uri))
        {
            web.timeout = AppGlobalSetting.Api_TimeOut;
            yield return web.SendWebRequest();

            if (web.isNetworkError || web.isHttpError)
            {
                isSuccess = false;
            }
            else
            {
                json = web.downloadHandler.text;
            }
        }

        // 결과 확인
        if (isSuccess)
        {
            if (MyJsonHelper<RoomSubListResponce>.Deserialize(json, out var res))
            {
                if (res.Status == CommonError.OK)
                {
                    LoadingRoomData = false;
                    // TODO : UI 처리부
                    Roomlist.SetActive(false);
                    MultiRoomItem.Roomname.text = roomname;
                    MultiRoomItem.User.text = roomusers;

                    MultiRoomItem.board.GenerateSimpleBoard(BkBoardBuilder.BuildBySimple(boarddata));
                    //MultiRoomItem.board.gameObject.SetActive(false);

                    MultiRoomItem.gameObject.SetActive(true);

                    res.Datas = res.Datas.OrderBy(x => x.Index).ToList();

                    // 기존 데이터 초기화
                    foreach (Transform child in MultiRoomContent.transform)
                    {
                        Destroy(child.gameObject);
                    }

                    // 호출 처리부
                    foreach (RoomData data in res.Datas)
                    {
                        roomItem.RoomType = (int)data.Type;
                        roomItem.v_RoomID = data.Index;
                        roomItem.v_RoomName = data.RoomName;
                        roomItem.v_CurrentUserCount = data.PlayerNow;
                        roomItem.v_MaxUserCount = data.PlayerMax;

                        var newRoomItem = Instantiate(roomItem, new Vector3(0, 0, 0), Quaternion.identity);

                        newRoomItem.transform.SetParent(MultiRoomContent.transform, false);
                    }
                }
                else
                {
                    // 에러 처리부
                    LoadingRoomData = false;
                    switch (res.Status)
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

                // 시퀸스 처리
                if (ExitManager.DisplayBySeq(res.Sequence))
                {
                    // TODO : 차후 메시지 안내 후 종료
                    Application.Quit();
                }

            }
        }
        else
        {
            SequenceManger.CallFail();
        }

    }


    #endregion


    public void TurnOnNoticePopup(string description)
    {
        Notice_Text.text = description;
        
        Notice_Popup.SetActive(true);
    }

    public void Charge()
    {
        this.ChargeRequest(Int32.Parse(ChargeAmount.text));
    }

    public void Excharge()
    {
        this.ExChargeRequest(Int32.Parse(ExChargeAmount.text), PIN.text );
    }

    public void ApplicationKill()
    {
        // 알림창
        ErrorByServer();

        // 콜 실패
        SequenceManger.CallFail();

        if (UserInfoManager.loginInfo != null) {

            var request = new LogoutRequest { UUID = UserInfoManager.loginInfo.UUID, Token = UserInfoManager.loginInfo.Token };

            var logoutURL = ApiUrlHelper.Get(UrlAccount.Logout, JsonConvert.SerializeObject(request));

            StartCoroutine(LogoutResponse(logoutURL, true));
        }
        else Application.Quit();
    }

    public void Logout()
    {
        if (UserInfoManager.loginInfo != null)
        {

            var request = new LogoutRequest { UUID = UserInfoManager.loginInfo.UUID, Token = UserInfoManager.loginInfo.Token };

            var logoutURL = ApiUrlHelper.Get(UrlAccount.Logout, JsonConvert.SerializeObject(request));

            StartCoroutine(LogoutResponse(logoutURL, false));
        }
    }

    public IEnumerator LogoutResponse(string uri, bool isQuit)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            if (webRequest.isNetworkError)
            {
                LoadingRoomData = false;
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                LogoutResponce responseData = JsonConvert.DeserializeObject<LogoutResponce>(webRequest.downloadHandler.text);

                Debug.Log("Execute Logout");

                if (responseData.IsLogout)
                {
                    if(isQuit)
                        Application.Quit();
                    else
                        SceneChanger.CallSceneLoader("Login");
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
        if(!loadingRoomPanel.activeInHierarchy && LoadingRoomData)
        loadingRoomPanel.SetActive(LoadingRoomData);

        if (Input.GetKey(KeyCode.Escape))
        {
            LogoutPanel.SetActive(true);
        }
    }

    void OnApplicationQuit()
    {
        ApplicationKill();
    }

    #region Loading

    public void StartLoading() 
    {
        LoadingRoomData = true;
        loadingRoomPanel.SetActive(LoadingRoomData);
    }

    public void EndLoading() 
    {
        LoadingRoomData = false;
        loadingRoomPanel.SetActive(LoadingRoomData);
    }

    #endregion

    #region Message

    public void ErrorByServer() 
    {
        var text = "서버에 접속할 수 없습니다.";
        ShowError(text);
    }

    public void ErrorByCalled() 
    {
        var text = "API 요청이 중복되었습니다.";
        ShowError(text);
    }

    public void ShowError(string text)
    {
        AlertError = true;
        errorAlertPanel_Text.text = text;
        errorAlertPanel.SetActive(AlertError);
    }

    #endregion
}
