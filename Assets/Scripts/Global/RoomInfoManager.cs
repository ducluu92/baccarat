using Module.Apis.Rooms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public class RoomInfoManager : MonoBehaviour
    {
        public static RoomListResponce roomdata;

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
