using Assets.Scripts.Baccarrat;
using Assets.Scripts.Baccarrat.InGames;
using Assets.Scripts.Loggers;
using Assets.Scripts.Settings;
using Module.Apis.Rooms;
using Module.Utils.Currency;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.UI;

public enum BETTINGTYPE : int
{
    PPAIR=1,
    PLAYER=2,
    TIE=3,
    BANKER=4,
    BPAIR=5,
    NONE=0
}

public class Room : MonoBehaviour
{
    UnityEngine.Object[] Numbers;

    Animator roomanimator;

    private AudioSource musicPlayer;
    
    // 플레이어 뱅커 사운드 플레이하는 소스 및 클립
    public AudioSource GameSoundPlayer;

    public AudioClip PlayerSound;
    public AudioClip BankerSound;
    public AudioClip NaturalSound;

    public ProgressBarController bettingTimerController;

    public AudioClip cardPickSound;
    public AudioClip cardPickRawSound;

    public AudioClip cardFlipSound;
    public AudioClip cardFlipRawSound;

    public AudioClip normalWin;
    public AudioClip bigWin;

    public AudioClip chipBet;
    public AudioClip chipRevert;

    public Animator Rainbow_Player;
    public Animator Rainbow_Banker;

    public Sprite[] MaxSprites = new Sprite[2];
    public Highlights[] highlight = new Highlights[5];
    public Image MaxButton;
    public GameObject MaxRing;

    //라운드 중 베팅 총액
    public long Betting_Amount_PPair;
    public long Betting_Amount_Player;
    public long Betting_Amount_BPair;
    public long Betting_Amount_Banker;
    public long Betting_Amount_Tie;

    public ushort Betting_Player_score; //카드를 뒤집으면서 결정되는 플레이어 점수.
    public ushort Betting_Banker_score; //카드를 뒤집으면서 결정되는 뱅커 점수.

    public SpriteRenderer[] CardFront = new SpriteRenderer[6];
    
    public GameObject[] Buttons = new GameObject[7];

    public GameObject chinaBoard;
    public Animator chinaBoard_Animator;

    public KLNetwork_Baccarat baccaratNetwork;

    public Natural_Firework Player_Natural;
    public Natural_Firework Banker_Natural;

    public GameObject[] MaxFireWorks = new GameObject[3];
    public GameObject NaturalFirework_Player;
    public GameObject NaturalFirework_Banker;

    public Text Board_PPair;
    public Text Board_Player;
    public Text Board_Tie;
    public Text Board_Banker;
    public Text Board_BPair;

    private ContentSizeFitter Board_PPair_csf;
    private ContentSizeFitter Board_Player_csf;
    private ContentSizeFitter Board_Tie_csf;
    private ContentSizeFitter Board_Banker_csf;
    private ContentSizeFitter Board_BPair_csf;

    // 게임 종료 패널
    public GameObject GameExit;

    //유저 슬롯
    public UserData_Play Player = null;
    public BETTINGTYPE Player_bettype = BETTINGTYPE.NONE;
    public UserData_Play[] Users = new UserData_Play[7];

    public KLSocketManager NetworkHandler;

    private int prevCount = 0;

    public GameObject Player_MaxEffect;
    public GameObject Banker_MaxEffect;
    public GameObject Tie_MaxEffect;

    public bool IsLoadingPanel { get; set; } = false;

