using System;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    class ChattingCanvas : MonoBehaviour
    {
        public void Toggle()
        {
            gameObject.SetActive(!gameObject.activeInHierarchy);
        }

    }
}
