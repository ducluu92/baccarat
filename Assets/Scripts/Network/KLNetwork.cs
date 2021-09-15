using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;
using UnityEngine.UI;
using Module.Apis.Networks;
using Module.Apis.ApiDefinition;
using Module.Apis.Infos;
using Module.Apis.SighUp;
using Module.Apis.Logins;
using Module.Apis.Seqs;
using System;
using Assets.Scripts.Network.Seq;
using Assets.Scripts.Settings;
using Module.Apis.Notifys;
using Module.Utils;
using Assets.Scripts.Loggers;
using Module.Packets.Definitions;
using Assets.Scripts.Network.ExApi;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using Assets.Scripts.Services;
using System.Runtime.InteropServices;

namespace Assets.Scripts.Network
{
    public class KLNetwork : MonoBehaviour 
    {
        public bool isLoginScene;
        public GameObject loadingPanel;
        public GameObject errorAlertPanel;
        public GameObject ExitPanel;
        public GameObject SighupPanel;

        public GameObject Panel_Login;
        public GameObject Panel_SighUp;


        public GameObject Obj_CheckImage_ID;
        public GameObject Obj_CheckImage_Email;
        public GameObject Obj_CheckImage_Nickname;
        public GameObject Obj_CheckImage_PromoCode;

        public InputField idField;
        public InputField pwdField;
        
        public Text errorAlertPanel_Text;
        public Text version_Text;

        private InputField field = null;

        public bool Loading { get; set; }
        private bool AlertError { get; set; }

        private string clientIP { get; set; }

        const string location = "";

        private IRememberMeService _rememberMeService;

        void Start()
        {
            Time.timeScale = 1.0f;

            Loading = false;
            AlertError = false;

            // 사용자 IP 초기화
            clientIP = GetRealIP();

            /* Start Setting */
            if (AppSettingManager.GetApi(out var http, out var url))
            {
                ApiUrlHelper.Set(http, url);
            }

            Time.timeScale = 1.0f;

            var networkInfo = AppSettingManager.GetNetwork();

            if (version_Text != null)
            {
                if ((AppSettingManager.GetEnv() == EnvironmentType.Development))
                {
                    version_Text.text = AppSettingManager.GetVersion() + "_" + networkInfo.Location + "_Debug";
                }
                else
                {
                    version_Text.text = AppSettingManager.GetVersion() + "_" + networkInfo.Location + "_Release";
                }
            }

            // 여기 보고 가져가서 하면됨
            CountryApi countryApi = new CountryApi();
            StartCoroutine(countryApi.Request(WorkCheck, KillApplication));

            // TODO : RememberMe Service to DI (prism lib)
            _rememberMeService = new RememberMeLocalStorageService();
        }

        public void WorkCheck()
        {
            Debug.Log("CountryApi OK !");
            if ( isLoginScene ) {
                LoginRequest ();
            }
        }

#region Login
        public void LoginRequest()
        {
            Debug.Log("Nomal-Login-Request");

            //InputField idField = GameObject.FindWithTag("Login_ID").GetComponent<InputField>();
            //InputField pwField = GameObject.FindWithTag("Login_PW").GetComponent<InputField>();

            idField.text = "testid01";
            pwdField.text = "1234";
            
            Loading = true;

            var token = TokenManager.Get();
            var req = new LoginRequest
            {
                ID = idField.text,
                Password = Convertor.Base64Encode(pwdField.text),
                DeviceID = SystemInfo.deviceUniqueIdentifier,
                DeviceType = SystemInfo.deviceModel,
                DeviceIP = clientIP == null ? "" : clientIP,
                Token = token,
                PacketVersion = SortationType.Version
            };

            Debug.Log ( $"Login Request JSON: {JsonConvert.SerializeObject ( req )}" );

            var url = ApiUrlHelper.Get(
                UrlAccount.Login,
                JsonConvert.SerializeObject(req));

            StartCoroutine(LoginResponse(url));
        }

