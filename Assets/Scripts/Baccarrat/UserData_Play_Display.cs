using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UserData_Play_Display : MonoBehaviour
{
    public Text Nickname;
    public Text Currentchips;
    public SpriteRenderer Profile;

    private void Start()
    {
        UpdateDisplays();
    }

    public void UpdateDisplays()
    {
        UserData_Play sum = gameObject.GetComponent<UserData_Play>();

        if (sum.ProfileID == 0)
        {
            Profile.gameObject.SetActive(false);
            Nickname.gameObject.SetActive(false);
            Currentchips.gameObject.SetActive(false);
        }
        else
        {
            Object[] Profiles;
            Profiles = Resources.LoadAll<Sprite>("Sprites/Sprites_Profile");
            Profile.sprite = (Sprite)Profiles[sum.ProfileID - 1];

            Nickname.text = sum.Nickname;

            Currentchips.text = string.Format("{0:#,###,###,###}", sum.CurrentChips);
        }
    }

    

}
