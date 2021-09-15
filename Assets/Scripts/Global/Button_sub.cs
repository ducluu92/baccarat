using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Button_sub : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = gameObject.GetComponent<Button>();
    }

    public void Select()
    {
        button.onClick.Invoke();
        //EventSystem.current.SetSelectedGameObject(button.gameObject);
        //button.Select();
        //button.onClick.Invoke();
    }
}
