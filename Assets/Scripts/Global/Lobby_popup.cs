using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;

public class Lobby_popup : MonoBehaviour
{
    public GameObject Gem;

    private void OnEnable()
    {
        Gem.SetActive(false);
    }

    private void OnDisable()
    {
        Gem.SetActive(true);
    }
}
