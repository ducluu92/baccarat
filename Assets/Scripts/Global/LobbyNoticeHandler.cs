using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyNoticeHandler : MonoBehaviour
{
    private static LobbyNoticeHandler _instance;

    public static LobbyNoticeHandler instance
    {
        get
        {
            if(_instance == null)
            {

                var obj = GameObject.Find("Main Canvas").transform.Find("PopUp_Notice").GetComponent<LobbyNoticeHandler>();
                if(obj != null)
                    _instance = obj;
                else
                {
                    var newNoticeHandler = new GameObject("LobbyNoticeHandler").AddComponent<LobbyNoticeHandler>();
                    _instance = newNoticeHandler;
                }
            }

            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        _instance = null;
    }

    public Text Index;
    public Text Title;
    public Text Header;
    public Text body;
    public Text Date;
}