        public IEnumerator LoginResponse(string uri)
        {
            Debug.Log("Nomal-Login-Response");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                webRequest.timeout = AppGlobalSetting.Api_TimeOut;

                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    // 로딩 패널 종료
                    Loading = false;
                    loadingPanel.SetActive(Loading);

                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                    errorAlertPanel.SetActive(AlertError);

                    //Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    // 웹 리퀘스트 성공 

                    LoginResponce responseData = JsonConvert.DeserializeObject<LoginResponce>(webRequest.downloadHandler.text);

                    if (responseData.Status == LoginStatus.Success)
                    {
                        /* Mark - 2020.07.13 */
                        SequenceManger.LoginSucceed();

                        /* Mark - 2020.07.29 - 토큰 파일에 저장 */
                        TokenManager.Set(responseData.Token);

                        UserInfoManager.loginInfo = responseData;
                        UserInfoManager.AccountInfo = responseData.AccountInfo;

                        //InputField idField = GameObject.FindWithTag("Login_ID").GetComponent<InputField>();
                        //InputField pwField = GameObject.FindWithTag("Login_PW").GetComponent<InputField>();
                        _rememberMeService.SetID(idField.text);
                        _rememberMeService.SetPassword(pwdField.text);

                        // 로그인 -> 로비 넘어가는 부분
                        SceneChanger.CallSceneLoader("Lobby");

                        //LoadPlayerInfo(UserInfoManager.loginInfo.UUID, UserInfoManager.loginInfo.Token);
                    }
                    else
                    {
                        // 로딩 패널 종료
                        Loading = false;
                        loadingPanel.SetActive(Loading);

                        Loading = false;

                        AlertError = true;

                        switch (responseData.Status)
                        {
                            case LoginStatus.FailByPacket:
                                errorAlertPanel_Text.text = "최신 버전이 아닙니다. 3초 후 다운로드 페이지로 이동합니다.\r\n설치 후 재 접속 해주시길바랍니다.";
                                errorAlertPanel.SetActive(AlertError);
                                StartCoroutine(Co_OpenDownloadURL());
                                break;
                            case LoginStatus.FailByDuplicate:
                                errorAlertPanel_Text.text = "접속 중인 사용자입니다.";
                                errorAlertPanel.SetActive(AlertError);
                                break;
                            case LoginStatus.FailByID:
                                errorAlertPanel_Text.text = "잘못된 아이디 입력하셨습니다.";
                                errorAlertPanel.SetActive(AlertError);
                                break;
                            case LoginStatus.FailByPassword:
                                errorAlertPanel_Text.text = "잘못된 비밀번호를 입력하셨습니다.";
                                errorAlertPanel.SetActive(AlertError);
                                break;
                            case LoginStatus.FailByNotActivation:
                                errorAlertPanel_Text.text = "관리자 승인을 대기하고 있습니다. 잠시 후 다시 시도해주세요.";
                                errorAlertPanel.SetActive(AlertError);
                                break;
                        }
                    }
                }

            }
        }
#endregion

        private IEnumerator Co_OpenDownloadURL()
        {
            yield return new WaitForSeconds(1.0f);
            errorAlertPanel_Text.text = "최신 버전이 아닙니다. 2초 후 다운로드 페이지로 이동합니다.\r\n설치 후 재 접속 해주시길바랍니다.";
            yield return new WaitForSeconds(1.0f);
            errorAlertPanel_Text.text = "최신 버전이 아닙니다. 1초 후 다운로드 페이지로 이동합니다.\r\n설치 후 재 접속 해주시길바랍니다.";
            yield return new WaitForSeconds(1.0f);
            Application.OpenURL("http://fftt88.com");
            yield return new WaitForSeconds(0.1f);
            Application.Quit();
        }

#region CheckID
        public void CheckIDRequest()
        {
            field = GameObject.Find("ID").GetComponentInChildren<InputField>();

            var req = new CheckIDRequest { ID = field.text };

            var url = ApiUrlHelper.Get(
                UrlAccount.CheckID,
                JsonConvert.SerializeObject(req));

            StartCoroutine(CheckIDResponse(url));
        }

        public IEnumerator CheckIDResponse(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Loading = false;

                    // 로딩 패널 종료
                    Loading = false;
                    loadingPanel.SetActive(Loading);

                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                    errorAlertPanel.SetActive(AlertError);

                    //Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    CheckIDResponce responseData = JsonConvert.DeserializeObject<CheckIDResponce>(webRequest.downloadHandler.text);

                    Loading = false;

                    switch (responseData.Status)
                    {
                        case IDStatus.OK:
                            Obj_CheckImage_ID.SetActive(true);
                            break;
                        case IDStatus.ErrorByDuplicate:
                            AlertError = true;
                            errorAlertPanel.SetActive(true);
                            errorAlertPanel_Text.text = "중복된 아이디가 존재합니다.";
                            break;
                    }
                }
            }
        }
