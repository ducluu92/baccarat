using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Button_sub_active : MonoBehaviour
{
    Button button;

    void Start()
    {
        button = gameObject.GetComponent<Button>();
    }

    private void OnEnable()
    {
        button = gameObject.GetComponent<Button>();
        Select();
    }

    public void Select()
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
    }
}
