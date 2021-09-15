using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Module.Apis;
using Module.Apis.Rooms;
using Newtonsoft.Json;
using UnityEngine.Networking;
using Module.Apis.Bakaras;
using UnityEngine.UI;
using Module.Apis.ApiDefinition;
using System.Linq;
using Assets.Scripts.Baccarrat;
using Module.Apis.Bakaras.Markers;

public class ChinaSimpleBoard : MonoBehaviour
{
    public Sprite[] elementSprite = new Sprite[4];

    public Text Player, Banker, Tie, pPair, bPair;

    //가로 20 세로 6 크기를 준수하는 가변적 크기의 심플 차이나 보드 요소 형성.
    //public Image[,] elements = new Image[6,20];

    public void GenerateSimpleBoard(BakaraSimpleBoard board)
    {
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (board.BigRoad[i, j] != null)
                {
                    switch (board.BigRoad[i,j].Winner)
                    {
                        case PacketBakaraWinner.Banker:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[1];
                            break;
                        case PacketBakaraWinner.Player:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[0];
                            break;
                        case PacketBakaraWinner.Tie:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                            break;
                        case PacketBakaraWinner.Undecided:
                            break;
                    }
                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[3];
                }
            }
        }

        var ori = board.Origin;

        
        var bankerCount = ori.Where(c => c.Winner == PacketBakaraWinner.Banker).Count();
        Banker.text = bankerCount.ToString();

        var playerCount = ori.Where(c => c.Winner == PacketBakaraWinner.Player).Count();
        Player.text = playerCount.ToString();

        var tieCount = ori.Where(c => c.Winner == PacketBakaraWinner.Tie).Count();
        Tie.text = tieCount.ToString();

        var pPairCount = ori.Where(c => c.PlayerPair).Count();
        pPair.text = pPairCount.ToString();

        var bPairCount = ori.Where(c => c.BankerPair).Count();
        bPair.text = bPairCount.ToString();




    }

    public void GenerateSimpleBoard_Ingame(BigRoadMarker[,] board, List<BkFullResult> origin)
    {
        BakaraBoardFinder.Get<BigRoadMarker>(out var startX, board, 20);

        BigRoadMarker[,] sum = board;

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = board[startX + i, j];
            }
        }


        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sum[i, j] != null)
                {
                    switch (sum[i, j].Winner)
                    {
                        case PacketBakaraWinner.Banker:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[1];
                            break;
                        case PacketBakaraWinner.Player:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[0];
                            break;
                        case PacketBakaraWinner.Tie:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                            break;
                        case PacketBakaraWinner.Undecided:
                            break;
                    }
                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[3];
                }
            }
        }

        var bankerCount = origin.Where(c => c.Winner == PacketBakaraWinner.Banker).Count();
        Banker.text = bankerCount.ToString();

        var playerCount = origin.Where(c => c.Winner == PacketBakaraWinner.Player).Count();
        Player.text = playerCount.ToString();

        var tieCount = origin.Where(c => c.Winner == PacketBakaraWinner.Tie).Count();
        Tie.text = tieCount.ToString();

        var pPairCount = origin.Where(c => c.IsPlayerPair == true).Count();
        pPair.text = pPairCount.ToString();

        var bPairCount = origin.Where(c => c.IsBankerPair == true).Count();
        bPair.text = bPairCount.ToString();




    }

    public void GenerateSimpleMarkerBoard_Ingame(BkFullResult[,] markerroad)
    {
        BakaraBoardFinder.Get<BkFullResult>(out var startX, markerroad, 20);

        //startX부터 startX + size 까지의 인덱스로 새 배열을 정의한다.

        BkFullResult[,] sum = markerroad;

        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = markerroad[startX + i, j];
            }
        }


        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sum[i, j] != null)
                {
                    switch (sum[i, j].Winner)
                    {
                        case PacketBakaraWinner.Banker:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[1];
                            break;
                        case PacketBakaraWinner.Player:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[0];
                            break;
                        case PacketBakaraWinner.Tie:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                            break;
                        case PacketBakaraWinner.Undecided:
                            break;
                    }

                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[3];
                }
            }
        }
    }

}
