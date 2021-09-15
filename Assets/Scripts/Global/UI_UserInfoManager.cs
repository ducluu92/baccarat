using Module.Utils.Currency;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_UserInfoManager : MonoBehaviour
{
    public Text nickname;
    public Text level;
    public Slider exp;
    public Text cash;
    public Text bonus;

    // Start is called before the first frame update
    void Start()
    {
        nickname.text = "";
        level.text = "";
        exp.value = 0.0f;
        cash.text = "";
        bonus.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        nickname.text = UserInfoManager.AccountInfo.NicName;
        level.text = "Lv " + Convert.ToString(UserInfoManager.AccountInfo.LV);
        exp.value = 0.0f;
        cash.text = CurrencyConverter.Kor(UserInfoManager.AccountInfo.Cash);
        bonus.text = Convert.ToString(UserInfoManager.AccountInfo.Bonus);
    }
}
