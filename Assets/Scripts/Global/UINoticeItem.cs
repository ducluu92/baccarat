using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UINoticeItem : MonoBehaviour
{
    public Text Index;
    public Text Date;
    public Text Message;
    public Text Type;
    public GameObject NewImage;
    public string body;

    public void UpdatePopup()
    {
        LobbyNoticeHandler.instance.Index.text = "No." + this.Index.text;
        LobbyNoticeHandler.instance.Title.text = Type.text;
        LobbyNoticeHandler.instance.Header.text = this.Message.text;
        LobbyNoticeHandler.instance.Date.text = this.Date.text;
        LobbyNoticeHandler.instance.body.text = this.body;
        LobbyNoticeHandler.instance.gameObject.SetActive(true);
    }

}
