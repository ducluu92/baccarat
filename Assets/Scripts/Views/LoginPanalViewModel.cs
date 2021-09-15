using Assets.Scripts.Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Settings;

public class LoginPanalViewModel : MonoBehaviour
{
    private IRememberMeService _rememberMeService;
    private InputFieldList _inputfieldlist;

    void Awake()
    {
        _rememberMeService = new RememberMeLocalStorageService();
        _inputfieldlist = GetComponent<InputFieldList>();
    }

    private void OnEnable()
    {
        _inputfieldlist.inputfields[0].text = _rememberMeService.GetID();
        _inputfieldlist.inputfields[1].text = _rememberMeService.GetPassword();
    }
}