#endregion

#region CheckNicRequest
        public void CheckNicRequest()
        {
            field = GameObject.Find("Nickname").GetComponentInChildren<InputField>();

            var req = new CheckNicRequest { Nicname = field.text };

            var url = ApiUrlHelper.Get(
             UrlAccount.CheckNic,
             JsonConvert.SerializeObject(req));

            StartCoroutine(CheckNicResponse(url));
        }

        public IEnumerator CheckNicResponse(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Loading = false;
                    // 로딩 패널 종료
                    Loading = false;
                    loadingPanel.SetActive(Loading);

                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                    errorAlertPanel.SetActive(AlertError);

                    //Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    CheckNicResponce responseData = JsonConvert.DeserializeObject<CheckNicResponce>(webRequest.downloadHandler.text);

                    Loading = false;

                    switch (responseData.Status)
                    {
                        case NicnameStatus.OK:
                            Obj_CheckImage_Nickname.SetActive(true);
                            break;
                        case NicnameStatus.ErrorByDuplicate:
                            AlertError = true;
                            errorAlertPanel.SetActive(AlertError);
                            errorAlertPanel_Text.text = "중복된 닉네임이 존재합니다.";
                            break;
                    }
                }
            }
        }
#endregion

#region CheckPromo
        public void CheckPromoRequest()
        {
            field = GameObject.Find("PromoCode").GetComponentInChildren<InputField>();

            var req = new CheckPromoRequest { PromoCode = field.text };

            var url = ApiUrlHelper.Get(
             UrlAccount.CheckPromoCode,
             JsonConvert.SerializeObject(req));

            StartCoroutine(CheckPromoResponse(url));
        }

        public IEnumerator CheckPromoResponse(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Loading = false;

                    // 로딩 패널 종료
                    Loading = false;
                    loadingPanel.SetActive(Loading);

                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                    errorAlertPanel.SetActive(AlertError);

                    //Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    CheckPromoResponce responseData = JsonConvert.DeserializeObject<CheckPromoResponce>(webRequest.downloadHandler.text);

                    Loading = false;

                    if (!responseData.IsExist)
                    {
                        field.text = "";
                        AlertError = true;
                        errorAlertPanel.SetActive(AlertError);
                        errorAlertPanel_Text.text = "존재하지 않는 추천인 코드 입니다.";
                    }
                    else
                    {
                        Obj_CheckImage_PromoCode.SetActive(true);
                    }
                }
            }
        }
#endregion

#region CheckEmail
        public void CheckEmailRequest()
        {
            field = GameObject.Find("Email").GetComponentInChildren<InputField>();

            var req = new CheckEmailRequest { Email = field.text };

            var url = ApiUrlHelper.Get(
            UrlAccount.CheckEmail,
            JsonConvert.SerializeObject(req));

            StartCoroutine(CheckEmailResponse(url));
        }

        public IEnumerator CheckEmailResponse(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Loading = false;

                    // 로딩 패널 종료
                    Loading = false;
                    loadingPanel.SetActive(Loading);

                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                    errorAlertPanel.SetActive(AlertError);
                }
                else
                {
                    CheckEmailResponce responseData = JsonConvert.DeserializeObject<CheckEmailResponce>(webRequest.downloadHandler.text);

                    Loading = false;

                    switch (responseData.Status)
                    {
                        case EmailStatus.OK:
                            Obj_CheckImage_Email.SetActive(true);
                            break;
                        case EmailStatus.ErrorByNotEmail:
                            AlertError = true;
                            errorAlertPanel.SetActive(AlertError);
                            errorAlertPanel_Text.text = "잘못된 형식입니다. 다시입력해주세요";
                            break;
                        case EmailStatus.ErrorByDuplicate:
                            AlertError = true;
                            errorAlertPanel.SetActive(AlertError);
                            errorAlertPanel_Text.text = "중복된 이메일이 존재합니다.";
                            break;
                    }
                }
            }
        }
#endregion

