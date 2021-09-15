using Module.Apis.Infos;
using Module.Apis.Logins;
using Module.Apis.Rooms;
using System.Collections.Generic;
using UnityEngine;

public class UserInfoManager : MonoBehaviour
{
    public static AccountInfoResponce AccountInfo { get; set; } = new AccountInfoResponce();

    public static LoginResponce loginInfo;
    public static int RoomIdx;

    public static bool isGamePlay;

    public static int SelectedIdx;

    public static List<RatingData> ratingDataList { get; set; }
    
    public static RatingData selectedRating { get; set; }


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static void Clear()
    {
        AccountInfo = null;
        loginInfo = null;
        RoomIdx = 0;
        isGamePlay = false;
        SelectedIdx = 0;
    }

    public static void ClearToRoomInfo()
    {
        RoomIdx = 0;
        isGamePlay = false;
        SelectedIdx = 0;
    }
}