    private void Update()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            GameExit.SetActive(true);
        }

        if (NetworkHandler.baccaratplayer != null)
        {
            if(prevCount != NetworkHandler.baccaratplayer.DoNotBettingCount && AppSettingManager.GetEnv() != EnvironmentType.Development)
            {
                // 개발 모드 추가
                if (NetworkHandler.baccaratplayer.DoNotBettingCount >= 5)
                {
                    // 배팅 5회 이상 안했을 때 자동 강퇴..
                    NetworkHandler.LeaveGame();
                }
                else if (NetworkHandler.baccaratplayer.DoNotBettingCount < 5 && NetworkHandler.baccaratplayer.DoNotBettingCount >= 3)
                {
                    NetworkHandler.Alert("자동 퇴장까지" + (5 - NetworkHandler.baccaratplayer.DoNotBettingCount) + "회 남았습니다.\r\n배팅을 해주시길바랍니다.", 1.5f);
                }
                prevCount = NetworkHandler.baccaratplayer.DoNotBettingCount;
            }
        }

        if (bettingTimerController.IsTimerEnabled())
        {
            var bagInstance = InGameBag.Instance;

            if (bagInstance.IsMiddlePosition)
            {
                // TODO :: Betting 기록 지우기
                bagInstance.IsMaxBetNow = false;

                // 칩 revert
                for (int i=0;i<7;i++)
                {
                    var ruser = Users[i];
                    var baccaratruser = ruser.gameObject.GetComponent<BaccaratPlayer>();

                    if(baccaratruser != null)
                    {
                        baccaratruser.BattingAmountBanker = 0;
                        baccaratruser.BattingAmountBPair = 0;
                        baccaratruser.BattingAmountPlayer = 0;
                        baccaratruser.BattingAmountPPair = 0;
                        baccaratruser.BattingAmountTie = 0;

                        ruser.PlayerBettingSignalClear();
                        baccaratruser.RevertAllWithoutEffect();
                    }
                }

                // 토탈 금액 0원 초기화
                NetworkHandler.BettingAmountTotalBanker = 0;
                NetworkHandler.BettingAmountTotalBP = 0;
                NetworkHandler.BettingAmountTotalPlayer = 0;
                NetworkHandler.BettingAmountTotalPP = 0;
                NetworkHandler.BettingAmountTotalTie = 0;

                bagInstance.IsMiddlePosition = false;
            }
        }
    }

    private void Start()
    {
        chinaBoard_Animator = chinaBoard.gameObject.GetComponent<Animator>();
        Numbers = Resources.LoadAll<Sprite>("Sprites/UI_Play_Bakara");
        roomanimator = gameObject.GetComponent<Animator>();
        musicPlayer = GetComponent<AudioSource>();
        RoomDataManager.Instance();

        Board_PPair_csf = Board_PPair.transform.parent.GetComponent<ContentSizeFitter>();
        Board_Player_csf = Board_Player.transform.parent.GetComponent<ContentSizeFitter>();
        Board_Tie_csf = Board_Tie.transform.parent.GetComponent<ContentSizeFitter>();
        Board_Banker_csf = Board_Banker.transform.parent.GetComponent<ContentSizeFitter>();
        Board_BPair_csf = Board_BPair.transform.parent.GetComponent<ContentSizeFitter>();
    }

    /// <summary>
    /// 로비로 이동
    /// </summary>
    public void GotoLobby()
    {
        SceneChanger.CallSceneLoader("Lobby");
    }

    /// <summary>
    /// 보드 위 배팅금액 업데이트
    /// </summary>
    /// <param name="index"></param>
    /// 

    public void UpdateBoardAtOnce()
    {
        if(NetworkHandler.baccaratplayer != null)
        {
                Board_PPair.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountPPair), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPP));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_PPair_csf.transform);

                Board_Player.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountPlayer), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPlayer));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Player_csf.transform);
 
                Board_Tie.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountTie), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalTie));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Tie_csf.transform);

                Board_Banker.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountBanker), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBanker));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Banker_csf.transform);

                Board_BPair.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountBPair), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBP));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_BPair_csf.transform);
        }
        else
        {
                Board_PPair.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPP));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_PPair_csf.transform);

                Board_Player.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPlayer));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Player_csf.transform);

                Board_Tie.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalTie));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Tie_csf.transform);

                Board_Banker.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBanker));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Banker_csf.transform);

                Board_BPair.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBP));
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_BPair_csf.transform);

        }
    }

    public void UpdateBoard(int index)
    {
        if(NetworkHandler.baccaratplayer != null)
        {
            switch(index - 1)
            {
                case 0:
                    Board_PPair.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountPPair), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPP));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_PPair_csf.transform);
                    break;
                case 1:
                    Board_Player.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountPlayer), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPlayer));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Player_csf.transform);
                    break;
                case 2:
                    Board_Tie.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountTie), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalTie));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Tie_csf.transform);
                    break;
                case 3:
                    Board_Banker.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountBanker), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBanker));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Banker_csf.transform);
                    break;
                case 4:
                    Board_BPair.text = string.Format("<color=#FFC631FF>{0}</color><color=#FFC631A5><size=18>/{1}</size></color>", CurrencyConverter.KorMax(NetworkHandler.baccaratplayer.BattingAmountBPair), CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBP));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_BPair_csf.transform);
                    break;
            }
        }
        else
        {
            switch (index - 1)
            {
                case 0:
                    Board_PPair.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPP));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_PPair_csf.transform);
                    break;
                case 1:
                    Board_Player.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalPlayer));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Player_csf.transform);
                    break;
                case 2:
                    Board_Tie.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalTie));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Tie_csf.transform);
                    break;
                case 3:
                    Board_Banker.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBanker));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_Banker_csf.transform);
                    break;
                case 4:
                    Board_BPair.text = string.Format("<color=#FFC631FF>0</color><color=#FFC631A5><size=18>/{0}</size></color>", CurrencyConverter.KorMax(NetworkHandler.BettingAmountTotalBP));
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Board_BPair_csf.transform);
                    break;
            }
        }
    }




    public void OffButtons()
    {
        foreach(GameObject btn in Buttons)
        {
            btn.SetActive(false);
        }
    }

    //카드ID에 따른 점수 계산 후 반환.
    public ushort GetCardScore(ushort input)
    {
        if ((input % 13) <= 10)
            return (ushort)(input % 13);
        else
            return 0;
    }

    public Animator[] Cards = new Animator[6];

    public SpriteRenderer PlayerScoreSprite, BankerScoreSprite;

    public SpriteRenderer[] ResultScoreSprite = new SpriteRenderer[3];

    public AudioSource PlayerScoreAudio, BankerScoreAudio;
    public AudioClip[] ScoreAudios = new AudioClip[10];


    public Button[] BoardButtons = new Button[6];
    public Button[] BoardButtons_init = new Button[5];

    public void DisableButtons()
    {
        for (int i = 0; i < 5; i++)
        {
            highlight[i].AmbientEffect.SetActive(false);
            BoardButtons[i].gameObject.SetActive(false);
            BoardButtons_init[i].gameObject.SetActive(false);
        }
    }

    public void EnableButtons()
    {
        for (int i = 0; i < 5; i++)
        {
            highlight[i].AmbientEffect.SetActive(true);
            BoardButtons[i].gameObject.SetActive(true);
            BoardButtons_init[i].gameObject.SetActive(true);
        }
    }

    public void DisabledTimer()
    {
        var bagInstance = InGameBag.Instance;
        bagInstance.IsBettingOK = false;

        bettingTimerController.Clear();
    }

    public void DefineCardvalue()
    {
        var manager = RoomDataManager.Instance();
        Sprite[] spriteList = manager.UpdateCardInfo();

        for (int i=0; i<6; i++)
            CardFront[i].sprite = spriteList[i];

        roomanimator.SetTrigger("Defined");
    }

    public void NoneFlipCard(int index)
    {
        Cards[index].SetTrigger("Seq_card_noneflip");
    }

    public void FlipCard1(int index)
    {
        playSound(cardFlipSound);
        Cards[index].SetTrigger("Seq_card_flip");
    }
    public void FlipCard2(int index)
    {
        playSound(cardFlipRawSound);
        Cards[index].SetTrigger("Seq_card_flip2");
    }

    public void FlipCard3(int index)
    {
        playSound(cardFlipSound);
        Cards[index].SetTrigger("Seq_card_flip4");
    }

    public void FlipCard4(int index)
    {
        playSound(cardFlipRawSound);
        Cards[index].SetTrigger("Seq_card_flip4");
    }

    public void FlipCard5(int index)
    {
        playSound(cardFlipSound);
        Cards[index].SetTrigger("Seq_card_flip5");
    }

    public void FlipCard6(int index)
    {
        playSound(cardFlipSound);
        Cards[index].SetTrigger("Seq_card_flip6");
    }

    public void ActiveMaxEffect(Assets.Scripts.Network.Bakaras.BakaraWinner winner, Assets.Scripts.Network.Bakaras.BakaraWinner bettingTarget)
    {
        var bagInstance = InGameBag.Instance;

        if (!bagInstance.IsMaxBetNow) return;

        if (winner != bettingTarget) return;

        switch (winner)
        {
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Player:
                    Player_MaxEffect.SetActive(true);
                break;
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Tie:
                    Tie_MaxEffect.SetActive(true);
                break;
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Banker:
                    Banker_MaxEffect.SetActive(true);
                break;
        }
    }

    public void ResetCard(int index)
    {
        Cards[index].Play("Standby");
    }

    public void ResetAnimParameters()
    {
        MyLog.Write($"{LogLocation.Room}-ResetAnimParameters", "Batting Clear");

        var bagInstance = InGameBag.Instance;
        bagInstance.IsMaxBetNow = false;
        bagInstance.IsMiddlePosition = false;
        bagInstance.IsChipClickedNow = false;

        PlayerScoreAudio.clip = null;
        BankerScoreAudio.clip = null;
        PlayerScoreSprite.sprite = null;
        BankerScoreSprite.sprite = null;

        Player_MaxEffect.SetActive(false);
        Banker_MaxEffect.SetActive(false);
        Tie_MaxEffect.SetActive(false);

        StartCoroutine(Co_BettingClear());

        NetworkHandler.BettingTotalClear();

        foreach (var MaxFireWork in MaxFireWorks)
            MaxFireWork.SetActive(false);

        Betting_Banker_score = 0;
        Betting_Player_score = 0;

        for (int i = 0; i < 5; i++)
        {
            highlight[i].AvailableZone.SetActive(false);
            highlight[i].Particle.SetActive(false);
            highlight[i].Upper.SetActive(false);
            highlight[i].UpperHigh.SetActive(false);
            highlight[i].AmbientEffect.SetActive(false);

            UpdateBoard(i+1);
        }

        foreach (var parameter in roomanimator.parameters)
        {
            roomanimator.ResetTrigger(parameter.name);
        }
    }

    private IEnumerator Co_BettingClear()
    {
        yield return null;

        var roomdata = RoomDataManager.Instance();

        IsLoadingPanel = true;

        for (int i = 0; i < 7; i++)
        {
            if (NetworkHandler.Users[i] == null) continue;

            var baccaratuser = NetworkHandler.Users[i].gameObject.GetComponent<BaccaratPlayer>();

            // 배팅 하지 않음 카운트를 추가함
            baccaratuser.DoNotBettingCount += 1;

            List<ChipContainer> containerList = new List<ChipContainer>();
            ChipContainer containerList_BPair = new ChipContainer();
            ChipContainer containerList_PPair = new ChipContainer();

            if (roomdata.isBankerPair)
            {
                baccaratuser.BattingAmountBPair *= 12;
                containerList_BPair = baccaratuser.ChipContainers[4];
            }
            else
            {
                baccaratuser.BattingAmountBPair = 0;
            }

            if (roomdata.isPlayerPair)
            {
                baccaratuser.BattingAmountPPair *= 12;
                containerList_PPair = baccaratuser.ChipContainers[0];
            }
            else
            {
                baccaratuser.BattingAmountPPair = 0;
            }

            if (roomdata.winner == Assets.Scripts.Network.Bakaras.BakaraWinner.Tie)
            {
                baccaratuser.BattingAmountTie *= 9;

                containerList.Add(baccaratuser.ChipContainers[2]);
            }
            else if (roomdata.winner == Assets.Scripts.Network.Bakaras.BakaraWinner.Player)
            {
                baccaratuser.BattingAmountTie = 0;
                baccaratuser.BattingAmountBanker = 0;
                baccaratuser.BattingAmountPlayer *= 2;

                containerList.Add(baccaratuser.ChipContainers[1]);
            }
            else if (roomdata.winner == Assets.Scripts.Network.Bakaras.BakaraWinner.Banker)
            {
                baccaratuser.BattingAmountTie = 0;
                baccaratuser.BattingAmountBanker = Convert.ToInt32(Math.Truncate(((double)baccaratuser.BattingAmountBanker * 1.95)));
                baccaratuser.BattingAmountPlayer = 0;
                containerList.Add(baccaratuser.ChipContainers[3]);
            }

            StartCoroutine(baccaratuser.WinnerEffect(containerList,containerList_BPair,containerList_PPair));
            
        }
    }

    public void Set_Score()
    {
        if (Betting_Banker_score == Betting_Player_score)
        {
            for (int i = 0; i < 3; i++)
            {
                ResultScoreSprite[i].sprite = PlayerScoreSprite.sprite;
            }
        }
        else if (Betting_Banker_score < Betting_Player_score)
        {
            for (int i = 0; i < 3; i++)
            {
                ResultScoreSprite[i].sprite = PlayerScoreSprite.sprite;
            }
        }
        else if (Betting_Banker_score > Betting_Player_score)
        {
            for (int i = 0; i < 3; i++)
            {
                ResultScoreSprite[i].sprite = BankerScoreSprite.sprite;
            }
        }

    }

    public void Set_PlayerSeet(int index)
    {
        if (Users[index].ProfileID == 0 && Player == null)
        {
            Users[index].Nickname = UserInfoManager.AccountInfo.NicName;
            Users[index].ProfileID = UserInfoManager.loginInfo.UUID;
            Users[index].CurrentChips = UserInfoManager.AccountInfo.Cash;
            Users[index].ChangePlayerBorder(Color.red);
            Player = Users[index];
            Player.UpdateDisplays();
        }
    }

    public void Bet(int amount)
    {
        playSound(chipBet);
        if (Player_bettype != BETTINGTYPE.NONE && Player != null)
            Player.Betting_Bet(amount, Player_bettype);
    }

    public void Bet_Max()
    {
        this.Bet((int)Player.CurrentChips);
    }

    public void RevertBet()
    {
        if (Player_bettype != BETTINGTYPE.NONE && Player != null)
        {
            playSound(chipRevert);
            
            Player_bettype = BETTINGTYPE.NONE;
            Player.Betting_Bet_Revert();
        }
    }

    public void ClearAllPlayersSignalList()
    {
        NetworkHandler.ClearAllSignalList();
    }

    public void changebettingtype(int type)
    {
        Player_bettype = (BETTINGTYPE)type;
    }

    public void Set_PlayerScore()
    {
        var manager = RoomDataManager.Instance();

        PlayerScoreAudio.clip = ScoreAudios[Betting_Player_score];

        PlayerScoreSprite.gameObject.SetActive(false);

        if (Betting_Player_score == 0)
            PlayerScoreSprite.sprite = (Sprite)Numbers[25];
        else
            PlayerScoreSprite.sprite = (Sprite)Numbers[Betting_Player_score + 15];

        // 플레이어 0~9 사운드 재생
        StartCoroutine(PlayScoreSound_Coroutine(false, manager.isPlayerNatural, ScoreAudios[Betting_Player_score]));

        PlayerScoreSprite.gameObject.SetActive(true);

    }

    public void Set_BankerScore()
    {
        var manager = RoomDataManager.Instance();
        
        BankerScoreSprite.gameObject.SetActive(false);

        if (Betting_Banker_score == 0)
            BankerScoreSprite.sprite = (Sprite)Numbers[25];
        else
            BankerScoreSprite.sprite = (Sprite)Numbers[Betting_Banker_score + 15];

        // 뱅커 0~9 사운드 재생
        StartCoroutine(PlayScoreSound_Coroutine(true, manager.isBankerNatural, ScoreAudios[Betting_Banker_score]));

        BankerScoreSprite.gameObject.SetActive(true);
    }

    IEnumerator PlayScoreSound_Coroutine(bool isBanker, bool isNatural, AudioClip numberClip)
    {
        yield return null;

        if(isBanker)
        {
            // 1. 뱅커 재생 PlayGameSound 
            PlayGameSound(BankerSound, 0.5f);
            yield return new WaitForSeconds(0.8f);
        }
        else
        {
            // 1. 플레이어 재생 
            PlayGameSound(PlayerSound, 0.5f);
            yield return new WaitForSeconds(0.8f);
        }
        
        if(isNatural)
        {
            // 2. 내츄럴 재생 
            PlayGameSound(NaturalSound, 0.75f);
            yield return new WaitForSeconds(0.8f);
        }
        
        if(numberClip != null)
        {
            // 3. 숫자 재생
            PlayGameSound(numberClip, 0.8f);
        }
    }

    public void ToggleRainbow(int index)
    {
        switch(index)
        {
            case 0:
                Rainbow_Player.SetTrigger("loop");
                break;
            case 1:
                Rainbow_Banker.SetTrigger("loop");
                break;
            default:
                break;
        }
    }

    public void Verify_PlayerCardsize_sub()
    {
        var manager = RoomDataManager.Instance();

        Betting_Player_score += (ushort)manager.CardValue_Player[0];
        Betting_Player_score += (ushort)manager.CardValue_Player[1];

        Betting_Player_score %= 10;

        int rand = manager.RandomSeed;

        switch (rand)
        {
            case 0:
                roomanimator.SetTrigger("Rotation");
                break;
            case 1:
                roomanimator.SetTrigger("Upsidedown");
                break;
            case 2:
                roomanimator.SetTrigger("NonRotation");
                break;
            case 3:
                roomanimator.SetTrigger("righttodown");
                break;
        }

        if (manager.isPlayerNatural)
        {
            if(Betting_Player_score == 8)
            {
                Player_Natural.num = 0;
            }
            else if (Betting_Player_score == 9)
            {
                Player_Natural.num = 1;
            }

            roomanimator.SetTrigger("Natural");
        }
    }

    public void Verify_PlayerCardsData_ByNetworkDatas()
    {
        var manager = RoomDataManager.Instance();

        Betting_Player_score += (ushort)manager.CardValue_Player[0];
        Betting_Player_score += (ushort)manager.CardValue_Player[1];

        Betting_Player_score %= 10;

        if (manager.isPlayerNatural)
        {
            if (Betting_Player_score == 8)
            {
                Player_Natural.num = 0;
            }
            else if (Betting_Player_score == 9)
            {
                Player_Natural.num = 1;
            }

            roomanimator.SetTrigger("Natural");
        }
    }

    public void Verify_BankerCardsize_sub()
    {
        var manager = RoomDataManager.Instance();

        Betting_Banker_score += (ushort)manager.CardValue_Banker[0];
        Betting_Banker_score += (ushort)manager.CardValue_Banker[1];

        Betting_Banker_score %= 10;

        int rand = manager.RandomSubSeed;

        switch (rand)
        {
            case 0:
                roomanimator.SetTrigger("Rotation");
                break;
            case 1:
                roomanimator.SetTrigger("Upsidedown");
                break;
            case 2:
                roomanimator.SetTrigger("NonRotation");
                break;
            case 3:
                roomanimator.SetTrigger("righttodown");
                break;
        }

        if (manager.isBankerNatural)
        {
            if (Betting_Banker_score == 8)
            {
                Banker_Natural.num = 0;
            }
            else if (Betting_Banker_score == 9)
            {
                Banker_Natural.num = 1;
            }

            roomanimator.SetTrigger("Natural");
        }
    }

    public void Verify_BankerCardsData_ByNetworkDatas()
    {
        var manager = RoomDataManager.Instance();

        Betting_Banker_score += (ushort)manager.CardValue_Banker[0];
        Betting_Banker_score += (ushort)manager.CardValue_Banker[1];

        Betting_Banker_score %= 10;

        if (manager.isBankerNatural)
        {
            if (Betting_Banker_score == 8)
            {
                Banker_Natural.num = 0;
            }
            else if (Betting_Banker_score == 9)
            {
                Banker_Natural.num = 1;
            }

            roomanimator.SetTrigger("Natural");
        }
    }

    public void BoardShowAtBattingStart()
    {
        if (PlayerPrefs.GetInt("isAutoChina", 1) == 1)
        {
            baccaratNetwork.BoardLoadingRequest();
            chinaBoard_Animator.SetBool("ChinaBoardOn",true);
        }
        baccaratNetwork.BoardLoadingRequest();
    }

    public void ToggleChinaBoard()
    {
        bool toggleFlag = !chinaBoard_Animator.GetBool("ChinaBoardOn");

        chinaBoard_Animator.SetBool("ChinaBoardOn", toggleFlag);
    }

    public void SetChinaBoard(bool boolean)
    {
        chinaBoard_Animator.SetBool("ChinaBoardOn", boolean);
    }

    public void Add_PlayerSub2Score()
    {
        var manager = RoomDataManager.Instance();
        var value = (ushort)manager.CardValue_Player[2];

        if (value > 0)
        {
            Betting_Player_score += value;
        }

        Betting_Player_score %= 10;
    }

    public void Add_BankerSub2Score()
    {
        var manager = RoomDataManager.Instance();
        var value = (ushort)manager.CardValue_Banker[2];

        if(value > 0)
        {
            Betting_Banker_score += value;
        }

        Betting_Banker_score %= 10;
    }

    public void Verify_PlayerSub2Card()
    {
        var manager = RoomDataManager.Instance();

        if (manager.CardID_Player[2] > 0)
        {
            roomanimator.SetTrigger("3rdCard_Player");
        }
        else
        {
            if (manager.CardID_Banker[2] > 0)
            {
                roomanimator.SetTrigger("3rdCard_Banker");
            }
            else roomanimator.SetTrigger("Check_Final_Status");
        }
    }

    public void Verify_BankerSub2Card()
    {
        var manager = RoomDataManager.Instance();

        if (manager.CardID_Banker[2] > 0)
        {
            roomanimator.SetTrigger("3rdCard_Banker");
        }
    }

    public void ActivateSelection(int index)
    {
        ClearSelection();
        highlight[index].Particle.SetActive(true);
        //highlight[index].Upper.SetActive(true);
        highlight[index].UpperHigh.SetActive(true);
    }

    public void ClearSelection()
    {
        for(int i=0; i<5; i++)
        {
            highlight[i].Particle.SetActive(false);
            //highlight[i].Upper.SetActive(false);
            highlight[i].UpperHigh.SetActive(false);
        }
    }

    public void ClearAllSelection()
    {
       if(NetworkHandler.Player != null)
        {

            for(int i=0;i<5;i++)
            {
                highlight[i].Particle.SetActive(false);
                highlight[i].AvailableZone.SetActive(false);
                highlight[i].UpperHigh.SetActive(false);
            }

            /*
            highlight[0].Particle.SetActive(false);
            highlight[1].Particle.SetActive(false);
            highlight[2].Particle.SetActive(false);

            if (NetworkHandler.Player.Betting_Amount_PPair <= 0)
            {
                highlight[0].AvailableZone.SetActive(false);
                highlight[0].Upper.SetActive(false);
                highlight[0].UpperHigh.SetActive(false);
            }
            else
                highlight[0].Upper.SetActive(true);

            if (NetworkHandler.Player.Betting_Amount_Player <= 0)
            {
                highlight[1].AvailableZone.SetActive(false);
                highlight[1].Upper.SetActive(false);
                highlight[1].UpperHigh.SetActive(false);
            }
            else
                highlight[1].Upper.SetActive(true);

            if (NetworkHandler.Player.Betting_Amount_Tie <= 0)
            {
                highlight[2].AvailableZone.SetActive(false);
                highlight[2].Upper.SetActive(false);
                highlight[2].UpperHigh.SetActive(false);
            }
            else
                highlight[2].Upper.SetActive(true);

            if (NetworkHandler.Player.Betting_Amount_Banker <= 0)
            {
                highlight[3].AvailableZone.SetActive(false);
                highlight[3].Particle.SetActive(false);
                highlight[3].Upper.SetActive(false);
                highlight[3].UpperHigh.SetActive(false);
            }
            else
                highlight[3].Upper.SetActive(true);

            if (NetworkHandler.Player.Betting_Amount_BPair <= 0)
            {
                highlight[4].AvailableZone.SetActive(false);
                highlight[4].Particle.SetActive(false);
                highlight[4].Upper.SetActive(false);
                highlight[4].UpperHigh.SetActive(false);
            }
            else
                highlight[4].Upper.SetActive(true);

            */
        }

    }

    public void SetCrown()
    {
        var manager = RoomDataManager.Instance();
        
        //manager.showDatas();

        switch (manager.winner)
        {
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Banker:
                this.NaturalFirework_Player.SetActive(false);
                if(!manager.isBankerNatural)
                    this.NaturalFirework_Banker.SetActive(false);
                break;
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Player:
                this.NaturalFirework_Banker.SetActive(false);
                if (!manager.isPlayerNatural)
                    this.NaturalFirework_Player.SetActive(false);
                break;
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Tie:
                if (manager.isPlayerNatural && manager.isBankerNatural)
                    this.NaturalFirework_Banker.SetActive(false);
                break;
        }
    }

    public void Setgame()
    {
        var manager = RoomDataManager.Instance();
        var player = NetworkHandler.baccaratplayer;

        switch (manager.winner)
        {
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Banker:
                roomanimator.SetTrigger("BankerWin");
                break;
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Player:
                roomanimator.SetTrigger("PlayerWin");
                break;
            case Assets.Scripts.Network.Bakaras.BakaraWinner.Tie:
                roomanimator.SetTrigger("TieWin");
                break;
        }

        if (player != null)
        {
            if (player.BattingAmountPlayer > 0)
            {
                ActiveMaxEffect(manager.winner, Assets.Scripts.Network.Bakaras.BakaraWinner.Player);
            }
            else if (player.BattingAmountTie > 0)
            {
                ActiveMaxEffect(manager.winner, Assets.Scripts.Network.Bakaras.BakaraWinner.Tie);
            }
            else
            {
                ActiveMaxEffect(manager.winner, Assets.Scripts.Network.Bakaras.BakaraWinner.Banker);
            }
        }

        SetPair();
    }

    public void SetPair()
    {
        var mgr = RoomDataManager.Instance();
        
        mgr.showDatas();

        if (mgr.isPlayerPair)
            roomanimator.SetTrigger("Seq_PlayerPair");

        if (mgr.isBankerPair)
            roomanimator.SetTrigger("Seq_BankerPair");
    }

    public void CheckShowDown()
    {
        
    }

    public void CannotShowLoadingImage()
    {
        IsLoadingPanel = false;
    }

    public void ShowChinaBoardCheck()
    {
        var bagInstance = InGameBag.Instance;
        
        bagInstance.IsBettingOK = true;
        bagInstance.IsSpecialBettingOK = true;

        if (PlayerPrefs.GetInt("isAutoChina", 1) == 1)
        {
            baccaratNetwork.BoardLoadingRequest();
            chinaBoard_Animator.SetBool("ChinaBoardOn", true);
        }
        else baccaratNetwork.BoardLoadingRequest();
    }

    public void ResetChips()
    {
        
    }

    public void Betting_UpdateAmount_PPair()
    {
        Betting_Amount_PPair = 0;
        for (int i = 0; i < 7; i++)
        {
            if (Users[i] != null)
                Betting_Amount_PPair += Users[i].Betting_Amount_PPair;
        }
    }

    public void Betting_UpdateAmount_Player()
    {
        Betting_Amount_Player = 0;
        for (int i = 0; i < 7; i++)
        {
            if (Users[i] != null)
                Betting_Amount_Player += Users[i].Betting_Amount_Player;
        }
    }

    public void Betting_UpdateAmount_Tie()
    {
        Betting_Amount_Tie = 0;
        for (int i = 0; i < 7; i++)
        {
            if (Users[i] != null)
                Betting_Amount_Tie += Users[i].Betting_Amount_Tie;
        }
    }

    public void Betting_UpdateAmount_Banker()
    {
        Betting_Amount_Banker = 0;
        for (int i = 0; i < 7; i++)
        {
            if (Users[i] != null)
                Betting_Amount_Banker += Users[i].Betting_Amount_Banker;
        }
    }

    public void Betting_UpdateAmount_BPair()
    {
        Betting_Amount_BPair = 0;
        for (int i = 0; i < 7; i++)
        {
            if (Users[i] != null)
                Betting_Amount_BPair += Users[i].Betting_Amount_BPair;
        }
    }

    public void playSound(AudioClip clip)
    {
        musicPlayer.clip = clip;
        musicPlayer.Play();
    }

    public void PlayGameSound(AudioClip clip, float volume)
    {
        GameSoundPlayer.clip = clip;
        GameSoundPlayer.volume = volume;

        while (GameSoundPlayer.isPlaying);
   
        GameSoundPlayer.Play();
    }

}