#region SighUp
        public void SighUpRequest()
        {
            InputField idField = GameObject.FindWithTag("SighUp_ID").GetComponent<InputField>();
            InputField pwField = GameObject.FindWithTag("SighUp_PW").GetComponent<InputField>();
            InputField pwcField = GameObject.FindWithTag("SighUp_PWC").GetComponent<InputField>();
            InputField nickField = GameObject.FindWithTag("SighUp_NICK").GetComponent<InputField>();
            InputField phoneField = GameObject.FindWithTag("SighUp_PHONE").GetComponent<InputField>();
            InputField promoField = GameObject.FindWithTag("SighUp_PROMO").GetComponent<InputField>();
            Dropdown bankDropdown = GameObject.FindWithTag("SighUp_BANK").GetComponent<Dropdown>();
            InputField accountField = GameObject.FindWithTag("SighUp_ACCOUNT").GetComponent<InputField>();
            InputField accountNumField = GameObject.FindWithTag("SighUp_ACCOUNT_NUM").GetComponent<InputField>();
            InputField pinField = GameObject.FindWithTag("SighUp_PIN").GetComponent<InputField>();

            Loading = true;

            var req = new SighUpRequest
            {
                ID = idField.text,
                Password = pwField.text,
                PasswordConfirm = pwcField.text,
                Email = "",
                NicName = nickField.text,
                PhoneNumber = phoneField.text,
                PromoCode = promoField.text,
                BankName = bankDropdown.options[bankDropdown.value].text,
                AccountNumber = accountNumField.text,
                AccountOwner = accountField.text,
                PIN = pinField.text,
                DeviceID = SystemInfo.deviceUniqueIdentifier,
                DeviceIP = clientIP == null ? "" : clientIP
            };

            var url = ApiUrlHelper.Get(
            UrlAccount.SighUp,
            JsonConvert.SerializeObject(req));

            StartCoroutine(SighUpResponse(url));
        }

        public IEnumerator SighUpResponse(string uri)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;

                if (webRequest.isNetworkError)
                {
                    Loading = false;

                    // 로딩 패널 종료
                    Loading = false;
                    loadingPanel.SetActive(Loading);

                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "네트워크 에러가 발생했습니다.";
                    errorAlertPanel.SetActive(AlertError);
                }
                else
                {
                    SighUpResponce responseData = JsonConvert.DeserializeObject<SighUpResponce>(webRequest.downloadHandler.text);

                    if (responseData.Status == SighUpStatus.OK)
                    {
                        // TODO:: 성공 화면 표시
                        Panel_Login.SetActive(true);
                        SighupPanel.SetActive(true);
                        Panel_SighUp.SetActive(false);
                        Loading = false;
                    }
                    else
                    {
                        switch(responseData.Status)
                        {
                            case SighUpStatus.ErrorByJson:
                                errorAlertPanel_Text.text = "시스템 에러가 발생 했습니다. 관리자에게 문의하거나 다시 값을 입력하여 시도해주세요.";
                                break;
                            case SighUpStatus.IDErrorByLenth:
                                errorAlertPanel_Text.text = "아이디의 길이가 잘못 되었습니다. (4~10자리 까지만 가능합니다.)";
                                break;
                            case SighUpStatus.IDErrorByStringType:
                                errorAlertPanel_Text.text = "아이디가 잘못 입력되었습니다. (영문 숫자로만 입력해주세요.)";
                                break;
                            case SighUpStatus.IDErrorByDuplicate:
                                errorAlertPanel_Text.text = "중복되는 아이디 명이 있습니다. 다른 이름을 선택해주세요.";
                                break;
                            case SighUpStatus.PwErrorByLenth:
                                errorAlertPanel_Text.text = "패스워드의 길이가 잘못 되었습니다. (4~10자리 까지만 가능합니다.)";
                                break;
                            case SighUpStatus.PwErrorByNotSame:
                                errorAlertPanel_Text.text = "입력한 패스워드와 패스워드 확인 값이 일치 하지 않습니다.";
                                break;
                            case SighUpStatus.NicErrorByLenth:
                                errorAlertPanel_Text.text = "닉네임 길이가 잘못 되었습니다. (2~10자리 까지만 가능합니다.)";
                                break;
                            case SighUpStatus.NicErrorByDuplicate:
                                errorAlertPanel_Text.text = "중복되는 닉네임이 존재합니다. 다른 닉네임을 사용해주세요.";
                                break;
                            case SighUpStatus.PonErrorByLenth:
                            case SighUpStatus.PonErrorByStringType:
                                errorAlertPanel_Text.text = "휴대폰 번호가 잘못 되었습니다. 확인 후 다시 입력해주세요.";
                                break;
                            case SighUpStatus.PromoErrorByLenth:
                            case SighUpStatus.PromoErrorByNotExist:
                                errorAlertPanel_Text.text = "잘못된 추천인 코드를 입력하셨습니다. 추천인 코드를 다시 입력하세요.";
                                break;
                            case SighUpStatus.AccountOwnerByLenth:
                                errorAlertPanel_Text.text = "잘못된 계좌주 이름입니다. 정상적인 이름을 입력해주세요.";
                                break;
                            case SighUpStatus.AccountNumberErrorByLenth:
                                errorAlertPanel_Text.text = "잘못된 계좌번호를 입력하셨습니다. 정확한 계좌번호를 입력해주세요.";
                                break;
                            case SighUpStatus.PinByLenth:
                                errorAlertPanel_Text.text = "핀 번호의 길이가 잘못 되었습니다. 4자리의 숫자만 입력해주세요.";
                                break;
                        }

                        errorAlertPanel.SetActive(true);
                        Loading = false;
                    }
                }
            }
        }
