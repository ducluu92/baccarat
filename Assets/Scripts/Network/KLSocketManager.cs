using Assets.Scripts.Baccarrat;
using Assets.Scripts.Network.Bakaras;
using Module.Packets;
using Module.Packets.Animator;
using Module.Packets.Bakara;
using Module.Packets.Betting;
using Module.Packets.Chat;
using Module.Packets.Event;
using Module.Packets.Login;
using Module.Packets.Networks;
using Module.Packets.Room;
using Module.Rooms.Animations;
using Module.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Assets.Scripts.Network;
using System.Threading;
using Assets.Scripts.Baccarrat.Battings;
using Module.Packets.Definitions;
using Module.Utils.Times;
using Assets.Scripts.Loggers;
using UnityEngine.SocialPlatforms;
using Assets.Scripts.Baccarrat.InGames;
using Module.Utils.Currency;
using DG.Tweening;
using Assets.Scripts.Network.ServerTimes;
using Assets.Scripts.Settings;
using UnityEngine.Analytics;
using System.Runtime.CompilerServices;

public class KLSocketManager : MonoBehaviour
{
    private UnityEngine.Object[] Cardsprites;

    public string hostIP = "localhost";
    public int hostPort = 16467;
    public double DropTime = 1500.0;

    private Client client = null;

    public Animator animatorController;

    public GameObject bettingTimer;
    public ProgressBarController bettingTimerController;

    private Queue<Transaction> transactionQueue = null;

    private bool isObserving = true;

    public GameObject[] PlayerCard = new GameObject[3];
    public GameObject[] BankerCard = new GameObject[3];

    public GameObject RoundLoading;

    public ScrollRect ChattingScrollRect;
    public InputField messageInputBox;
    public GameObject messageArea;
    public GameObject messageItem;

    public Room CurrentRoom;

    public GameObject Panel_DropByElapseTime;
    public GameObject Panel_DropByApplicationDIE;

    public Image MaxButton;
    public Sprite[] MaxSprites = new Sprite[2];
    public GameObject MaxRing;

    //유저 슬롯
    public UserData_Play Player = null;
    public BaccaratPlayer baccaratplayer = null;
    public BETTINGTYPE Player_bettype = BETTINGTYPE.NONE;
    public UserData_Play[] Users = new UserData_Play[7];

    public BaccaratPlayer[] BaccaratUsers = new BaccaratPlayer[7];

    public Text Text_MinMax;

    // 총 배팅 금액
    public int BettingAmountTotalBanker = 0;
    public int BettingAmountTotalPlayer = 0;
    public int BettingAmountTotalBP = 0;
    public int BettingAmountTotalPP = 0;
    public int BettingAmountTotalTie = 0;

    // 알림 관련 UI
    public GameObject Obj_NoticeAlert;
    public Text Text_NoticeAlert;

    // 로딩 UI
    public GameObject Obj_LoadingPanel;
    public GameObject Obj_LoadingPanel_SubText;
    public GameObject Obj_LoadingPanel_SubImage;

    // 이전 배팅 정보
    bool isBatting = false;
    bool isInObserve = false;

    // 룸 배팅 정보 로딩
    bool isBettingRoom = false;

    // 어플리케이션 Background 판단 여부 
    bool IsAppPaused = false;

    // 타임 체커 
    public ServerTimeBehaviour serverTimer;

    private int decompressedCount = 0;

    private bool gotoLogin = false;

    public enum KLBettingType : int
    {
        NONE = 0,
        PPAIR = 1,
        PLAYER = 2,
        TIE = 3,
        BANKER = 4,
        BPAIR = 5
    };


