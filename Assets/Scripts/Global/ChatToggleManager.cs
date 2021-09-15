using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatToggleManager : MonoBehaviour
{
    public Toggle m_goToggle;
    public GameObject chattingPanel;

    void Start()
    {
        m_goToggle = gameObject.GetComponent<Toggle>();
        m_goToggle.isOn = false;
    }

    public void OnChangeToggle(bool bActive)
    {
        chattingPanel.SetActive(bActive);
        m_goToggle.isOn = bActive;
    }

    public void BuildingSetActive() 
    {
        
    }


}
