using Assets.Scripts.Network.Bakaras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Baccarrat
{
    public class RoomDataManager
    {
        UnityEngine.Object[] Cardsprites;
        
        public int Info_LimitToJoin; // 입장 제한액
        public int Info_BettingMin; // 베팅 최소 금액
        public int Info_BettingMax; // 베팅 최대 금액
        public short Info_CurrentRound; // 현재 라운드
        public short Info_CurrentCard; // 현재 카드 수

        // 누적 승 수
        public short Info_Wins_PPair;
        public short Info_Wins_Player;
        public short Info_Wins_BPair;
        public short Info_Wins_Banker;
        public short Info_Wins_Tie;

        // Min/Max Betting
        public int MinBetting { get; set; } = 0;
        public int MaxBetting { get; set; } = 0;


        //카드ID 배열. 
        //0 : 카드 값 없음.
        // ( 1~10 : 클로버 숫자카드 / 11~13 : 클로버 J,Q,K / 14~23 : 스페이드 숫자카드 / 24~26 : 스페이드 J,Q,K)
        // ( 27~36 : 다이아 숫자카드 / 37~39 : 다이아 J,Q,K / 40 ~ 49 : 하트 숫자카드 / 50 ~ 52 : 하트 J,Q,K)
        public int[] CardID_Player = new int[3];
        public int[] CardID_Banker = new int[3];

        public int[] CardValue_Player = new int[3];
        public int[] CardValue_Banker = new int[3];

        public bool isPlayerPair = false;
        public bool isBankerPair = false;

        public BakaraWinner winner = BakaraWinner.Undecided;

        public bool isPlayerNatural = false;
        public bool isBankerNatural = false;

        public bool isLastGame = false;

        public int BettingTime = 0;

        public int RandomSeed { get; set; } = 0;

        public int RandomSubSeed { get; set; } = 0;


        public bool isLoading { get; set; } = false;

        private static RoomDataManager instance = null;

        public Sprite[] CardFront = new Sprite[6];

        private RoomDataManager() { 
            Cardsprites = Resources.LoadAll<Sprite>("Sprites/Sprites_Card"); 
        }

        public void Reset()
        {
            CardID_Player = new int[3];
            
            for (int i = 0; i < 3; i++)
                CardID_Player[i] = 0;

            CardID_Banker = new int[3];
            
            for (int i = 0; i < 3; i++)
                CardID_Banker[i] = 0;

            CardValue_Player = new int[3];
            for (int i = 0; i < 3; i++)
                CardValue_Player[i] = -1;

            CardValue_Banker = new int[3];
            for (int i = 0; i < 3; i++)
                CardValue_Banker[i] = -1;

            winner = BakaraWinner.Undecided;
            
            isPlayerNatural = false;
            isBankerNatural = false;

            CardFront = new Sprite[6];
        }

        public Sprite[] UpdateCardInfo()
        {
            CardFront[0] = (Sprite)Cardsprites[CardID_Player[0]];
            CardFront[1] = (Sprite)Cardsprites[CardID_Banker[0]];
            CardFront[2] = (Sprite)Cardsprites[CardID_Player[1]];
            CardFront[3] = (Sprite)Cardsprites[CardID_Banker[1]];
            CardFront[4] = (Sprite)Cardsprites[CardID_Player[2]];
            CardFront[5] = (Sprite)Cardsprites[CardID_Banker[2]];

            return CardFront;
        }

        public void showDatas()
        {
            if (false) 
            {
                Debug.Log("=============== SHOW ROOM-DATA ===============");

                Debug.Log("00. CardID_Player=> " + CardID_Player);
                Debug.Log("01. CardID_Banker=> " + CardID_Banker);

                Debug.Log("02. CardValue_Player=> " + CardValue_Player);
                Debug.Log("03. CardValue_Banker=> " + CardValue_Banker);

                Debug.Log("04. winner=> " + winner);
                Debug.Log("05. isPlayerNatural=> " + isPlayerNatural);
                Debug.Log("06. isBankerNatural=> " + isBankerNatural);
                Debug.Log("07. CardFront=> " + CardFront);

                Debug.Log("=============== END ROOM-DATA ===============");
            }
        }

        public static RoomDataManager Instance()
        {
            if(instance == null)
            {
                instance = new RoomDataManager();
            }
            return instance;
        }
    }
}
