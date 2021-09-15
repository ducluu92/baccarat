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

public class ChinaBoard : MonoBehaviour
{
    public Sprite[] elementSprite = new Sprite[4];

    #region BigRoad
    //BigRoad 데이터 파싱하여 스프라이트 배열에 첨가.
    public void GenerateBigRoad(BigRoadMarker[,] board)
    {

        BakaraBoardFinder.Get<BigRoadMarker>(out var startX, board, 38);

        //startX부터 startX + size 까지의 인덱스로 새 배열을 정의한다.

        BigRoadMarker[,] sum = board;

        for (int i = 0; i < 38; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = board[startX + i, j];
            }
        }

        for (int i = 0; i < 38; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sum[i, j] != null)
                {
                    switch (sum[i, j].Winner)
                    {
                        case PacketBakaraWinner.Banker:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[sum[i,j].WinValue + 10];
                            break;
                        case PacketBakaraWinner.Player:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[sum[i, j].WinValue];
                            break;
                        case PacketBakaraWinner.Tie:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[sum[i, j].WinValue + 20];
                            break;
                        case PacketBakaraWinner.Undecided:
                            break;
                    }

                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[30];
                }
            }
        }

        /*
        var ori = board;

        var bankerCount = ori.Where(c => c.Winner == BakaraWinner.Banker).Count();
        var playerCount = ori.Where(c => c.Winner == BakaraWinner.Player).Count();
        var tieCount = ori.Where(c => c.Winner == BakaraWinner.Tie).Count();

        //var pPairCount = ori.Where(c => c.PlayerPair).Count();
        //var bPairCount = ori.Where(c => c.BankerPair).Count();
        */
    }
    #endregion

    #region MarkerRoad
    //BigRoad 데이터 파싱하여 스프라이트 배열에 첨가.
    public void GenerateMarkerRoad(BkFullResult[,] markerroad)
    {
        BakaraBoardFinder.Get<BkFullResult>(out var startX, markerroad, 14);

        //startX부터 startX + size 까지의 인덱스로 새 배열을 정의한다.

        BkFullResult[,] sum = markerroad;

        for (int i = 0; i < 14; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = markerroad[startX + i, j];
            }
        }


        for (int i = 0; i < 14; i++)
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
    #endregion

    #region BigEyeRoad
    //BigRoad 데이터 파싱하여 스프라이트 배열에 첨가.
    public void GenerateBigEyeRoad(bool?[,] BigEye)
    {
        BakaraBoardFinder.Get<bool?>(out var startX, BigEye, 60);

        bool?[,] sum = BigEye;

        for (int i = 0; i < 60; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = BigEye[startX + i, j];
            }
        }

        for (int i = 0; i < 60; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sum[i, j] != null)
                {
                    switch (sum[i, j])
                    {
                        case true:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[1];
                            break;

                        case false:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[0];
                            break;

                        default:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                            break;
                    }

                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                }
            }
        }
    }
    #endregion

    #region SmallEyeRoad
    //BigRoad 데이터 파싱하여 스프라이트 배열에 첨가.
    public void GenerateSmallRoad(bool?[,] SmallRoad)
    {
        BakaraBoardFinder.Get<bool?>(out var startX, SmallRoad, 28);

        bool?[,] sum = SmallRoad;

        for (int i = 0; i < 28; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = SmallRoad[startX + i, j];
            }
        }

        for (int i = 0; i < 28; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sum[i, j] != null)
                {
                    switch (sum[i, j])
                    {
                        case true:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[1];
                            break;

                        case false:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[0];
                            break;

                        default:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                            break;
                    }

                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                }
            }
        }
    }
    #endregion

    #region CockroachRoad
    //BigRoad 데이터 파싱하여 스프라이트 배열에 첨가.
    public void GenerateCockroachRoad(bool?[,] CockroachRoad)
    {
        BakaraBoardFinder.Get<bool?>(out var startX, CockroachRoad, 28);

        bool?[,] sum = CockroachRoad;

        for (int i = 0; i < 28; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                sum[i, j] = CockroachRoad[startX + i, j];
            }
        }

        for (int i = 0; i < 28; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (sum[i, j] != null)
                {
                    switch (sum[i, j])
                    {
                        case true:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[1];
                            break;

                        case false:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[0];
                            break;

                        default:
                            transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                            break;
                    }

                }
                else
                {
                    transform.GetChild(i).GetChild(j).GetComponent<Image>().sprite = elementSprite[2];
                }
            }
        }
    }
    #endregion




}