#endregion

#region LoadPlayerInfo
        public void LoadPlayerInfo(int uuid, string token)
        {
            Debug.Log("Nomal-LoadPlayerInfo-Request");

            Loading = true;

            // mark - 2020.07.13 Add seq
            var error = SequenceManger.Call(out var seq);

            switch (error)
            {
                case SequenceStatusByCall.OK:
                    break;
                case SequenceStatusByCall.Called:
                    // Load Fail
                    AlertError = true;
                    errorAlertPanel_Text.text = "중복 호출이 발생하였습니다.";
                    errorAlertPanel.SetActive(AlertError);

                    // todo : 로그아웃 요청
                    return;
            }

            var req = new AccountInfoRequest { UUID = uuid, Token = token, Sequence = seq };

            var url = ApiUrlHelper.Get(
            UrlAccount.AccountInfo_Get,
            JsonConvert.SerializeObject(req));

            StartCoroutine(LoadPlayerInfoResponse(url));
        }

        public IEnumerator LoadPlayerInfoResponse(string uri)
        {
            Debug.Log("Nomal-LoadPlayerInfo-Response");

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                string[] pages = uri.Split('/');
                int page = pages.Length - 1;


                // 로딩 패널 종료
                Loading = false;
                loadingPanel.SetActive(Loading);

                if (webRequest.isNetworkError)
                {
                    // 알림창
                    AlertError = true;
                    errorAlertPanel_Text.text = "서버에 접속할 수 없습니다.";
                    errorAlertPanel.SetActive(AlertError);

                    //Debug.Log(pages[page] + ": Error: " + webRequest.error);
                }
                else
                {
                    AccountInfoResponce responseData = JsonConvert.DeserializeObject<AccountInfoResponce>(webRequest.downloadHandler.text);

                    Debug.Log("Response => " + responseData.Status);

                    if (responseData.Status == CommonError.OK)
                    {
                        // 시퀸스 에러시 종료
                        if (ExitManager.DisplayBySeq(responseData.Sequence))
                        {
                            Application.Quit();
                        }

                        var acc = UserInfoManager.AccountInfo;
                        acc.Cash = responseData.Cash;
                        acc.EXP = responseData.EXP;
                        acc.LV = responseData.LV;
                        acc.Bonus = responseData.Bonus;

                    }
                    else
                    {
                        // TODO:: 실패 화면 표시
                        Loading = false;
                        SequenceManger.CallFail();
                    }
                }
            }
        }
#endregion


        void Update()
        {
            loadingPanel.SetActive(Loading);

            if (Input.GetKey(KeyCode.Escape))
            {
                ExitPanel.SetActive(true);
            }
        }

        public void KillApplication()
        {
            Application.Quit();
        }

        private string GetRealIP(bool supportIPv6 = false)
        {
            if (supportIPv6 && !Socket.OSSupportsIPv6)
                return null;

            string output = "";

            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {

                NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
                NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

                if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        output = ip.Address.ToString();
                    }
                }
            }

            return output;
        }
    }
}
