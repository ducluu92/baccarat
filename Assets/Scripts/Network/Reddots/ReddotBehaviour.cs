using Assets.Scripts.Settings;
using Assets.Scripts.Network.Seq;
using Assets.Scripts.Utils;
using Module.Apis.ApiDefinition;
using Module.Apis.Networks;
using Module.Apis.Notifys;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.Loggers;
using UnityEngine.UI;

namespace Assets.Scripts.Network.Reddots
{
    public class ReddotBehaviour : MonoBehaviour
    {
        // UI 요소
        public GameObject Reddot_Notice;
        public GameObject Reddot_charge;
        public GameObject Reddot_NotifyToCharge;
        public Text Reddot_NotifyToChargeText;


        // 플레이어 호출을 위한 객체
        public KLNetwork NetworkHandler;

        // 타이머 확인
        private ReddotTimer time;
        private bool isRequest;
        private int error;

        // 데이터 저장
        private ReddotBag bag;
        
        #region Unity 기능

        private void Start()
        {
            bag = ReddotBag.Instance;
            time = new ReddotTimer(15.0f);
            isRequest = false;

            // URI 요청

            StartCoroutine(FirstReddot());
            
        }

        private void Update()
        {
            // https://bluemeta.tistory.com/1
            // 델타 타임을 추가

            if(time == null)
            {
                time = new ReddotTimer(15.0f);
            }

            time.Update(Time.deltaTime);

            if (time.IsRefrash())
            {
                if (isRequest)
                {
                    return;
                }

                // 요청 시작
                RequestStart();

                // URI 요청
                ReddotRefrash();
            }
        }

        #endregion

        #region 최초 로딩

        private IEnumerator FirstReddot() 
        {
            yield return null;//new WaitForSeconds(3);

            // 요청 시작
            RequestStart();


            ReddotRefrash();
        }

        #endregion

        #region Request 제어

        public void RequestStart()
        {
            isRequest = true;
        }

        public void RequestWait()
        {
            isRequest = false;
        }

        public void RequestEnd()
        {
            time.Clear();
            isRequest = false;
            error = 0;
        }

        public void RequestError()
        {
            error++;

            if (error > 3)
            {
                // 프로그램 종료 로직
            }
        }

        #endregion

        #region API - Reddot Refrash

        public void ReddotRefrash()
        {
            MyLog.Write(LogLocation.Api, "Reddot-Call");

            // 시퀸스 체크
            //var status = SequenceManger.Call(out var seq);
            //switch (status)
            //{
            //    case Module.Apis.ApiDefinition.SequenceStatusByCall.OK:
            //        break;
            //    case Module.Apis.ApiDefinition.SequenceStatusByCall.Called:
            //        MyLog.Write(LogLocation.Api, "Reddot-Wait");
            //        RequestWait();
            //        return;
            //}

            // 리퀘스트
            var info = UserInfoManager.loginInfo;
            ReddotRefrashRequest req = new ReddotRefrashRequest
            {
                UUID = info.UUID,
                Token = info.Token,
                Sequence = 0//seq
            };

            // URL
            var url = ApiUrlHelper.Get(UrlNotify.ReddotCall, JsonConvert.SerializeObject(req));
            StartCoroutine(ResponceByRefrash(url));
        }


