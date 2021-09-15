using Module.Apis.ApiDefinition;
using Module.Apis.Bakaras;
using Module.Apis.Bakaras.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Scripts.Baccarrat
{
    public class UIBaccaratRoom : MonoBehaviour
    {
        public ChinaBoard BigRoad;
        public ChinaBoard MarkerRoad;
        public ChinaBoard BigEye;
        public ChinaBoard SmallRoad;
        public ChinaBoard Cockroach;

        public Text PlayerScore;
        public Text BankerScore;
        public Text TieScore;
        public Text PPairScore;
        public Text BPairScore;

        public GameObject[] PlayerNext = new GameObject[3];
        public GameObject[] BankerNext = new GameObject[3];

        public Sprite[] NextSprites;

        public void GenerateScore(List<BkFullResult> origin)
        {
            var bankerCount = origin.Where(c => c.Winner == PacketBakaraWinner.Banker).Count();
            var playerCount = origin.Where(c => c.Winner == PacketBakaraWinner.Player).Count();
            var tieCount = origin.Where(c => c.Winner == PacketBakaraWinner.Tie).Count();
            var pPairCount = origin.Where(c => c.IsPlayerPair == true).Count();
            var bPairCount = origin.Where(c => c.IsBankerPair == true).Count();

            var TotalCount = bankerCount + playerCount + tieCount + pPairCount + bPairCount;
            
            double playerPercent = Math.Truncate((((double)playerCount / (double)TotalCount) * 10000)) / 100 ;
            double BankerPercent = Math.Truncate((((double)bankerCount / (double)TotalCount) * 10000)) / 100;
            double TiePercent = Math.Truncate((((double)tieCount / (double)TotalCount) * 10000)) / 100;
            double pPairPercent = Math.Truncate((((double)pPairCount / (double)TotalCount) * 10000)) / 100;
            double bPairPercent = Math.Truncate((((double)bPairCount / (double)TotalCount) * 10000)) / 100;

            PlayerScore.text = String.Format("Player[{0}]\n<color=#FFFFFF>({1}%)</color>",playerCount, playerPercent);
            BankerScore.text = String.Format("Banker[{0}]\n<color=#FFFFFF>({1}%)</color>", bankerCount, BankerPercent);
            TieScore.text = String.Format("Tie[{0}]\n<color=#FFFFFF>({1}%)</color>", tieCount, TiePercent);
            PPairScore.text = String.Format("P.Pair[{0}]\n<color=#FFFFFF>({1}%)</color>", pPairCount, pPairPercent);
            BPairScore.text = String.Format("B.Pair[{0}]\n<color=#FFFFFF>({1}%)</color>", bPairCount, bPairPercent);
        }

        public void SetPlayerNext(ChinaNext next)
        {
            for(int i=0;i<3;i++)
                PlayerNext[i].SetActive(true);

            if(!next.BigEyeRoad.HasValue)
            {
                PlayerNext[0].SetActive(false);
            }
            else
            {
                bool x = next.BigEyeRoad.Value;
                if(x)
                {
                    PlayerNext[0].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[0];
                }
                else
                {
                    PlayerNext[0].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[3];
                }
            }

            if (!next.SmallRoad.HasValue)
            {
                PlayerNext[1].SetActive(false);
            }
            else
            {
                bool x = next.SmallRoad.Value;
                if (x)
                {
                    PlayerNext[1].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[1];
                }
                else
                {
                    PlayerNext[1].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[4];
                }
            }

            if (!next.CockroachRoad.HasValue)
            {
                PlayerNext[2].SetActive(false);
            }
            else
            {
                bool x = next.CockroachRoad.Value;
                if (x)
                {
                    PlayerNext[2].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[2];
                }
                else
                {
                    PlayerNext[2].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[5];
                }
            }
        }

        public void SetBankerNext(ChinaNext next)
        {
            for (int i = 0; i < 3; i++)
                BankerNext[i].SetActive(true);

            if (next.BigEyeRoad == null)
            {
                BankerNext[0].SetActive(false);
            }
            else
            {
                bool x = (bool)next.BigEyeRoad;
                if (x)
                {
                    BankerNext[0].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[0];
                }
                else
                {
                    BankerNext[0].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[3];
                }
            }

            if (next.SmallRoad == null)
            {
                BankerNext[1].SetActive(false);
            }
            else
            {
                bool x = (bool)next.SmallRoad;
                if (x)
                {
                    BankerNext[1].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[1];
                }
                else
                {
                    BankerNext[1].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[4];
                }
            }

            if (next.CockroachRoad == null)
            {
                BankerNext[2].SetActive(false);
            }
            else
            {
                bool x = (bool)next.CockroachRoad;
                if (x)
                {
                    BankerNext[2].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[2];
                }
                else
                {
                    BankerNext[2].gameObject.GetComponent<SpriteRenderer>().sprite = NextSprites[5];
                }
            }
        }

    }
}
