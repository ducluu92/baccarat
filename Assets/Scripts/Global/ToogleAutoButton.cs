using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToogleAutoButton : MonoBehaviour
{
    void Start()
    {
        if (!PlayerPrefs.HasKey("isAutoChina"))
        {
            PlayerPrefs.SetInt("isAutoChina", 1);

            var toggleBtn = GetComponent<Toggle>();
            toggleBtn.isOn = true;
        }
    }

    public void changeAutoChina()
    {
        var toggleBtn = GetComponent<Toggle>();
        if(toggleBtn.isOn)
        {
            PlayerPrefs.SetInt("isAutoChina", 1);
        }
        else PlayerPrefs.SetInt("isAutoChina", 0);
    }
}