        public IEnumerator ResponceByRefrash(string uri)
        {
            bool isSuccess = true;
            string json = "";

            // 웹 요청부
            using (UnityWebRequest req = UnityWebRequest.Get(uri))
            {
                req.timeout = AppGlobalSetting.Api_TimeOut;
                yield return req.SendWebRequest();

                if (req.isNetworkError || req.isHttpError)
                {
                    isSuccess = false;
                }
                else
                {
                    json = req.downloadHandler.text;
                }
            }

            bool isNotice = false;
            bool isInfo = false;
            bool isNotifyToCharge = false;

            List<ReddotType> nomalCheck = new List<ReddotType>();

            // 결과 확인
            if (isSuccess)
            {
                if (MyJsonHelper<ReddotRefrashResponce>.Deserialize(json, out var res))
                {
                    if (res.Status == CommonError.OK)
                    {
                        // 데이터 처리부
                        bag.Set(res.Reddots);

                        // UI 처리부
                        //Reddot_Notice.SetActive(false);
                        //Reddot_charge.SetActive(false);

                        foreach (var reddot in res.Reddots)
                        {
                            switch (reddot.Types)
                            {
                                case ReddotType.Notice:
                                case ReddotType.Question_Applcation:
                                case ReddotType.Question_Replied:
                                    isNotice = true;
                                    break;
                                case ReddotType.Account:
                                    isInfo = true;
                                    break;
                                case ReddotType.NotifyToCharge:
                                    isNotifyToCharge = true;
                                    break;
                                default:
                                    if (!nomalCheck.Contains(reddot.Types)) 
                                    {
                                        nomalCheck.Add(reddot.Types);
                                    }
                                    break;
                            }
                        }

                        // 호출 처리부
                        RequestEnd();
                    }
                    else
                    {
                        RequestError();
                    }
                    
                    // 시퀸스 처리
                    //if (ExitManager.DisplayBySeq(res.Sequence))
                    //{
                    //    // TODO : 차후 메시지 안내 후 종료
                    //    Application.Quit();
                    //}

                    /* 시퀸스 처리 이후 처리 진행 */
                    // 유저 메시지 정보 로딩
                    if (isNotice)
                    {
                        Reddot_Notice.SetActive(true);
                        MyLog.Write(LogLocation.Api, "Reddot-Notice");
                    }

                    // 유저 재화 정보 로딩
                    if (isInfo)
                    {
                        yield return new WaitForSeconds(4);
                        NetworkHandler.LoadPlayerInfo(UserInfoManager.loginInfo.UUID, UserInfoManager.loginInfo.Token);
                        MyLog.Write(LogLocation.Api, "Reddot-Currcncy");

                        nomalCheck.Add(ReddotType.Account);
                    }

                    // 충전 계좌 전송 완료 알림 표시
                    if(isNotifyToCharge)
                    {
                        Reddot_NotifyToChargeText.text = $"입금 하실 계좌가 전송 되었습니다.\r\n확인해주세요.";
                        Reddot_NotifyToCharge.SetActive(true);
                        MyLog.Write(LogLocation.Api, "Reddot-NotifyToCharge");

                        nomalCheck.Add(ReddotType.NotifyToCharge);
                    }

                    // 레드닷에 쌓이는 거 클리어 해줌 ( 아직 구현 안된것들 클리어용)
                    if (nomalCheck.Count > 0) 
                    {
                        yield return new WaitForSeconds(4);
                        RequestByCheck(nomalCheck);
                        MyLog.Write(LogLocation.Api, "Reddot-Clear");
                    }

                    MyLog.Write(LogLocation.Api, "Reddot-End");
                }
            }
            else
            {
                // 에러 처리
                RequestError();
                SequenceManger.CallFail();
            }
        }

        #endregion


        #region API - Clear

        public void RequestByCheck(List<ReddotType> rTypes) 
        {
            // 시퀸스 체크
            var status = SequenceManger.Call(out var seq);
            switch (status)
            {
                case Module.Apis.ApiDefinition.SequenceStatusByCall.OK:
                    break;
                case Module.Apis.ApiDefinition.SequenceStatusByCall.Called:
                    // TODO : 에러 창 띄워주기

                    return;
            }

            // 리퀘스트 제작
            var info = UserInfoManager.loginInfo;
            ReddotCheckRequest req = new ReddotCheckRequest
            {
                UUID = info.UUID,
                Token = info.Token,
                Sequence = seq,
                TargetTypes = rTypes
            };

            StartCoroutine(ResponceByCheck(req));
        }

        public IEnumerator ResponceByCheck(ReddotCheckRequest req)
        {
            var url = ApiUrlHelper.Get(UrlNotify.ReddotCheck, JsonConvert.SerializeObject(req));

            bool isSuccess = true;
            string json = "";

            // 웹 요청부
            using (UnityWebRequest web = UnityWebRequest.Get(url))
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
                if (MyJsonHelper<ReddotRefrashResponce>.Deserialize(json, out var res))
                {
                    if (res.Status == CommonError.OK)
                    {
                        // 데이터 처리부
                        bag.Removes(req.TargetTypes);

                        // TODO : UI 처리부
                        this.ReddotRefrash();

                        // 호출 처리부
                        RequestEnd();
                    }
                    else
                    {

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

    }
}
