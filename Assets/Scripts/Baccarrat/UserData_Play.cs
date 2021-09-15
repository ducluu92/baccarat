using Assets.Scripts.Baccarrat;
using Module.Packets.Betting;
using Module.Utils.Currency;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class UserData_Play : MonoBehaviour
{
    public Room CurrentRoom;

    public GameObject JoinButton;

    //플레이어가 베팅을 할 때 날아가는 칩 이펙트에 대한 스프라이트렌더러 컴포넌트.
    public SpriteRenderer Chip;
    // 칩 이펙트 스프라이트 컨테이너용 컴포넌트.
    public Sprite[] Chips;

    //플레이어가 배팅을 하고 Chip 스프라이트 렌더러 컴포넌트가 날아가고 나서 해당 위치에 칩이 쌓이게 되면서
    //플레이어의 보드별 배팅 현황에 대한 정보를 표시하는 칩컨테이너 객체.
    //PPair,Player,Tie,Banker,BPair에 대한 배팅 현황을 표시하기 위해 배열로 지정해놓았음.
    public ChipContainer[] ChipContainers;

    //베팅타입 인덱스를 통해 호출받아 칩컨테이너를 참조하기 위한 객체.
    public ChipContainer ChipContainer_Pointer;

    // 칩 이펙트 (16줄 참고)가 날아가고 나서, 칩컨테이너 (21줄 참고)가 업데이트 되기전 위치해야할 좌표를 포함하고 있는 객체. 
    //PPair,Player,Tie,Banker,BPair에 대한 베팅 위치를 저장하기 위해 배열로 지정해 놓았음.
    public Transform[] BettingPosition = new Transform[5];

    public int uuid; // UUID
    public string Nickname; //닉네임
    public int ProfileID; //프로필 사진 ID
    public long CurrentChips; //현재 소지금액
    public long CurrentBettings; //현재 베팅 총액
    public GameObject EmoticonUI; // 이모티콘
    public GameObject BettingSignBackground; // 신호등 알림 배경
    public GameObject[] BettingSign = new GameObject[5]; // 신호등 알림 아이템

    //UI 변수
    public Text UI_Nickname;
    public SpriteRenderer UI_Profile;
    public GameObject UI_Max;

    //라운드 중 유저 개인 베팅 총액
    public long Betting_Amount_PPair;
    public long Betting_Amount_Player;
    public long Betting_Amount_BPair;
    public long Betting_Amount_Banker;
    public long Betting_Amount_Tie;

    // 차감 금액 계산을 위한 총액
    public long Betting_Amount_PPair_Minus;
    public long Betting_Amount_Player_Minus;
    public long Betting_Amount_BPair_Minus;
    public long Betting_Amount_Banker_Minus;
    public long Betting_Amount_Tie_Minus;

    // 금액 증,가감 UI
    public GameObject WinnerMoneyUI;
    public GameObject LoseMoneyUI;
    public GameObject MoneyBackgroundUI;

    // 금액 증,가감 Anim
    public Animation WinnerMoneyUI_Anim;
    public Animation LoseMoneyUI_Anim;
    private bool IsMoneyAnimPlay = false;

    // 현재 플레이어의 배팅
    public BETTINGTYPE bettingtype = BETTINGTYPE.NONE;

    // 현재 플레이어의 MAX 배팅 여부 
    public bool IsMaxBetting = false;

    // 좌표계
    float x, y;

    //이모티콘 sprite
    private Sprite[] EmoticonSpriteList = new Sprite[7];



    void Start()
    {
        UpdateDisplays();
        foreach (var item in ChipContainers)
        {
            item.Initialize();
        }
        this.SelectChipContainer(0);
        EmoticonSpriteList = Resources.LoadAll<Sprite>("Sprites/Emoticon");
    }

    public void ChangePlayerBorder(Color color)
    {
        GetComponent<SpriteRenderer>().color = color;
    }

    public void TogglePlayerEffect(bool boolean)
    {
        foreach (var item in ChipContainers)
        {
            for(int i=0;i<4;i++)
            {
                item.PlayerEffect[i].SetActive(boolean);
            }
        }
    }

    public void AllChipClear()
    {
        foreach (var item in ChipContainers)
        {
            item.ClearAllChips(null);
        }
    }

    public void PlayerBettingSignalCheck(int index)
    {
        BettingSign[index - 1].SetActive(true);
    }

    public void PlayerBettingSignalClear()
    {
        for (int i = 0; i < 5; i++)
        {
            BettingSign[i].SetActive(false);
        }
    }

    public void ShowWinnerMoneyUI(long amount)
    {
        MoneyBackgroundUI.SetActive(true);
        WinnerMoneyUI.GetComponent<Text>().text = "+" + CurrencyConverter.Kor(amount);
        WinnerMoneyUI_Anim.Play();
        StartCoroutine(ChangeBackgroundStatus_Winner());
    }

    public void ShowLoseMoneyUI(long amount)
    {
        MoneyBackgroundUI.SetActive(true);
        LoseMoneyUI.GetComponent<Text>().text = "-" + CurrencyConverter.Kor(amount);
        LoseMoneyUI_Anim.Play();
        StartCoroutine(ChangeBackgroundStatus_Lose());
    }

    IEnumerator ChangeBackgroundStatus_Winner()
    {
        while (WinnerMoneyUI_Anim.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }

        MoneyBackgroundUI.SetActive(false);

        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator ChangeBackgroundStatus_Lose()
    {
        while (LoseMoneyUI_Anim.isPlaying)
        {
            yield return new WaitForSeconds(0.1f);
        }

        MoneyBackgroundUI.SetActive(false);

        yield return new WaitForSeconds(0.1f);
    }

    public void ShowEmoticon(int index)
    {
        EmoticonUI.SetActive(false);

        EmoticonUI.GetComponent<SpriteRenderer>().sprite = EmoticonSpriteList[index];

        var anim = EmoticonUI.GetComponent<Animation>();

        if (anim.isPlaying)
        {
            anim.Stop();
        }

        EmoticonUI.SetActive(true);
        anim.Play();
    }

    public void SelectChipContainer(int index)
    {
        ChipContainer_Pointer = ChipContainers[index];
    }

    public void Betting_Display_Bet(Vector2 position)
    {
        //Chip.sprite = Resources.Load("Sprites/Chip_Player_1", typeof(Sprite)) as Sprite;

        if (ChipContainer_Pointer.ChipContainer_1000.localPosition == Vector3.zero)
        {
            ChipContainer_Pointer.ChipContainer_1000.position = new Vector3(position.x, position.y, 0);
            ChipContainer_Pointer.ChipContainer_10000.position = new Vector3(position.x + 0.35f, position.y, 0);
            ChipContainer_Pointer.ChipContainer_100000.position = new Vector3(position.x, position.y - 0.17f, 0);
            ChipContainer_Pointer.ChipContainer_1000000.position = new Vector3(position.x + 0.35f, position.y - 0.17f, 0);
        }

        StartCoroutine(Betting_Display_Bet_co());
        //Chip.transform.position = Vector3.Lerp(Chip.transform.position, ChipContainer.position, Time.deltaTime);

    }

    public void Betting_Display_Bet_withoutChip(float minx, float maxx, float miny, float maxy, BETTINGTYPE type)
    {
        //Chip.sprite = Resources.Load("Sprites/Chip_Player_1", typeof(Sprite)) as Sprite;

        if (ChipContainer_Pointer.ChipContainer_1000.localPosition == Vector3.zero)
        {
            x = Random.Range(minx, maxx);
            y = Random.Range(miny, maxy);

            ChipContainer_Pointer.ChipContainer_1000.position = new Vector3(x, y, 0);
            ChipContainer_Pointer.ChipContainer_10000.position = new Vector3(x + 0.35f, y, 0);
            ChipContainer_Pointer.ChipContainer_100000.position = new Vector3(x, y - 0.17f, 0);
            ChipContainer_Pointer.ChipContainer_1000000.position = new Vector3(x + 0.35f, y - 0.17f, 0);
        }

        StartCoroutine(Betting_Display_Bet_withoutChip_co(type));
        //Chip.transform.position = Vector3.Lerp(Chip.transform.position, ChipContainer.position, Time.deltaTime);

    }



    IEnumerator Betting_Display_Bet_co()
    {
        Chip.gameObject.SetActive(true);
        while (Vector2.Distance(Chip.transform.position, ChipContainer_Pointer.ChipContainer_1000.position) > 0.04f)
        {
            Chip.transform.position = Vector3.Lerp(Chip.transform.position, ChipContainer_Pointer.ChipContainer_1000.position, Time.deltaTime * 15);
            yield return new WaitForSeconds(0.000001f);
        }
        Chip.gameObject.SetActive(false);
        ChipContainer_Pointer.ChipContainer_1000.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.000001f);
        Chip.transform.localPosition = Vector3.zero;

        UpdateChipContainer(this.bettingtype);
    }

    IEnumerator Betting_Display_Bet_withoutChip_co(BETTINGTYPE type)
    {
        ChipContainer_Pointer.ChipContainer_1000.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.000001f);
        UpdateChipContainer(type);
    }

    public void UpdateDisplays()
    {
        if (this.ProfileID == 0)
        {
            UI_Profile.gameObject.SetActive(false);
            UI_Nickname.gameObject.SetActive(false);
            JoinButton.SetActive(true);
        }
        else
        {
            JoinButton.SetActive(false);

            Object[] Profiles;
            Profiles = Resources.LoadAll<Sprite>("Sprites/Sprites_Profile");
            UI_Profile.sprite = (Sprite)Profiles[1];
            UI_Nickname.text = this.Nickname;

            UI_Profile.gameObject.SetActive(true);
            UI_Nickname.gameObject.SetActive(true);
        }

        if (UserInfoManager.isGamePlay)
        {
            JoinButton.SetActive(false);
        }
    }

    IEnumerator UpdateChipContainer_co(BETTINGTYPE type)
    {
        long bettings = 0;


        switch (type)
        {
            case BETTINGTYPE.BPAIR:
                bettings = this.Betting_Amount_BPair;
                break;
            case BETTINGTYPE.BANKER:
                bettings = this.Betting_Amount_Banker;
                break;
            case BETTINGTYPE.TIE:
                bettings = this.Betting_Amount_Tie;
                break;
            case BETTINGTYPE.PLAYER:
                bettings = this.Betting_Amount_Player;
                break;
            case BETTINGTYPE.PPAIR:
                bettings = this.Betting_Amount_PPair;
                break;
            case BETTINGTYPE.NONE:
                bettings = this.CurrentBettings;
                break;

        }

        bettings /= 1000;

        if (bettings % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                ChipContainer_Pointer.ChipContainer_chips_1000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (bettings % 10 >= i)
                    ChipContainer_Pointer.ChipContainer_chips_1000[i - 1].SetActive(true);
                else
                    ChipContainer_Pointer.ChipContainer_chips_1000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }


        bettings /= 10;

        if (bettings % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                ChipContainer_Pointer.ChipContainer_chips_10000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (bettings % 10 >= i)
                    ChipContainer_Pointer.ChipContainer_chips_10000[i - 1].SetActive(true);
                else
                    ChipContainer_Pointer.ChipContainer_chips_10000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }


        bettings /= 10;

        if (bettings % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                ChipContainer_Pointer.ChipContainer_chips_100000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (bettings % 10 >= i)
                    ChipContainer_Pointer.ChipContainer_chips_100000[i - 1].SetActive(true);
                else
                    ChipContainer_Pointer.ChipContainer_chips_100000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }

        bettings /= 10;

        if (bettings % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                ChipContainer_Pointer.ChipContainer_chips_1000000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (bettings % 10 >= i)
                    ChipContainer_Pointer.ChipContainer_chips_1000000[i - 1].SetActive(true);
                else
                    ChipContainer_Pointer.ChipContainer_chips_1000000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }
    }

    public void UpdateChipContainer(BETTINGTYPE type)
    {
        StartCoroutine(UpdateChipContainer_co(type));
    }

    //칩을 베팅하는 함수 (수량 포함)



    public void Betting_Bet_PPair_AllIn(Vector3 position)
    {
        StartCoroutine(Betting_Bet_PPair_AllIn_co(position));
    }

    IEnumerator Betting_Bet_PPair_AllIn_co(Vector3 position)
    {
        if (ChipContainer_Pointer.GoldChip.transform.localPosition == Vector3.zero)
        {

            while (Vector2.Distance(ChipContainer_Pointer.GoldChip.transform.position, position) > 0.04f)
            {
                ChipContainer_Pointer.GoldChip.transform.position = Vector3.Lerp(ChipContainer_Pointer.GoldChip.transform.position, position, Time.deltaTime * 15);
                yield return new WaitForSeconds(0.000001f);
            }

            UI_Max.SetActive(true);

            foreach (var MaxFireWork in CurrentRoom.MaxFireWorks)
                MaxFireWork.SetActive(true);

            while (ChipContainer_Pointer.ChipContainer_1000.localPosition != Vector3.zero)
                yield return new WaitForSeconds(0.000001f);
        }
    }

    public void Betting_Bet_withoutChip(long amount, BETTINGTYPE type)
    {
        long changer;
        var managerInterceptor = RoomDataManager.Instance();

        switch (type)
        {
            case BETTINGTYPE.PPAIR:
                changer = this.Betting_Amount_PPair;
                break;
            case BETTINGTYPE.PLAYER:
                changer = this.Betting_Amount_Player;
                break;
            case BETTINGTYPE.TIE:
                changer = this.Betting_Amount_Tie;
                break;
            case BETTINGTYPE.BANKER:
                changer = this.Betting_Amount_Banker;
                break;
            case BETTINGTYPE.BPAIR:
                changer = this.Betting_Amount_BPair;
                break;
            default:
                changer = 0;
                break;
        }

        Debug.Log("managerInterceptor.MaxBetting => " + managerInterceptor.MaxBetting);

        if (((this.CurrentChips - amount) >= managerInterceptor.MinBetting) && ((changer + amount) <= (managerInterceptor.MaxBetting - 1000)))
        {
            switch (amount)
            {
                case 1000:
                    Chip.sprite = Chips[0];
                    break;
                case 10000:
                    Chip.sprite = Chips[1];
                    break;
                case 100000:
                    Chip.sprite = Chips[2];
                    break;
                case 1000000:
                    Chip.sprite = Chips[3];
                    break;
            }

            if (type != BETTINGTYPE.NONE && BettingPosition[((int)type) + -1] != null)
                Betting_Display_Bet(this.BettingPosition[((int)type) + -1].position);

            this.CurrentChips = (this.CurrentChips - amount);
            this.CurrentBettings = (this.CurrentBettings + amount);
            UpdateDisplays();

            switch (type)
            {
                case BETTINGTYPE.PPAIR:
                    this.Betting_Amount_PPair += amount;
                    this.Betting_Amount_PPair_Minus += amount;
                    break;
                case BETTINGTYPE.PLAYER:
                    this.Betting_Amount_Player += amount;
                    this.Betting_Amount_Player_Minus += amount;
                    break;
                case BETTINGTYPE.TIE:
                    this.Betting_Amount_Tie += amount;
                    this.Betting_Amount_Tie_Minus += amount;
                    break;
                case BETTINGTYPE.BANKER:
                    this.Betting_Amount_Banker += amount;
                    this.Betting_Amount_Banker_Minus += amount;
                    break;
                case BETTINGTYPE.BPAIR:
                    this.Betting_Amount_BPair += amount;
                    this.Betting_Amount_BPair_Minus += amount;
                    break;
            }
        }

        if (this.CurrentChips == 0)
            StartCoroutine(Betting_Bet_PPair_AllIn_co(this.BettingPosition[((int)type) + -1].position));
    }

    public void Betting_Bet(long amount, BETTINGTYPE type)
    {
        long changer;
        var managerInterceptor = RoomDataManager.Instance();

        switch (type)
        {
            case BETTINGTYPE.PPAIR:
                changer = this.Betting_Amount_PPair;
                break;
            case BETTINGTYPE.PLAYER:
                changer = this.Betting_Amount_Player;
                break;
            case BETTINGTYPE.TIE:
                changer = this.Betting_Amount_Tie;
                break;
            case BETTINGTYPE.BANKER:
                changer = this.Betting_Amount_Banker;
                break;
            case BETTINGTYPE.BPAIR:
                changer = this.Betting_Amount_BPair;
                break;
            default:
                changer = 0;
                break;
        }

        if (((this.CurrentChips - amount) >= managerInterceptor.MinBetting) && ((changer + amount) <= (managerInterceptor.MaxBetting - 1000)))
        {
            switch (amount)
            {
                case 1000:
                    Chip.sprite = Chips[0];
                    break;
                case 10000:
                    Chip.sprite = Chips[1];
                    break;
                case 100000:
                    Chip.sprite = Chips[2];
                    break;
                case 1000000:
                    Chip.sprite = Chips[3];
                    break;
            }

            if (type != BETTINGTYPE.NONE)
                Betting_Display_Bet(this.BettingPosition[((int)type) + -1].position);

            this.CurrentChips = (this.CurrentChips - amount);
            this.CurrentBettings = (this.CurrentBettings + amount);
            UpdateDisplays();

            switch (type)
            {
                case BETTINGTYPE.PPAIR:
                    this.Betting_Amount_PPair += amount;
                    this.Betting_Amount_PPair_Minus += amount;
                    break;
                case BETTINGTYPE.PLAYER:
                    this.Betting_Amount_Player += amount;
                    this.Betting_Amount_Player_Minus += amount;
                    break;
                case BETTINGTYPE.TIE:
                    this.Betting_Amount_Tie += amount;
                    this.Betting_Amount_Tie_Minus += amount;
                    break;
                case BETTINGTYPE.BANKER:
                    this.Betting_Amount_Banker += amount;
                    this.Betting_Amount_Banker_Minus += amount;
                    break;
                case BETTINGTYPE.BPAIR:
                    this.Betting_Amount_BPair += amount;
                    this.Betting_Amount_BPair_Minus += amount;
                    break;
            }
        }
    }

    // MAX BETTING
    public void Betting_MAX_Bet(long amount, BETTINGTYPE type)
    {
        IsMaxBetting = true;

        this.CurrentChips = (this.CurrentChips - amount);
        this.CurrentBettings = (this.CurrentBettings + amount);
        UpdateDisplays();

        switch (type)
        {
            case BETTINGTYPE.PPAIR:
                this.Betting_Amount_PPair += amount;
                this.Betting_Amount_PPair_Minus += amount;
                break;
            case BETTINGTYPE.PLAYER:
                this.Betting_Amount_Player += amount;
                this.Betting_Amount_Player_Minus += amount;
                break;
            case BETTINGTYPE.TIE:
                this.Betting_Amount_Tie += amount;
                this.Betting_Amount_Tie_Minus += amount;
                break;
            case BETTINGTYPE.BANKER:
                this.Betting_Amount_Banker += amount;
                this.Betting_Amount_Banker_Minus += amount;
                break;
            case BETTINGTYPE.BPAIR:
                this.Betting_Amount_BPair += amount;
                this.Betting_Amount_BPair_Minus += amount;
                break;
        }

        StartCoroutine(Betting_Bet_PPair_AllIn_co(this.BettingPosition[((int)type) + -1].position));
    }

    //칩을 회수하는 함수
    public void Betting_Bet_Revert()
    {
        StartCoroutine(Betting_Bet_Revert_co());
    }

    // 배팅 취소 버튼으로 취소 했을 때 사용하는 함수
    public void Betting_Bet_Revert_Click()
    {
        StartCoroutine(Betting_Bet_Revert_Click_co());
    }

    public void Betting_Bet_Revert_PPair()
    {
        StartCoroutine(Betting_Bet_Revert_PPair_co());
    }

    IEnumerator Betting_Bet_Revert_PPair_co()
    {
        Betting_Amount_PPair = (Betting_Amount_PPair - this.Betting_Amount_PPair);

        this.CurrentChips += this.Betting_Amount_PPair;

        this.CurrentBettings -= this.Betting_Amount_PPair;

        this.Betting_Amount_PPair = 0;

        this.Betting_Amount_PPair = CurrentChips;
        this.CurrentBettings = (this.CurrentBettings + CurrentChips);

        this.Betting_Amount_PPair = CurrentChips;
        this.CurrentChips = 0;

        UpdateDisplays();

        while (ChipContainer_Pointer.ChipContainer_1000.localPosition != Vector3.zero)
        {
            ChipContainer_Pointer.ChipContainer_1000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_1000.localPosition, Vector3.zero, Time.deltaTime * 15); //new Vector3(x, y, 0);
            ChipContainer_Pointer.ChipContainer_10000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_10000.localPosition, Vector3.zero, Time.deltaTime * 15); //new Vector3(x, y, 0);
            ChipContainer_Pointer.ChipContainer_100000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_100000.localPosition, Vector3.zero, Time.deltaTime * 15); //new Vector3(x, y, 0);
            ChipContainer_Pointer.ChipContainer_1000000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_1000000.localPosition, Vector3.zero, Time.deltaTime * 15); //new Vector3(x, y, 0);
            yield return new WaitForSeconds(0.000001f);
        }

        StartCoroutine(UpdateChipContainer_co(BETTINGTYPE.NONE));

    }

    IEnumerator Betting_Bet_Revert_co()
    {
        long ResultLoseShowCalc = 0L;
        long ResultWinShowCalc = 0L;

        ResultLoseShowCalc += this.Betting_Amount_Banker_Minus;
        ResultLoseShowCalc += this.Betting_Amount_Player_Minus;
        ResultLoseShowCalc += this.Betting_Amount_Tie_Minus;
        ResultLoseShowCalc += this.Betting_Amount_PPair_Minus;
        ResultLoseShowCalc += this.Betting_Amount_BPair_Minus;

        ResultWinShowCalc += this.Betting_Amount_Banker;
        ResultWinShowCalc += this.Betting_Amount_Player;
        ResultWinShowCalc += this.Betting_Amount_Tie;
        ResultWinShowCalc += this.Betting_Amount_PPair;
        ResultWinShowCalc += this.Betting_Amount_BPair;

        this.CurrentChips += this.Betting_Amount_Banker;
        this.CurrentChips += this.Betting_Amount_Player;
        this.CurrentChips += this.Betting_Amount_Tie;
        this.CurrentChips += this.Betting_Amount_PPair;
        this.CurrentChips += this.Betting_Amount_BPair;

        if (ResultWinShowCalc > 0)
        {
            ShowWinnerMoneyUI(ResultWinShowCalc);
        }
        else if(ResultLoseShowCalc > 0)
        {
            ShowLoseMoneyUI(ResultLoseShowCalc);
        }

        this.Betting_Amount_Banker = 0;
        this.Betting_Amount_Player = 0;
        this.Betting_Amount_Tie = 0;
        this.Betting_Amount_PPair = 0;
        this.Betting_Amount_BPair = 0;

        this.Betting_Amount_Banker_Minus = 0;
        this.Betting_Amount_Player_Minus = 0;
        this.Betting_Amount_Tie_Minus = 0;
        this.Betting_Amount_PPair_Minus = 0;
        this.Betting_Amount_BPair_Minus = 0;

        this.CurrentBettings = 0;

        this.CurrentRoom.Betting_UpdateAmount_Banker();
        this.CurrentRoom.Betting_UpdateAmount_BPair();
        this.CurrentRoom.Betting_UpdateAmount_Tie();
        this.CurrentRoom.Betting_UpdateAmount_Player();
        this.CurrentRoom.Betting_UpdateAmount_PPair();

        UpdateDisplays();

        for (int i = 0; i < 5; i++)
        {
            this.SelectChipContainer(i);


            while (Vector3.Distance(ChipContainer_Pointer.GoldChip.transform.localPosition,Vector3.zero) >= 0.3f)
            {
                ChipContainer_Pointer.GoldChip.transform.localPosition = Vector3.Lerp(ChipContainer_Pointer.GoldChip.transform.localPosition, Vector3.zero, Time.deltaTime * 25);
                yield return new WaitForSeconds(0.000001f);
            }

            ChipContainer_Pointer.GoldChip.transform.localPosition = Vector3.zero;

            UI_Max.SetActive(false);

            while (Vector3.Distance(ChipContainer_Pointer.ChipContainer_1000.localPosition, Vector3.zero) >= 0.3f)
            {
                ChipContainer_Pointer.ChipContainer_1000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_1000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                ChipContainer_Pointer.ChipContainer_10000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_10000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                ChipContainer_Pointer.ChipContainer_100000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_100000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                ChipContainer_Pointer.ChipContainer_1000000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_1000000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                yield return new WaitForSeconds(0.000001f);
            }

            ChipContainer_Pointer.ChipContainer_1000.localPosition = Vector3.zero;
            ChipContainer_Pointer.ChipContainer_10000.localPosition = Vector3.zero;
            ChipContainer_Pointer.ChipContainer_100000.localPosition = Vector3.zero;
            ChipContainer_Pointer.ChipContainer_1000000.localPosition = Vector3.zero;

        }

        StartCoroutine(UpdateChipContainer_co(BETTINGTYPE.NONE));

    }

    IEnumerator Betting_Bet_Revert_Click_co()
    {
        this.CurrentChips += this.Betting_Amount_Banker;
        this.CurrentChips += this.Betting_Amount_Player;
        this.CurrentChips += this.Betting_Amount_Tie;
        this.CurrentChips += this.Betting_Amount_PPair;
        this.CurrentChips += this.Betting_Amount_BPair;

        this.Betting_Amount_Banker = 0;
        this.Betting_Amount_Player = 0;
        this.Betting_Amount_Tie = 0;
        this.Betting_Amount_PPair = 0;
        this.Betting_Amount_BPair = 0;

        this.Betting_Amount_Banker_Minus = 0;
        this.Betting_Amount_Player_Minus = 0;
        this.Betting_Amount_Tie_Minus = 0;
        this.Betting_Amount_PPair_Minus = 0;
        this.Betting_Amount_BPair_Minus = 0;

        this.CurrentBettings = 0;

        this.CurrentRoom.Betting_UpdateAmount_Banker();
        this.CurrentRoom.Betting_UpdateAmount_BPair();
        this.CurrentRoom.Betting_UpdateAmount_Tie();
        this.CurrentRoom.Betting_UpdateAmount_Player();
        this.CurrentRoom.Betting_UpdateAmount_PPair();

        UpdateDisplays();

        for (int i = 0; i < 5; i++)
        {
            this.SelectChipContainer(i);


            while (Vector3.Distance(ChipContainer_Pointer.GoldChip.transform.localPosition, Vector3.zero) >= 0.3f)
            {
                ChipContainer_Pointer.GoldChip.transform.localPosition = Vector3.Lerp(ChipContainer_Pointer.GoldChip.transform.localPosition, Vector3.zero, Time.deltaTime * 25);
                yield return new WaitForSeconds(0.000001f);
            }

            ChipContainer_Pointer.GoldChip.transform.localPosition = Vector3.zero;

            UI_Max.SetActive(false);

            while (Vector3.Distance(ChipContainer_Pointer.ChipContainer_1000.localPosition, Vector3.zero) >= 0.3f)
            {
                ChipContainer_Pointer.ChipContainer_1000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_1000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                ChipContainer_Pointer.ChipContainer_10000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_10000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                ChipContainer_Pointer.ChipContainer_100000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_100000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                ChipContainer_Pointer.ChipContainer_1000000.localPosition = Vector3.Lerp(ChipContainer_Pointer.ChipContainer_1000000.localPosition, Vector3.zero, Time.deltaTime * 25); //new Vector3(x, y, 0);
                yield return new WaitForSeconds(0.000001f);
            }

            ChipContainer_Pointer.ChipContainer_1000.localPosition = Vector3.zero;
            ChipContainer_Pointer.ChipContainer_10000.localPosition = Vector3.zero;
            ChipContainer_Pointer.ChipContainer_100000.localPosition = Vector3.zero;
            ChipContainer_Pointer.ChipContainer_1000000.localPosition = Vector3.zero;

        }

        AllChipClear();

        StartCoroutine(UpdateChipContainer_co(BETTINGTYPE.NONE));

    }
}
