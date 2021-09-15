using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Settings
{
    class UISendMessage : MonoBehaviour
    {
        public Text Header;
        public Text Body;
        public Dropdown Type;

        public KLNetwork_Lobby networkhandler;

        public void SendMessage()
        {
            networkhandler.SendMessageRequest(Header.text, Body.text, Type.value);
            //Debug.Log(Type.value);

        }

    }
}
