using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby_AccountPopUp : MonoBehaviour
{
    private static Lobby_AccountPopUp _instance;

    public Text index;
    public Text CompanyAccountName;
    public Text CompanyAccountOwner;
    public Text CompanyAccountNumber;

    public static Lobby_AccountPopUp instance
    {
        get
        {
            if(_instance == null)
            {

                var obj = GameObject.Find("Main Canvas").transform.Find("PopUp_Account").GetComponent<Lobby_AccountPopUp>();
                if(obj != null)
                    _instance = obj;
                else
                {
                    var newAccountPopUp = new GameObject("PopUp_Account").AddComponent<Lobby_AccountPopUp>();
                    _instance = newAccountPopUp;
                }
            }

            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }
}
