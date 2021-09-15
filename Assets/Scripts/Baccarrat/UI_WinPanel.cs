using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_WinPanel : MonoBehaviour
{
    public Room CurrentRoom;
    public GameObject UI_Max;

    public void ToggleMax()
    {
        if(CurrentRoom.Player != null && UI_Max != null)
        UI_Max.SetActive(!CurrentRoom.Player.UI_Max.activeInHierarchy);
    }
}
