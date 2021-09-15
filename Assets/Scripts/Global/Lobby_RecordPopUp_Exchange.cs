using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lobby_RecordPopUp_Exchange : MonoBehaviour
{
    private static Lobby_RecordPopUp_Exchange _instance;

    public Text index;
    public Text Money; //입금 예정 금액
    public Text TransferredMoney; //이체된 금액
    public Text Point;
    public Text Bonus;
    public Text ProcessedDate;

    public static Lobby_RecordPopUp_Exchange instance
    {
        get
        {
            if(_instance == null)
            {

                var obj = GameObject.Find("Main Canvas").transform.Find("PopUp_Record (Exchange)").GetComponent<Lobby_RecordPopUp_Exchange>();
                if(obj != null)
                    _instance = obj;
                else
                {
                    var newAccountPopUp = new GameObject("PopUp_Record (Exchange)").AddComponent<Lobby_RecordPopUp_Exchange>();
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
