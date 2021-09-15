using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Assets.Scripts.Pages;

public class UIIndexButton : MonoBehaviour, IPointerClickHandler
{
    public int index = 1;
    public Text indexText;
    public int ListType = 0; //0 = Notice, 1 = BankingQuestion, 2 = AccountQuestion, 3 = EtcQuestion
    public KLNetwork_Lobby networkHandler;

    public void Initialize(KLNetwork_Lobby _networkHandler)
    {
        networkHandler = _networkHandler;
        indexText.text = index.ToString();
    }

    public void OnPointerClick(PointerEventData data)
    {
        switch(this.ListType)
        {
            case 0:
                networkHandler.NoticeListRequest(index);
                break;
            case 1:
                networkHandler.QuestionListRequest(0, index);
                break;
            case 2:
                networkHandler.QuestionListRequest(1, index);
                break;
            case 3:
                networkHandler.QuestionListRequest(2, index);
                break;
            default:
                break;
        }
        
    }

}