    // Start is called before the first frame update
    void Start()
    {
        gotoLogin = false;

        // 접속정보 가져오기
        if (!Assets.Scripts.Settings.AppSettingManager.GetSocket(out hostIP, out hostPort)) 
        {
            return;
        }
        
        Cardsprites = Resources.LoadAll<Sprite>("Sprites/Sprites_Card");
        transactionQueue = new Queue<Transaction>();

        // 클라이언트 소켓 생성 
        RoomDataManager.Instance();

        client = new Client(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), SortationType.PacketSize);
        client.clntSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 5000);
        client.clntSock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 5000);

        if (AppSettingManager.GetEnv() == EnvironmentType.Development)
        {
            Obj_LoadingPanel.SetActive(false);
        }

        try
        {
            // 소켓 연결
            if (client.clntSock != null)
            {
                client.clntSock.Connect(hostIP, hostPort);

                client.clntSock.BeginReceive(client.Buffer, 0, SortationType.PacketSize, 0, DataReceiveCallBack, client);

                StartCoroutine(SendInfoCo());
            }
        }
        catch (ObjectDisposedException e) 
        {
            Debug.Log($"Socket Disposed Error => {e.Message}");
            return;
        }
        catch (SocketException socketError)
        {
            Debug.Log($"Socket Error => {socketError.Message}");
            return;
        }
        catch (Exception e)
        {
            Debug.Log($"Socket Error => {e.Message}");
        }

        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (!(AppSettingManager.GetEnv() == EnvironmentType.Development))
            StartCoroutine(Co_ClientReceivedDataCheck(10.0f));
    }

    IEnumerator Co_ClientReceivedDataCheck(float waittime)
    {
        yield return new WaitForSeconds(waittime);

        if (!isBettingRoom)
        {
            SceneChanger.CallSceneLoader("Lobby");
        }
    }

    void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            IsAppPaused = true;
        }
        else
        {
            if(IsAppPaused)
            {
                IsAppPaused = false;
                Panel_DropByApplicationDIE.SetActive(true);
            }
        }
    }

    public IEnumerator SendInfoCo()
    {
        // 연결 되면 플레이어 정보를 보낸다..
        int count = 0;
        while (
            !SendPlayerData(UserInfoManager.loginInfo.UUID, UserInfoManager.RoomIdx, UserInfoManager.AccountInfo.NicName, UserInfoManager.AccountInfo.Cash, 0, 0)
            && count < 10)
        {
            count++;
            yield return new WaitForSeconds(0.125f);
        }

        // 정보가 보내지면 바로 옵저버 등록을 한다.
        JoinObserve();
    }

    int joinCounter = 0;

    void Update()
    {
        if ((transactionQueue != null) && (transactionQueue.Count > 0))
        {
            Transaction tx = transactionQueue.Dequeue();
            var hex = tx.Data.type.ToString("X");

            var managerInterceptor = RoomDataManager.Instance();
            var bagInstance = InGameBag.Instance;

            // 지연율
            var bef = TimeStamper.LongToDateTime(tx.Data.timestamp);
            //Debug.Log($"Tx Recieved => Type: 0x{ hex },  Time :{bef}"); 

            switch (tx.Data.type)
            {
                #region ping/pong
                case 0x0001:
                    PingPongPacket ppp = (PingPongPacket)tx.Data;
                    if (ppp.pingorpong.Equals("ping"))
                    {
                        // 지연시간을 구하기 위해서 생성
                        var packet = new PingPongPacket("pong");
                        packet.timestamp = ppp.timestamp;

                        if (!client.SendPacket(packet, out var error))
                        {
                            Debug.Log(error);
                        }
                    }
                    break;
                #endregion
                #region ping/pong
                case 0x1001:
                    ChattingPacket chatPack = (ChattingPacket)tx.Data;
                    //Debug.Log("Chat Data=> " + chatPack.nickname + ": " + chatPack.message);
                    var msgObj = Instantiate(messageItem);
                    Text changeMsg = msgObj.transform.GetChild(0).gameObject.GetComponent<Text>();
                    changeMsg.text = chatPack.nickname + ": " + chatPack.message;
                    msgObj.transform.SetParent(messageArea.transform, false);
                    StartCoroutine(SetScrollRect());
                    break;
                #endregion
                #region room in/out
                case 0x4001:
                    SystemMessage("자리에 앉기 성공");

                    RoomInOut roomInOutInfo = (RoomInOut)tx.Data;
                    var ruser = Users[roomInOutInfo.selectedIdx];
                    var baccaratruser = ruser.gameObject.GetComponent<BaccaratPlayer>();

                    if (roomInOutInfo.isEnter)
                    {
                        if (roomInOutInfo.IsSuccess)
                        {
                            // mark - uuid 추가.
                            baccaratruser.CurrentBetting = Convert.ToInt32(roomInOutInfo.cash);

                            Users[roomInOutInfo.selectedIdx].uuid = roomInOutInfo.uuid;
                            Users[roomInOutInfo.selectedIdx].Nickname = roomInOutInfo.nickname;
                            Users[roomInOutInfo.selectedIdx].ProfileID = 1;
                            Users[roomInOutInfo.selectedIdx].CurrentChips = roomInOutInfo.cash;
                            Users[roomInOutInfo.selectedIdx].UpdateDisplays();

                            if (roomInOutInfo.nickname.Equals(UserInfoManager.AccountInfo.NicName))
                            {
                                Player = Users[roomInOutInfo.selectedIdx];
                                Player.ChangePlayerBorder(new Color((234.0f/255.0f), (52.0f/255.0f), 0.0f));
                                baccaratplayer = Users[roomInOutInfo.selectedIdx].gameObject.GetComponent<BaccaratPlayer>();
                                baccaratplayer.isPlayer = true;
                            }
                        }
                        else 
                        {
                            if (joinCounter < 10)
                            {
                                JoinGame(roomInOutInfo.selectedIdx + 1);
                            }
                            else 
                            {
                                SystemMessage($"룸 입장실패입니다. 룸에 다시 입장해주세요. (개발중...)");
                            }

                            joinCounter++;
                        }
                    }
                    else
                    {
                        baccaratruser.RevertAll();
                        baccaratruser.Clear();

                        Users[roomInOutInfo.selectedIdx].ProfileID = 0;
                        Users[roomInOutInfo.selectedIdx].UpdateDisplays();
                        //Users[roomInOutInfo.selectedIdx].Betting_Bet_Revert();
                        Users[roomInOutInfo.selectedIdx].PlayerBettingSignalClear();
                    }
                    break;
                #endregion
                #region Bakara
                #region AnimationUpdate
                case 0x3001:
                    if (managerInterceptor.isLoading)
                        return;
                    AnimationUpdate animUpdate = (AnimationUpdate)tx.Data;
                    ChangeAnimation(animUpdate.anim);
                    break;
                #endregion
                #region BakaraGameData
                case 0x3002:
                    if (managerInterceptor.isLoading)
                        return;

                    BakaraGameData bakaraData = (BakaraGameData)tx.Data;

                    var manager = RoomDataManager.Instance();

                    manager.Reset();

                    if (bakaraData.data.Winner != 0)
                    {
                        manager.winner = new BakaraWinner();
                        manager.winner = (BakaraWinner)bakaraData.data.Winner;

                        manager.isBankerPair = bakaraData.data.BankerPair;
                        manager.isPlayerPair = bakaraData.data.PlayerPair;

                        manager.isPlayerNatural = bakaraData.data.IsPlayerNatual;
                        manager.isBankerNatural = bakaraData.data.IsBankerNatual;

                        manager.CardValue_Player[0] = bakaraData.data.Cards[0].Value;
                        manager.CardValue_Banker[0] = bakaraData.data.Cards[1].Value;
                        manager.CardValue_Player[1] = bakaraData.data.Cards[2].Value;
                        manager.CardValue_Banker[1] = bakaraData.data.Cards[3].Value;

                        // RandomSeed 추가 
                        manager.RandomSeed = bakaraData.data.RandomSeed;
                        manager.RandomSubSeed = bakaraData.data.RandomSubSeed;

                        if (bakaraData.data.Cards.Count == 5)
                        {
                            if (bakaraData.data.Cards[4].Picker == 0) // Player 
                            {
                                manager.CardValue_Player[2] = bakaraData.data.Cards[4].Value;
                            }
                            else // 1 - Banker
                            {
                                manager.CardValue_Banker[2] = bakaraData.data.Cards[4].Value;
                            }
                        }
                        else if (bakaraData.data.Cards.Count == 6)
                        {
                            if (bakaraData.data.Cards[4].Picker == 0)
                                manager.CardValue_Player[2] = bakaraData.data.Cards[4].Value;
                            else if (bakaraData.data.Cards[4].Picker == 1)
                                manager.CardValue_Banker[2] = bakaraData.data.Cards[4].Value;

                            if (bakaraData.data.Cards[5].Picker == 0)
                                manager.CardValue_Player[2] = bakaraData.data.Cards[5].Value;
                            else if (bakaraData.data.Cards[5].Picker == 1)
                                manager.CardValue_Banker[2] = bakaraData.data.Cards[5].Value;
                        }


                        manager.CardID_Player[0] = bakaraData.data.Cards[0].ImgID + 1;
                        manager.CardID_Banker[0] = bakaraData.data.Cards[1].ImgID + 1;
                        manager.CardID_Player[1] = bakaraData.data.Cards[2].ImgID + 1;
                        manager.CardID_Banker[1] = bakaraData.data.Cards[3].ImgID + 1;

                        if (bakaraData.data.Cards.Count == 5)
                        {
                            if (bakaraData.data.Cards[4].Picker == 0)
                                manager.CardID_Player[2] = bakaraData.data.Cards[4].ImgID + 1;
                            else if (bakaraData.data.Cards[4].Picker == 1)
                                manager.CardID_Banker[2] = bakaraData.data.Cards[4].ImgID + 1;
                        }
                        else if (bakaraData.data.Cards.Count == 6)
                        {
                            if (bakaraData.data.Cards[4].Picker == 0)
                                manager.CardID_Player[2] = bakaraData.data.Cards[4].ImgID + 1;
                            else if (bakaraData.data.Cards[4].Picker == 1)
                                manager.CardID_Banker[2] = bakaraData.data.Cards[4].ImgID + 1;

                            if (bakaraData.data.Cards[5].Picker == 0)
                                manager.CardID_Player[2] = bakaraData.data.Cards[5].ImgID + 1;
                            else if (bakaraData.data.Cards[5].Picker == 1)
                                manager.CardID_Banker[2] = bakaraData.data.Cards[5].ImgID + 1;
                        }


                        manager.showDatas();

                        animatorController.SetTrigger("DataReceived");
                    }

                    break;
                #endregion
                #region BakaraGameRoomData
                case 0x3003:
                    break;
                #endregion
                #region BettingAnimStart
                case 0x3004:
                    var bas = (BettingAnimStart)tx.Data;
                    RoomDataManager.Instance().BettingTime = bas.bettingTime;
                    ChangeAnimation(AnimationStatus.BattingTime);
                    break;
                #endregion

                case SortationType.RoomPlayerInfos:
                    if (isBettingRoom) break;
                    SystemMessage("전체 룸 데이터 가져오기 성공");
                    RoomPlayerInfosResponce((RoomPlayerInfos)tx.Data);
                    break;
                #endregion

                case SortationType.RoomObserveCheck:
                    SystemMessage("옵저버 입장 성공");
                    ObserverJoinResponce((RoomObserveCheck)tx.Data);
                    break;
                case SortationType.MaxBattingRes:
                    if(!managerInterceptor.isLoading)
                        MaxBattingResponce((MaxBattingRes)tx.Data);
                    break;
                case SortationType.BattingResPlayer:
                    if (!managerInterceptor.isLoading)
                        BattingResponce((ResPlayerBetting)tx.Data);
                    break;
                case SortationType.BattingSpecial:
                    if (!managerInterceptor.isLoading)
                        BattingSpecialResponce((SpecialBatting)tx.Data);
                    break;
                case SortationType.PingOut:
                    PingOut();
                    break;
                case SortationType.BattingClear:
                    if (!managerInterceptor.isLoading)
                        BattingClearResponce((ClearPlayerBetting)tx.Data);
                    break;
                case SortationType.Emoticon:
                    if (!managerInterceptor.isLoading)
                        EmoticonResponse((Emoticon)tx.Data);
                    break;
                default:
                    Debug.Log($"Invalid Packet {hex}");
                    break;
            }
            
        }
    }

    private void RoomPlayerInfosResponce(RoomPlayerInfos playerInfos)
    {
        try
        {
            var managerInterceptor = RoomDataManager.Instance();
            var bagInstance = InGameBag.Instance;

            managerInterceptor.isLoading = true;
            bagInstance.IsMiddlePosition = true;

            if (playerInfos.playerList != null)
            {
                foreach (var rp in playerInfos.playerList)
                {
                    if (!rp.Nickname.Equals(UserInfoManager.AccountInfo.NicName))
                    {
                        if (rp.SelectedIdx >= 0)
                        {
                            var rpu = Users[rp.SelectedIdx];
                            var baccaratrpu = rpu.gameObject.GetComponent<BaccaratPlayer>();

                            Users[rp.SelectedIdx].uuid = rp.UUID;
                            Users[rp.SelectedIdx].Nickname = rp.Nickname;
                            Users[rp.SelectedIdx].ProfileID = rp.ProfileID;
                            Users[rp.SelectedIdx].CurrentBettings = rp.CurrentBettings;


                            if (Users[rp.SelectedIdx].CurrentBettings > 0)
                                Users[rp.SelectedIdx].CurrentChips = rp.CurrentChips - rp.CurrentBettings;
                            else
                                Users[rp.SelectedIdx].CurrentChips = rp.CurrentChips;

                            baccaratrpu.CurrentBetting = Convert.ToInt32(rp.CurrentChips);
                            baccaratrpu.CurrentBettingTotal = Convert.ToInt32(Users[rp.SelectedIdx].CurrentBettings);

                            Users[rp.SelectedIdx].UpdateDisplays();
                        }
                    }
                }
            }


            if (playerInfos.RobotInfoList != null)
            {
                foreach (var robot in playerInfos.RobotInfoList)
                {
                    if (robot.Seat >= 0)
                    {
                        var botu = Users[robot.Seat];
                        var baccaratbot = botu.gameObject.GetComponent<BaccaratPlayer>();

                        Users[robot.Seat].Nickname = robot.Nicname;
                        Users[robot.Seat].ProfileID = 1;
                        Users[robot.Seat].CurrentBettings = 0;
                        Users[robot.Seat].CurrentChips = robot.Cash;
                        baccaratbot.CurrentBetting = Convert.ToInt32(robot.Cash);
                        baccaratbot.CurrentBettingTotal = 0;
                        Users[robot.Seat].uuid = robot.ID;

                        Users[robot.Seat].UpdateDisplays();
                    }
                }
            }

            managerInterceptor.Reset();

            managerInterceptor.MinBetting = playerInfos.MinBetting;
            managerInterceptor.MaxBetting = playerInfos.MaxBetting;


            Text_MinMax.text = "Min/Max " + CurrencyConverter.Kor(playerInfos.MinBetting) + " - " + CurrencyConverter.Kor(playerInfos.MaxBetting);

            if (playerInfos.data.Winner != 0)
            {
                managerInterceptor.showDatas();

                managerInterceptor.winner = new BakaraWinner();
                managerInterceptor.winner = (BakaraWinner)playerInfos.data.Winner;

                managerInterceptor.isBankerPair = playerInfos.data.BankerPair;
                managerInterceptor.isPlayerPair = playerInfos.data.PlayerPair;

                managerInterceptor.isPlayerNatural = playerInfos.data.IsPlayerNatual;
                managerInterceptor.isBankerNatural = playerInfos.data.IsBankerNatual;

                managerInterceptor.CardValue_Player[0] = playerInfos.data.Cards[0].Value;
                managerInterceptor.CardValue_Banker[0] = playerInfos.data.Cards[1].Value;
                managerInterceptor.CardValue_Player[1] = playerInfos.data.Cards[2].Value;
                managerInterceptor.CardValue_Banker[1] = playerInfos.data.Cards[3].Value;

                if (playerInfos.data.Cards.Count == 5)
                {
                    if (playerInfos.data.Cards[4].Picker == 0) // Player 
                    {
                        managerInterceptor.CardValue_Player[2] = playerInfos.data.Cards[4].Value;
                    }
                    else // 1 - Banker
                    {
                        managerInterceptor.CardValue_Banker[2] = playerInfos.data.Cards[4].Value;
                    }
                }
                else if (playerInfos.data.Cards.Count == 6)
                {
                    if (playerInfos.data.Cards[4].Picker == 0)
                        managerInterceptor.CardValue_Player[2] = playerInfos.data.Cards[4].Value;
                    else if (playerInfos.data.Cards[4].Picker == 1)
                        managerInterceptor.CardValue_Banker[2] = playerInfos.data.Cards[4].Value;

                    if (playerInfos.data.Cards[5].Picker == 0)
                        managerInterceptor.CardValue_Player[2] = playerInfos.data.Cards[5].Value;
                    else if (playerInfos.data.Cards[5].Picker == 1)
                        managerInterceptor.CardValue_Banker[2] = playerInfos.data.Cards[5].Value;
                }


                managerInterceptor.CardID_Player[0] = playerInfos.data.Cards[0].ImgID + 1;
                managerInterceptor.CardID_Banker[0] = playerInfos.data.Cards[1].ImgID + 1;
                managerInterceptor.CardID_Player[1] = playerInfos.data.Cards[2].ImgID + 1;
                managerInterceptor.CardID_Banker[1] = playerInfos.data.Cards[3].ImgID + 1;

                PlayerCard[0].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[0].ImgID + 1];
                PlayerCard[1].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[2].ImgID + 1];
                BankerCard[0].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[1].ImgID + 1];
                BankerCard[1].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[3].ImgID + 1];

                if (playerInfos.data.Cards.Count == 5)
                {
                    if (playerInfos.data.Cards[4].Picker == 0)
                    {
                        managerInterceptor.CardID_Player[2] = playerInfos.data.Cards[4].ImgID + 1;
                        PlayerCard[2].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[4].ImgID + 1];
                    }
                    else if (playerInfos.data.Cards[4].Picker == 1)
                    {
                        managerInterceptor.CardID_Banker[2] = playerInfos.data.Cards[4].ImgID + 1;
                        BankerCard[2].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[4].ImgID + 1];
                    }
                }
                else if (playerInfos.data.Cards.Count == 6)
                {
                    if (playerInfos.data.Cards[4].Picker == 0)
                    {
                        managerInterceptor.CardID_Player[2] = playerInfos.data.Cards[4].ImgID + 1;
                        PlayerCard[2].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[4].ImgID + 1];
                    }
                    else if (playerInfos.data.Cards[4].Picker == 1)
                    {
                        managerInterceptor.CardID_Banker[2] = playerInfos.data.Cards[4].ImgID + 1;
                        BankerCard[2].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[4].ImgID + 1];
                    }
                    if (playerInfos.data.Cards[5].Picker == 0)
                    {
                        managerInterceptor.CardID_Player[2] = playerInfos.data.Cards[5].ImgID + 1;
                        PlayerCard[2].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[5].ImgID + 1];
                    }
                    else if (playerInfos.data.Cards[5].Picker == 1)
                    {
                        managerInterceptor.CardID_Banker[2] = playerInfos.data.Cards[5].ImgID + 1;
                        BankerCard[2].GetComponent<SpriteRenderer>().sprite = (Sprite)Cardsprites[playerInfos.data.Cards[5].ImgID + 1];
                    }
                }

                foreach (PlayerBetting pbValue in playerInfos.BettingInfoList)
                {
                    // 배팅 애니메이션 반영
                    if (pbValue.SelectedIdx >= 0)
                    {
                        var user = FindUserData(pbValue.UUID); // Users[pbValue.selectedIdx];

                        if (user != null)
                        {
                            // 결과창 업데이트
                            StartCoroutine(ShowInterceptorBettingAnim(user, pbValue));
                        }
                    }
                }

                managerInterceptor.showDatas();

            }
            SetAnimation(playerInfos.anim, playerInfos.currentBettingTime);
            managerInterceptor.isLoading = false;

            bagInstance.IsBettingOK = true;
            bagInstance.IsSpecialBettingOK = true;

            SystemMessage("방 입장을 환영합니다 !");
            StartCoroutine(Co_ObjLoadingPanelClose());
            isBettingRoom = true;
        }
        catch(ArgumentOutOfRangeException)
        {
            Debug.Log("client network status is unstable. client going to lobby.");
            LeaveGame();
        }
        catch
        {
            Debug.Log("Unkown network error. client going to lobby.");
            LeaveGame();
        }
    }

    private IEnumerator Co_ObjLoadingPanelClose()
    {
        yield return null;

        Obj_LoadingPanel.GetComponent<Image>().DOFade(0.0f, 2.5f).SetEase(Ease.InExpo);
        Obj_LoadingPanel_SubText.GetComponent<Text>().DOFade(0.0f, 2.5f).SetEase(Ease.InExpo);
        Obj_LoadingPanel_SubImage.GetComponent<Image>().DOFade(0.0f, 2.5f).SetEase(Ease.InExpo);

        yield return new WaitForSeconds(2.5f);
        Obj_LoadingPanel.SetActive(false);
    }

    public void ClearAllSignalList()
    {
        for(int i=0;i<7;i++)
        {
            if (Users[i] != null)
                Users[i].PlayerBettingSignalClear();
        }
    }

    private void EmoticonResponse(Emoticon packet)
    {
        var uuid = UserInfoManager.loginInfo.UUID;

        if (packet.UUID == uuid)
        {
            // 내가 보낸 이모티콘
            Player.ShowEmoticon(packet.EmoticonID);
        }
        else
        {
            // 다른 사람의 이모티콘
            Users[packet.SelectedIdx].ShowEmoticon(packet.EmoticonID);
        }
    }

    private void PingOut()
    {
        Time.timeScale = 0.0f;
        Panel_DropByElapseTime.SetActive(true);
    }

    public void ObserverJoinResponce(RoomObserveCheck packet)
    {
        switch (packet.ConnetionType)
        {
            case BakaraConnetionType.OK:   // 성공
                isInObserve = true;
                break;
            default:
                // TODO : 재시도 요청.
                JoinObserve();
                break;
        }
    }

    public IEnumerator SetScrollRect()
    {
        yield return new WaitForSeconds(0.1f);
        ChattingScrollRect.verticalNormalizedPosition = 0.0f;
    }

    /// <summary>
    /// 중도입장 시의 애니메이션 변경
    /// </summary>
    /// <param name="animStatus">변경할 애니메이션 상태</param>
    /// <param name="bettingTime">현재 베팅 시간</param>
    private void SetAnimation(AnimationStatus animStatus, int bettingTime)
    {
        switch (animStatus)
        {
            case AnimationStatus.BattingTime:
                Player_bettype = BETTINGTYPE.NONE;
                
                if (bettingTime >= 3)
                {
                    bettingTimerController.StartTimer(bettingTime);

                    animatorController.SetTrigger("DrawCard");
                    animatorController.Play("Network_Init_Temp");

                    isBatting = true;
                }
                else
                {
                    CurrentRoom.DisableButtons();
                    animatorController.Play("UI_Seq_Betting_Check");
                }
                break;
            case AnimationStatus.ShowDown:
                Player_bettype = BETTINGTYPE.NONE;
                isBatting = false;
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_ShowDown");
                break;
            case AnimationStatus.LastCardDraw:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_LastGame"); 
                break;
            case AnimationStatus.Seq_DrawCard1:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_DrawCard1");
                break;
            case AnimationStatus.Seq_FlipCard1:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_FlipCard1");
                break;
            case AnimationStatus.Seq_FlipCard_Player_sub_rotation:
            case AnimationStatus.Seq_FlipCard_Player_sub_nonrotation:
            case AnimationStatus.Seq_FlipCard_Player_sub_upsidedown:
            case AnimationStatus.Seq_FlipCard_Player_sub_righttodown:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_FlipCard_PlayerAfterSub");
                break;
            case AnimationStatus.Seq_FlipCard_Player_Natural:
            case AnimationStatus.Seq_PlayerPair:
            case AnimationStatus.Seq_FlipCard_Banker_sub_rotation:
            case AnimationStatus.Seq_FlipCard_Banker_sub_nonrotation:
            case AnimationStatus.Seq_FlipCard_Banker_sub_upsidedown:
            case AnimationStatus.Seq_FlipCard_Banker_sub_righttodown:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_FlipCard_BankerAfterSub");
                break;
            case AnimationStatus.Seq_FlipCard_Banker_Natural:
            case AnimationStatus.Seq_BankerPair:
            case AnimationStatus.Seq_PlayerSub2:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_Player_FlipCard2_Network");
                break;
            case AnimationStatus.Seq_BankerSub2:
                CurrentRoom.DisableButtons();
                animatorController.Play("Seq_Banker_FlipCard2_Network");
                break;
            case AnimationStatus.PlayerWin:
            case AnimationStatus.BankerWin:
            case AnimationStatus.TieWin:
                CurrentRoom.DisableButtons();
                animatorController.Play("Network_Init_Temp");
                break;
            case AnimationStatus.SuffleTime:
                CurrentRoom.DisableButtons();
                animatorController.SetTrigger("SuffleTime");
                animatorController.Play("Network_Init_Temp");
                break;
        }
    }

    /// <summary>
    /// 중도입장이 아닐경우의 애니메이션 전환
    /// </summary>
    /// <param name="animStatus">변경할 애니메이션 상태</param>
    private void ChangeAnimation(AnimationStatus animStatus)
    {
        bool trigger = false;

        switch (animStatus)
        {
            case AnimationStatus.Init:
                Player_bettype = BETTINGTYPE.NONE;
                if(CurrentRoom.IsLoadingPanel)
                {
                    Obj_LoadingPanel.GetComponent<Image>().DOFade(1.0f, 0.0f);
                    Obj_LoadingPanel_SubImage.GetComponent<Image>().DOFade(1.0f, 0.0f);
                    Obj_LoadingPanel_SubText.GetComponent<Text>().DOFade(1.0f, 0.0f);

                    Obj_LoadingPanel.SetActive(true);

                    StartCoroutine(Co_LoadingPanelClosed(0.8f));
                }
                animatorController.Play("Seq_FlipCard_gotoidle");
                break;
            case AnimationStatus.BattingTime:
                //animatorController.Play("Network_Init_Temp");
                Player_bettype = BETTINGTYPE.NONE;

                bettingTimerController.StartTimer(RoomDataManager.Instance().BettingTime);
                animatorController.SetTrigger("DrawCard");

                // Mark - 이후 오류 해결
                isBatting = true;
                break;
            case AnimationStatus.ShowDown:
                // Mark - 이후 오류 해결
                Player_bettype = BETTINGTYPE.NONE;
                isBatting = false;

                // 배팅 기록 넘기기
                var bag = BattingBag.Instance;
                bag.Next();
 
                // Origin
                animatorController.SetTrigger("ShowDown");
                break;
            case AnimationStatus.LastCardDraw:
                // Mark - 이후 오류 해결
                Player_bettype = BETTINGTYPE.NONE;
                isBatting = false;

                // 배팅 기록 넘기기
                var bag2 = BattingBag.Instance;
                bag2.Next();

                animatorController.SetTrigger("LastGame");
                break;
            case AnimationStatus.SuffleTime:
                animatorController.SetTrigger("SuffleTime");
                break;
        }

        if (trigger)
        {
            trigger = false;
            animatorController.SetTrigger("StandBy");
        }
    }

    private IEnumerator Co_LoadingPanelClosed(float delay)
    {
        Obj_LoadingPanel.GetComponent<Image>().DOFade(0.0f, delay + 0.4f).SetEase(Ease.InExpo);
        Obj_LoadingPanel_SubText.GetComponent<Text>().DOFade(0.0f, delay + 0.4f).SetEase(Ease.InExpo);
        Obj_LoadingPanel_SubImage.GetComponent<Image>().DOFade(0.0f, delay + 0.4f).SetEase(Ease.InExpo);
        yield return new WaitForSeconds(delay + 0.4f);
        Obj_LoadingPanel.SetActive(false);
    }

    private void DataReceiveCallBack(IAsyncResult result)
    {
        Client clnt = (Client)result.AsyncState;
        Packet packet;

        if (!IsConnected(clnt.clntSock)) 
        {
            try {
                clnt.Clear();
                clnt.Close();
            }
            catch { }

            if(!gotoLogin)
                SceneChanger.CallSceneLoader("Lobby");

            return;
        }

        var decompressed = Compressor.Decompress(clnt.Buffer);
        if (decompressed == null)
        {
            Debug.Log($"Decompressed Error => {clnt.Buffer}");
            decompressedCount++;
            if(decompressedCount >= 30)
            {
                try
                {
                    clnt.Clear();
                    clnt.Close();
                }
                catch { }

                Debug.Log("Heererer wer qwer qwerq e!@~ #~!@# !@#!@#! @ LLOOOOOOBBBBYYYYY 2222222");
                SceneChanger.CallSceneLoader("Lobby");

            }
            return;
        }
        else
        {
            try
            {
                var arr = decompressed.ToArray();

                packet = (Packet)Convertor.ByteArrayToObject(arr);

                if (AppSettingManager.GetEnv() == EnvironmentType.Development)
                {
                    Debug.Log("Packet Time => " + serverTimer.TimeSpanMilies(TimeStamper.LongToDateTime(packet.timestamp)));        
                }
            }
            catch (Exception e)
            {
                Debug.Log("Error => " + e.Message + ", " + e.StackTrace);
                return;
            }

            Transaction tx = new Transaction(clnt, packet);

            transactionQueue.Enqueue(new Transaction(clnt, packet));

            decompressedCount = 0;

            clnt.Clear();

            clnt.clntSock.BeginReceive(clnt.Buffer, 0, SortationType.PacketSize, 0, DataReceiveCallBack, clnt);
        }
    }

    public bool Close()
    {
        try
        {
            // TODO: 소켓 연결 해제 이전에 서버에 접속 종료 메시지를 전달한 뒤에 종료해야함 

            // 소켓 연결 해제
            if (client != null)
            {
                if (client.clntSock != null)
                {
                    client.Close();
                    client = null;
                }
            }
            return true;
        }
        catch (SocketException socketError)
        {
            return false;
        }
    }

    public void SendEmoticon(int index)
    {
        var LoginInfo = UserInfoManager.loginInfo;
        var packet = new Emoticon(LoginInfo.UUID, UserInfoManager.RoomIdx, UserInfoManager.SelectedIdx, index);

        if (client == null) 
        {
            Debug.Log("Client not exist");
            return;
        }

        if (!client.SendPacket(packet, out var error)) 
        {
            Debug.Log(error);
        }
    }

    public bool SendPlayerData(int UUID, int RoomIdx, string NickName, long CurrentChips, long CurrentBettings, int selectedIdx)
    {
        SystemMessage("플레이어 정보 전송");

        if (client == null)
        {
            Debug.Log("Client not exist");
            return false;
        }

        var packet = new PlayerInfoData(UUID, RoomIdx, NickName, CurrentChips, CurrentBettings, selectedIdx);
        if (!client.SendPacket(packet, out var error))
        {
            Debug.Log(error);
            return false;
        }

        return true;
    }

    public bool sendPacket(Packet packet)
    {
        if (client == null)
        {
            Debug.Log("Client not exist");
            return false;
        }

        if (!client.SendPacket(packet, out var error))
        {
            Debug.Log(error);
            return false;
        }

        return true;
    }

    public void JoinGame(int idx)
    {
        SystemMessage("자리에 앉기 시도");

        idx = idx - 1;
        
        if (isInObserve) 
        {
            var uuid = UserInfoManager.loginInfo.UUID;

            isObserving = false;
            UserInfoManager.isGamePlay = true;
            UserInfoManager.SelectedIdx = idx;

            var packet = new RoomInOut(UserInfoManager.RoomIdx, uuid, true, idx);

            if (client == null)
            {
                Debug.Log("Client not exist");
                return;
            }

            if (!client.SendPacket(packet, out var error))
            {
                // 실패 처리를 여기서 해줄것.
                SystemMessage($"패킷 보내기 실패, error=>{error}");

                // 로그 
                Debug.Log(error);
                return;
            }
        }
    }

    public void LeaveGame()
    {
        var bagInstance = InGameBag.Instance;

        if (baccaratplayer != null)
        {
            var playerTotalBettingAmount = baccaratplayer.BattingAmountBanker
                + baccaratplayer.BattingAmountBPair
                + baccaratplayer.BattingAmountPlayer
                + baccaratplayer.BattingAmountPPair
                + baccaratplayer.BattingAmountTie;

            if (playerTotalBettingAmount > 0)
            {
                if (!bettingTimerController.IsTimerEnabled())
                {
                    Alert("배팅한 상태에는 방을 나갈 수 없습니다.\r\n게임이 끝난 이후에 다시 시도해주세요 !", 1.5f);
                    return;
                }
            }

            CancleBetting();
        }

        if (!isObserving)
        {
            var uuid = UserInfoManager.loginInfo.UUID;
            var packet = new RoomInOut(UserInfoManager.RoomIdx, uuid, false, UserInfoManager.SelectedIdx);
            
            while (!client.SendPacket(packet, out var error))
            {
                Debug.Log(error);
            }
        }
        else 
        {
            var packet = new RoomObserveInOut(false);
            while (!client.SendPacket(packet, out var error))
            {
                Debug.Log(error);
            }
        }

        if (Close())
        {
            UserInfoManager.ClearToRoomInfo();
            SceneChanger.CallSceneLoader("Lobby");
        }
    }

    public void LeaveGameToLogin()
    {
        if (Close())
        {
            gotoLogin = true;
            UserInfoManager.Clear();
            SceneChanger.CallSceneLoader("Login");
        }
    }


    public void JoinObserve()
    {
        SystemMessage("관전 시도");

        if (client == null)
        {
            Debug.Log("Client not exist");
            return;
        }

     
        var packet = new RoomObserveInOut(true);

        for (int i = 0; i < 10; i++)
        {
            bool noError = false;

            Thread.Sleep(100);
            try
            {
                if (!client.SendPacket(packet, out var error))
                {
                    Debug.Log(error);
                }

                noError = true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }

            if (noError)
            {
                break;
            }
        }

    }

    public void LeaveObserve()
    {
        var packet = new RoomObserveInOut(false);

        if (client == null)
        {
            Debug.Log("Client not exist");
            return;
        }

        if (!client.SendPacket(packet, out var error))
        {
            // 실패 처리를 여기서 해줄것.

            // 로그 
            Debug.Log(error);
            return;
        }
    }


    public void SendChat()
    {
        if (messageInputBox.text.Length > 0)
        {
            var packet = new ChattingPacket(UserInfoManager.RoomIdx, messageInputBox.text, UserInfoManager.AccountInfo.NicName);
            
            if (client == null)
            {
                Debug.Log("Client not exist");
                return;
            }

            if (!client.SendPacket(packet, out var error))
            {
                // 실패 처리를 여기서 해줄것.

                // 로그 
                Debug.Log(error);
                return;
            }

            messageInputBox.text = "";
            messageInputBox.ActivateInputField();
            // 비활성화
            //messageInputBox.Select();
        }
    }


    #region Button - 배팅

    public void ChangeBetType(int type)
    {
        if (Player != null) 
        {
            Player_bettype = (BETTINGTYPE)type;
            Player.bettingtype = (BETTINGTYPE)type;
        }
    }


    public void PlayerBetting(int index)
    {
        var bagInstance = InGameBag.Instance;
        var managerInterceptor = RoomDataManager.Instance();

        if (baccaratplayer.CurrentBetting < managerInterceptor.MinBetting)
        {
            Alert(managerInterceptor.MinBetting + "원 이하는 배팅할 수 없습니다.\r\n충전 후 다시 접속해주세요 !", 1.5f);
            return;
        }

        if (!bagInstance.IsBettingOK)
        {
            Alert("현재 배팅할 수 없습니다.\r\n배팅 시간에 다시 배팅해주세요 !", 1.5f);
            return;
        }

        bagInstance.IsChipClickedNow = true;

        if (bagInstance.SelectedChip != ChipType.None)
        {
            int amount = 0;

            switch (bagInstance.SelectedChip)
            {
                case ChipType.ChipThousand:
                    if (bagInstance.IsMaxBetNow) return;
                    bagInstance.IsMaxBetNow = false;
                    amount = 1000;
                    break;
                case ChipType.Chip10Thousand:
                    if (bagInstance.IsMaxBetNow) return;
                    bagInstance.IsMaxBetNow = false;
                    amount = 10000;
                    break;
                case ChipType.Chip100Thousand:
                    if (bagInstance.IsMaxBetNow) return;
                    bagInstance.IsMaxBetNow = false;
                    amount = 100000;
                    break;
                case ChipType.ChipMillion:
                    if (bagInstance.IsMaxBetNow) return;
                    bagInstance.IsMaxBetNow = false;
                    amount = 1000000;
                    break;
                case ChipType.ChipMax:
                    if (bagInstance.IsMaxBetNow) return;
                    else bagInstance.IsMaxBetNow = true;
                    break;
            }

            if (UserInfoManager.SelectedIdx > -1)
            {
                // 맥스 배팅이 될 수 있는지에 대한 여부 체크
                if ((baccaratplayer.GetBettingAmountWithIndex(index) + amount) >= managerInterceptor.MaxBetting)
                {
                    bagInstance.IsMaxBetNow = true;
                }

                if (!bagInstance.IsMaxBetNow)
                {
                    switch (index)
                    {
                        case 0:
                            Player.bettingtype = BETTINGTYPE.PPAIR;
                            SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, amount, UserInfoManager.SelectedIdx, NetworkBettingType.PlayerPair));

                            break;
                        case 1:
                            Player.bettingtype = BETTINGTYPE.PLAYER;
                            SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, amount, UserInfoManager.SelectedIdx, NetworkBettingType.Player));
                            break;
                        case 2:
                            Player.bettingtype = BETTINGTYPE.TIE;
                            SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, amount, UserInfoManager.SelectedIdx, NetworkBettingType.Tie));
                            break;
                        case 3:
                            Player.bettingtype = BETTINGTYPE.BANKER;
                            SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, amount, UserInfoManager.SelectedIdx, NetworkBettingType.Banker));
                            break;
                        case 4:
                            Player.bettingtype = BETTINGTYPE.BPAIR;
                            SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, amount, UserInfoManager.SelectedIdx, NetworkBettingType.BankerPair));
                            break;
                    }
                }
                else
                {
                    switch (index)
                    {
                        case 0:
                            Player.bettingtype = BETTINGTYPE.PPAIR;
                            SendMaxBetPacket(new MaxBattingReq(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, UserInfoManager.SelectedIdx, NetworkBettingType.PlayerPair));
                            break;
                        case 1:
                            Player.bettingtype = BETTINGTYPE.PLAYER;
                            SendMaxBetPacket(new MaxBattingReq(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, UserInfoManager.SelectedIdx, NetworkBettingType.Player));
                            break;
                        case 2:
                            Player.bettingtype = BETTINGTYPE.TIE;
                            SendMaxBetPacket(new MaxBattingReq(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, UserInfoManager.SelectedIdx, NetworkBettingType.Tie));
                            break;
                        case 3:
                            Player.bettingtype = BETTINGTYPE.BANKER;
                            SendMaxBetPacket(new MaxBattingReq(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, UserInfoManager.SelectedIdx, NetworkBettingType.Banker));
                            break;
                        case 4:
                            Player.bettingtype = BETTINGTYPE.BPAIR;
                            SendMaxBetPacket(new MaxBattingReq(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, UserInfoManager.SelectedIdx, NetworkBettingType.BankerPair));
                            break;
                    }
                }
                StartCoroutine(Co_BettingOKtoTrue());
                StartCoroutine(Co_SpecialBettingOKtoTrue());
            }
        }
    }

    private IEnumerator Co_BettingOKtoTrue()
    {
        yield return new WaitForSeconds(0.2f);

        var bagInstance = InGameBag.Instance;
        bagInstance.IsBettingOK = true;
    }

    private IEnumerator Co_SpecialBettingOKtoTrue()
    {
        yield return new WaitForSeconds(0.2f);

        var bagInstance = InGameBag.Instance;
        bagInstance.IsSpecialBettingOK = true;
    }

    private void SendBettingPacket(AddPlayerBetting packet) 
    {
        if (client == null)
        {
            Debug.Log("Client not exist");
            return;
        }

        while (!client.SendPacket(packet, out var error))
        {
            // 로그 
            Debug.Log($"{packet.bettingType.ToString()} : " + error);
            return;
        }
    }

    private void SendMaxBetPacket(MaxBattingReq packet) 
    {
        if (client == null)
        {
            Debug.Log("Client not exist");
            return;
        }

        while (!client.SendPacket(packet, out var error))
        {
            // 로그 
            Debug.Log($"{packet.BattingType.ToString()} : " + error);
            return;
        }
    }

    private void UpdateBettingAmount(ResPlayerBetting data)
    {
        switch (data.bettingType)
        {
            case NetworkBettingType.Banker:
                BettingAmountTotalBanker += data.betting;
                break;
            case NetworkBettingType.Player:
                BettingAmountTotalPlayer += data.betting;
                break;
            case NetworkBettingType.BankerPair:
                BettingAmountTotalBP += data.betting;
                break;
            case NetworkBettingType.PlayerPair:
                BettingAmountTotalPP += data.betting;
                break;
            case NetworkBettingType.Tie:
                BettingAmountTotalTie += data.betting;
                break;
        }
    }

    private void UpdateBettingAmountUsingDatas(NetworkBettingType type, int betting)
    {
        switch (type)
        {
            case NetworkBettingType.Banker:
                BettingAmountTotalBanker += betting;
                break;
            case NetworkBettingType.Player:
                BettingAmountTotalPlayer += betting;
                break;
            case NetworkBettingType.BankerPair:
                BettingAmountTotalBP += betting;
                break;
            case NetworkBettingType.PlayerPair:
                BettingAmountTotalPP += betting;
                break;
            case NetworkBettingType.Tie:
                BettingAmountTotalTie += betting;
                break;
        }
    }

    public void BettingTotalClear()
    {
        BettingAmountTotalBanker = 0;
        BettingAmountTotalPlayer = 0;
        BettingAmountTotalBP = 0;
        BettingAmountTotalPP = 0;
        BettingAmountTotalTie = 0;
    }

    public IEnumerator ShowInterceptorBettingAnim(UserData_Play user, PlayerBetting pbValue)
    {
        yield return null;

        int cIdx = ToChipContainerType(pbValue.BattingType);
        user.SelectChipContainer(cIdx);
        
        user.Betting_Bet_withoutChip(pbValue.Betting, ConvertBettingType(pbValue.BattingType));
        yield return new WaitForSeconds(0.2f);

        user.PlayerBettingSignalCheck(ConvertNetworkBettingTypeToInt(pbValue.BattingType));
        yield return new WaitForSeconds(0.1f);

        UpdateBettingAmountUsingDatas(pbValue.BattingType, Convert.ToInt32(pbValue.Betting));
        yield return new WaitForSeconds(0.2f);

        for (int i = 1; i < 6; i++)
            CurrentRoom.UpdateBoard(i);
        yield return new WaitForSeconds(0.1f);
    }

    private void BattingResponce(ResPlayerBetting data)
    {
        var uuid = UserInfoManager.loginInfo.UUID;
        var bagInstance = InGameBag.Instance;

        if (data.uuid == uuid)
        {
            // 내 배팅인경우
            StartCoroutine(BattingByMy(data));

            bagInstance.IsChipClickedNow = false;
        }
        else
        {
            // 남의 배팅인경우
            StartCoroutine(BattingByOthers(data));
        }

        UpdateBettingAmount(data);
    }

    public IEnumerator BattingByMy(ResPlayerBetting data)
    {
        yield return null;

        if (data.error == BettingErrorType.OK) 
        {
            var a = Convert.ToInt64(data.betting);
            var b = ConvertBettingType(data.bettingType);
            
            //애니메이션 및 사운드 재생.
            baccaratplayer.changebettingamount(data.betting);
            baccaratplayer.changebettingtype((int)b);
            baccaratplayer.ExecuteBetting();
            baccaratplayer.UI_HightlightPanel[((int)b) -1].SetActive(true);


            // 신호등 체크
            Player.PlayerBettingSignalCheck(ConvertBettingTypeToInt(b));

            // 배팅 데이터 저장
            var bag = BattingBag.Instance;
            bag.Batting(b, a);
            CurrentRoom.UpdateBoard((int)b);
        }
        else if(data.error == BettingErrorType.OverUpperLimit)
        {
            Alert("보너스게임 당첨 상한가 이상은\r\n배팅할 수 없습니다 !!", 1.5f);
        }
    }

    public IEnumerator BattingByOthers(ResPlayerBetting data)
    {
        yield return null;

        if (data.error == BettingErrorType.OK)
        {
            var a = Convert.ToInt64(data.betting);
            var b = ConvertBettingType(data.bettingType);

            var user = FindUserData(data.uuid);
            
            if (user != null) 
            {
                var baccaratuser = user.gameObject.GetComponent<BaccaratPlayer>();

                baccaratuser.changebettingamount(data.betting);
                baccaratuser.changebettingtype((int)b);
                baccaratuser.ExecuteBetting();
                //user.Betting_Bet(a, b);

                // 신호등 체크
                user.PlayerBettingSignalCheck(ConvertBettingTypeToInt(b));

                CurrentRoom.UpdateBoard((int)b);
            }
        }
    }

    private UserData_Play FindUserData(int tUUID) 
    {
        foreach (var user in Users) 
        {
            if (user != null) 
            {
                if (user.uuid == tUUID) 
                {
                    return user;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 배팅 여러개 동시에 할때 애니메이션
    /// </summary>
    private IEnumerator BattingAnimation(List<BattingRecord> battings)
    {
        foreach (var b in battings)
        {
            switch (b.Type)
            {
                case BETTINGTYPE.PPAIR:
                    SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, Convert.ToInt32(b.Value), UserInfoManager.SelectedIdx, NetworkBettingType.PlayerPair));
                    break;
                case BETTINGTYPE.PLAYER:
                    SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, Convert.ToInt32(b.Value), UserInfoManager.SelectedIdx, NetworkBettingType.Player));
                    break;
                case BETTINGTYPE.TIE:
                    SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, Convert.ToInt32(b.Value), UserInfoManager.SelectedIdx, NetworkBettingType.Tie));
                    break;
                case BETTINGTYPE.BANKER:
                    SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, Convert.ToInt32(b.Value), UserInfoManager.SelectedIdx, NetworkBettingType.Banker));
                    break;
                case BETTINGTYPE.BPAIR:
                    SendBettingPacket(new AddPlayerBetting(UserInfoManager.RoomIdx, UserInfoManager.loginInfo.UUID, Convert.ToInt32(b.Value), UserInfoManager.SelectedIdx, NetworkBettingType.BankerPair));
                    break;
                case BETTINGTYPE.NONE: // 없으면 아무것도 하지않음
                    break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }


    public void ChangePlayerChipContainer(int type)
    {
        Player.SelectChipContainer(type);
    }

    public void PlayerBattingX2()
    {
        var bagInstance = InGameBag.Instance;

        // 배팅 가능한 상태인지 확인, 후 진행
        if (!isBatting)
        {
            Alert("현재 2배 배팅을 사용할 수 없습니다.\r\n잠시 후 다시 시도해주세요.", 1.5f);
            return;
        }

        if(!bagInstance.IsSpecialBettingOK)
        {
            Alert("현재 2배 배팅을 사용할 수 없습니다.\r\n잠시 후 다시 시도해주세요.", 1.5f);
            return;
        }

        var roomIdx = UserInfoManager.RoomIdx;
        var uuid = UserInfoManager.loginInfo.UUID;
        var selectedIdx = UserInfoManager.SelectedIdx;
        var packet = new SpecialBatting(CallType.Request, roomIdx, uuid, selectedIdx, SpecialBattingType.Double, new List<SingleBatting>());
        
        bagInstance.IsSpecialBettingOK = false;

        if (!client.SendPacket(packet, out var error))
        {
            Debug.Log(error);
        }

        StartCoroutine(Co_SpecialBettingOKtoTrue());
    }

    public void PlayerBattingBefore()
    {
        var bagInstance = InGameBag.Instance;

        // 배팅 가능한 상태인지 확인, 후 진행
        if (!isBatting)
        {
            Alert("현재 이전 배팅을 사용할 수 없습니다.\r\n잠시 후 다시 시도해주세요.",1.5f);
            return;
        }

        if (!bagInstance.IsSpecialBettingOK)
        {
            Alert("현재 이전 배팅을 사용할 수 없습니다.\r\n잠시 후 다시 시도해주세요.", 1.5f);
            return;
        }

        var bag = BattingBag.Instance;
        var b = bag.GetBefore();
        var battings = CopyByNetwork(b);

        var roomIdx = UserInfoManager.RoomIdx;
        var uuid = UserInfoManager.loginInfo.UUID;
        var selectedIdx = UserInfoManager.SelectedIdx;

        bagInstance.IsSpecialBettingOK = false;

        var packet = new SpecialBatting(CallType.Request, roomIdx, uuid, selectedIdx, SpecialBattingType.Before, battings);

        if (!client.SendPacket(packet, out var error))
        {
            Debug.Log(error);
        }

        StartCoroutine(Co_SpecialBettingOKtoTrue());
    }


    private List<SingleBatting> CopyByNetwork(List<BattingRecord> battings)
    {
        List<SingleBatting> list = new List<SingleBatting>();

        foreach (var batting in battings)
        {
            var b = new SingleBatting
            {
                BattingType = ConvertBettingType(batting.Type),
                Batting = Convert.ToInt32(batting.Value)
            };

            list.Add(b);
        }

        return list;
    }

    public void CancleBetting()
    {
        var packet = new ClearPlayerBetting(
                UserInfoManager.RoomIdx,
                UserInfoManager.loginInfo.UUID,
                UserInfoManager.SelectedIdx
                );

        if (!client.SendPacket(packet, out var error))
        {
            Debug.Log(error);
        }
    }


    public void PlayerMaxBetting()
    {
        MaxBattingRequest();
    }

    public void MaxBattingRequest()
    {
        /*if (!isBatting)
        {
            return;
        }

        if (Player_bettype == BETTINGTYPE.NONE)
        {
            return;
        }

        var uuid = UserInfoManager.loginInfo.UUID;
        var roomIdx = UserInfoManager.RoomIdx;
        var selectedIdx = UserInfoManager.SelectedIdx;

        var req = new MaxBattingReq(roomIdx, uuid, selectedIdx, ConvertBettingType(Player_bettype));
        client.SendPacket(req);*/
    }

    private NetworkBettingType ConvertBettingType(BETTINGTYPE type)
    {
        switch (type)
        {
            case BETTINGTYPE.PPAIR:
                return NetworkBettingType.PlayerPair;
            case BETTINGTYPE.PLAYER:
                return NetworkBettingType.Player;
            case BETTINGTYPE.TIE:
                return NetworkBettingType.Tie;
            case BETTINGTYPE.BANKER:
                return NetworkBettingType.Banker;
            case BETTINGTYPE.BPAIR:
                return NetworkBettingType.BankerPair;
        }

        return NetworkBettingType.Tie;
    }

    private BETTINGTYPE ConvertBettingType(NetworkBettingType type)
    {
        switch (type)
        {
            case NetworkBettingType.PlayerPair:
                return BETTINGTYPE.PPAIR;
            case NetworkBettingType.Player:
                return BETTINGTYPE.PLAYER;
            case NetworkBettingType.Tie:
                return BETTINGTYPE.TIE;
            case NetworkBettingType.Banker:
                return BETTINGTYPE.BANKER;
            case NetworkBettingType.BankerPair:
                return BETTINGTYPE.BPAIR;
        }

        return BETTINGTYPE.NONE;
    }

    private int ConvertBettingTypeToInt(BETTINGTYPE type)
    {
        switch (type)
        {
            case BETTINGTYPE.PPAIR:
                return 1;
            case BETTINGTYPE.PLAYER:
                return 2;
            case BETTINGTYPE.TIE:
                return 3;
            case BETTINGTYPE.BANKER:
                return 4;
            case BETTINGTYPE.BPAIR:
                return 5;
        }

        return -1;
    }

    private int ConvertNetworkBettingTypeToInt(NetworkBettingType type)
    {
        switch (type)
        {
            case NetworkBettingType.PlayerPair:
                return 1;
            case NetworkBettingType.Player:
                return 2;
            case NetworkBettingType.Tie:
                return 3;
            case NetworkBettingType.Banker:
                return 4;
            case NetworkBettingType.BankerPair:
                return 5;
        }

        return -1;
    }

    private void MaxBattingResponce(MaxBattingRes data)
    {
        var uuid = UserInfoManager.loginInfo.UUID;
        var bagInstance = InGameBag.Instance;

        if (data.UUID == uuid)
        {
            // 내 배팅인경우
            StartCoroutine(MaxBattingByMy(data));

            bagInstance.IsChipClickedNow = false;
        }
        else
        {
            // 남의 배팅인경우
            StartCoroutine(MaxBattingByOthers(data));
        }
    }

    public IEnumerator MaxBattingByMy(MaxBattingRes data) 
    {
        yield return null;

        if (data.Error == BettingErrorType.OK) 
        {
            // 배팅 없애기
            BettingAmountTotalBanker -= Convert.ToInt32(baccaratplayer.BattingAmountBanker);
            BettingAmountTotalPlayer -= Convert.ToInt32(baccaratplayer.BattingAmountPlayer);
            BettingAmountTotalBP -= Convert.ToInt32(baccaratplayer.BattingAmountBPair);
            BettingAmountTotalPP -= Convert.ToInt32(baccaratplayer.BattingAmountPPair);
            BettingAmountTotalTie -= Convert.ToInt32(baccaratplayer.BattingAmountTie);

            baccaratplayer.RevertAllWithoutEffect();

            // 플레이어 정보 클리어
            //Player_bettype = BETTINGTYPE.NONE;
            //Player.CurrentBettings = 0;
            //Player.Betting_Bet_Revert();

            // 신호등 표시
            Player.PlayerBettingSignalCheck(ConvertNetworkBettingTypeToInt(data.BattingType));
            yield return new WaitForSeconds(0.05f);

            // 저장 로직 클리어
            var bag = BattingBag.Instance;
            bag.Clear();

            // 애니메이션 플레이
            //layer.Betting_MAX_Bet(data.Batting, ConvertBettingType(data.BattingType));
            baccaratplayer.changebettingtype(ConvertNetworkBettingTypeToInt(data.BattingType));
            baccaratplayer.ExecuteBettingMAX();

            baccaratplayer.UI_HightlightPanel[ConvertNetworkBettingTypeToInt(data.BattingType) -1].SetActive(true);

            yield return new WaitForSeconds(1.0f);

            // 배팅 총액 업데이트
            UpdateBettingAmountUsingDatas(data.BattingType, data.Batting);

            for (int i=1;i<6;i++)
                CurrentRoom.UpdateBoard(i);
        }
        else if (data.Error == BettingErrorType.OverUpperLimit)
        {
            var bagInstance = InGameBag.Instance;
            bagInstance.IsMaxBetNow = false;
            Alert("보너스게임 당첨 상한가 이상은\r\n배팅할 수 없습니다 !!", 1.5f);
        }
    }

    public IEnumerator MaxBattingByOthers(MaxBattingRes data)
    {
        yield return null;

        if (data.Error == BettingErrorType.OK)
        {
            var user = Users[data.SelectedIdx];
            if (user != null) 
            {
                var baccaratuser = user.gameObject.GetComponent<BaccaratPlayer>();

                // 배팅 없애기
                BettingAmountTotalBanker -= Convert.ToInt32(baccaratuser.BattingAmountBanker);
                BettingAmountTotalPlayer -= Convert.ToInt32(baccaratuser.BattingAmountPlayer);
                BettingAmountTotalBP -= Convert.ToInt32(baccaratuser.BattingAmountBPair);
                BettingAmountTotalPP -= Convert.ToInt32(baccaratuser.BattingAmountPPair);
                BettingAmountTotalTie -= Convert.ToInt32(baccaratuser.BattingAmountTie);

                // 플레이어 정보 클리어
                baccaratuser.RevertAllWithoutEffect();

                //user.CurrentBettings = 0;
                //user.Betting_Bet_Revert();

                // 신호등 표시
                user.PlayerBettingSignalCheck(ConvertNetworkBettingTypeToInt(data.BattingType));
                yield return new WaitForSeconds(0.05f);

                // 애니메이션 플레이
                baccaratuser.changebettingtype(ConvertNetworkBettingTypeToInt(data.BattingType));
                baccaratuser.ExecuteBettingMAX();
                //user.Betting_MAX_Bet(data.Batting, ConvertBettingType(data.BattingType));
                yield return new WaitForSeconds(1.0f);

                // 배팅 총액 업데이트
                UpdateBettingAmountUsingDatas(data.BattingType, data.Batting);

                for (int i=1;i<6;i++)
                    CurrentRoom.UpdateBoard(i);
            }
        }
    }

    private void BattingSpecialResponce(SpecialBatting data)
    {
        if (data.Call == CallType.Responce)
        {
            SpecialResponce(data);
        }
    }

    private void SpecialResponce(SpecialBatting data)
    {
        var uuid = UserInfoManager.loginInfo.UUID;
        var bagInstance = InGameBag.Instance;

        if (data.UUID == uuid)
        {
            // 내 배팅인경우
            StartCoroutine(SpecialByMy(data));
            bagInstance.IsSpecialBettingOK = true;
        }
        else
        {
            // 남의 배팅인경우
            StartCoroutine(SpecialByOthers(data));
        }
    }

    private IEnumerator SpecialByOthers(SpecialBatting data)
    {
        yield return null;

        if (data.Error == BettingErrorType.OK)
        {
            var user =  Users[data.SelectedIdx] ;

            if (user != null)
            {
                var baccaratuser = user.gameObject.GetComponent<BaccaratPlayer>();
                baccaratuser.Bettingtype = BETTINGTYPE.NONE;
                baccaratuser.CurrentBettingTotal = 0;

                BettingAmountTotalBanker -= Convert.ToInt32(baccaratuser.BattingAmountBanker);
                BettingAmountTotalPlayer -= Convert.ToInt32(baccaratuser.BattingAmountPlayer);
                BettingAmountTotalBP -= Convert.ToInt32(baccaratuser.BattingAmountBPair);
                BettingAmountTotalPP -= Convert.ToInt32(baccaratuser.BattingAmountPPair);
                BettingAmountTotalTie -= Convert.ToInt32(baccaratuser.BattingAmountTie);

                baccaratuser.RevertAllWithoutEffect();


                foreach (var b in data.Battings) 
                {
                    
                    baccaratuser.changebettingamount(b.Batting);
                    baccaratuser.changebettingtype(ConvertNetworkBettingTypeToInt(b.BattingType));
                    baccaratuser.ExecuteBetting();

                    user.PlayerBettingSignalCheck(ConvertNetworkBettingTypeToInt(b.BattingType));
                    
                    UpdateBettingAmountUsingDatas(b.BattingType, b.Batting);
                    yield return new WaitForSeconds(0.3f);
                }

                yield return new WaitForSeconds(0.1f);

                for (int i = 1; i < 6; i++)
                    CurrentRoom.UpdateBoard(i);
            }
        }
    }

    private int ToChipContainerType(NetworkBettingType btype)
    {
        int idx = -1;

        switch (btype)
        {
            case NetworkBettingType.Player:
                idx = 1;
                break;
            case NetworkBettingType.Banker:
                idx = 3;
                break;
            case NetworkBettingType.Tie:
                idx = 2;
                break;
            case NetworkBettingType.PlayerPair:
                idx = 0;
                break;
            case NetworkBettingType.BankerPair:
                idx = 4;
                break;
        }

        return idx;
    }

    private int ToChipContainerIdx(SingleBatting b) 
    {
        int idx = -1;

        switch (b.BattingType)
        {
            case NetworkBettingType.Player:
                idx = 1;
                break;
            case NetworkBettingType.Banker:
                idx = 3;
                break;
            case NetworkBettingType.Tie:
                idx = 2;
                break;
            case NetworkBettingType.PlayerPair:
                idx = 0;
                break;
            case NetworkBettingType.BankerPair:
                idx = 4;
                break;
        }

        return idx;
    }

    private IEnumerator SpecialByMy(SpecialBatting data)
    {
        if (data.Error == BettingErrorType.OK)
        {
            // 데이터 보내기
            var user = Player;
            if (user != null)
            {
                baccaratplayer.Bettingtype = BETTINGTYPE.NONE;
                baccaratplayer.CurrentBettingTotal = 0;


                BettingAmountTotalBanker -= Convert.ToInt32(baccaratplayer.BattingAmountBanker);
                BettingAmountTotalPlayer -= Convert.ToInt32(baccaratplayer.BattingAmountPlayer);
                BettingAmountTotalBP -= Convert.ToInt32(baccaratplayer.BattingAmountBPair);
                BettingAmountTotalPP -= Convert.ToInt32(baccaratplayer.BattingAmountPPair);
                BettingAmountTotalTie -= Convert.ToInt32(baccaratplayer.BattingAmountTie);

                baccaratplayer.RevertAllWithoutEffect();

                for (int i = 0; i < 5; i++)
                {
                    baccaratplayer.UI_HightlightPanel[i].SetActive(false);
                }

                foreach (var b in data.Battings)
                {


                    baccaratplayer.changebettingamount(b.Batting);
                    baccaratplayer.changebettingtype(ConvertNetworkBettingTypeToInt(b.BattingType));
                    baccaratplayer.ExecuteBetting();

                    baccaratplayer.UI_HightlightPanel[(ConvertNetworkBettingTypeToInt(b.BattingType)) - 1].SetActive(true);

                    user.PlayerBettingSignalCheck(ConvertNetworkBettingTypeToInt(b.BattingType));
                    UpdateBettingAmountUsingDatas(b.BattingType, b.Batting);

                    yield return new WaitForSeconds(0.3f);
                }

                for (int i = 1; i < 6; i++)
                    CurrentRoom.UpdateBoard(i);
            }

            // 현재 정보 저장
            var bag = BattingBag.Instance;
            bag.SetBySp(data.Battings);
        }
        else if(data.Error == BettingErrorType.OverUpperLimit)
        {
            Alert("보너스게임 당첨 상한가 이상은\r\n배팅할 수 없습니다 !!", 1.5f);
        }
    }

    private void BattingClearResponce(ClearPlayerBetting data)
    {
        var uuid = UserInfoManager.loginInfo.UUID;

        if (data.uuid == uuid)
        {
            // 내 배팅인경우
            StartCoroutine(BattingClearMy());
        }
        else
        {
            // 남의 배팅인경우
            StartCoroutine(BattingClearOther(data));
        }
    }

    private IEnumerator BattingClearMy() 
    {
        yield return null;

        BettingAmountTotalBanker -= Convert.ToInt32(baccaratplayer.BattingAmountBanker);
        BettingAmountTotalPlayer -= Convert.ToInt32(baccaratplayer.BattingAmountPlayer);
        BettingAmountTotalBP -= Convert.ToInt32(baccaratplayer.BattingAmountBPair);
        BettingAmountTotalPP -= Convert.ToInt32(baccaratplayer.BattingAmountPPair);
        BettingAmountTotalTie -= Convert.ToInt32(baccaratplayer.BattingAmountTie);

        baccaratplayer.RevertAllWithoutEffect();
        
        Player.PlayerBettingSignalClear();

        // 저장 로직 클리어
        var bag = BattingBag.Instance;
        bag.Clear();

        var bagInstance = InGameBag.Instance;
        bagInstance.IsMaxBetNow = false;

        for (int i = 1; i < 6; i++)
        {
            baccaratplayer.UI_HightlightPanel[i-1].SetActive(false);
            CurrentRoom.UpdateBoard(i);
        }
            
    }

    private IEnumerator BattingClearOther(ClearPlayerBetting data) 
    {
        yield return null;

        var user = Users[data.selectedIdx];

        if (user != null)
        {
            var baccaratuser = user.gameObject.GetComponent<BaccaratPlayer>();

            BettingAmountTotalBanker -= Convert.ToInt32(baccaratuser.BattingAmountBanker);
            BettingAmountTotalPlayer -= Convert.ToInt32(baccaratuser.BattingAmountPlayer);
            BettingAmountTotalBP -= Convert.ToInt32(baccaratuser.BattingAmountBPair);
            BettingAmountTotalPP -= Convert.ToInt32(baccaratuser.BattingAmountPPair);
            BettingAmountTotalTie -= Convert.ToInt32(baccaratuser.BattingAmountTie);

            baccaratuser.RevertAllWithoutEffect();

            user.PlayerBettingSignalClear();

            for(int i=1;i<6;i++)
                CurrentRoom.UpdateBoard(i);
        }
    }
    #endregion

    public void Alert(string msg, float showtime)
    {
        // 알림 메시지 추가
        Text_NoticeAlert.text = msg;
        Obj_NoticeAlert.transform.DOScale(1.0f, 0.7f);

        // 알림 메시지 삭제 루틴 동작
        StartCoroutine(closeAlert(showtime));
    }

    private IEnumerator closeAlert(float showtime)
    {
        yield return new WaitForSeconds(showtime);

        // 알림 페이드 아웃
        Obj_NoticeAlert.transform.DOScale(0.0f, 0.7f);

        // 알림 메시지 초기화
        Text_NoticeAlert.text = "";
    }

    #region Chat

    private void SystemMessage(string msg) 
    {
        if (AppSettingManager.GetEnv() == EnvironmentType.Live) 
        {
            return;
        }

        var msgObj = Instantiate(messageItem);
        Text changeMsg = msgObj.transform.GetChild(0).gameObject.GetComponent<Text>();
        changeMsg.text = $"System : {msg}";
        msgObj.transform.SetParent(messageArea.transform, false);
        StartCoroutine(SetScrollRect());
    }

    #endregion

    #region SocketError

    private bool IsConnected(Socket socket) 
    {
        try
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private void EmergencyEnd(Socket socket) 
    {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
    }

    #endregion
}