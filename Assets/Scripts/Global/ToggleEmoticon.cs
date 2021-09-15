using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleEmoticon : MonoBehaviour
{
    public GameObject Panel_Emoticon;

    private bool toggleFlag = false;

    void Start()
    {
        Panel_Emoticon.SetActive(toggleFlag);
    }

    public void TogglePanel()
    {
        toggleFlag = !toggleFlag;

        Panel_Emoticon.SetActive(toggleFlag && (UserInfoManager.SelectedIdx > 0));
    }
}
