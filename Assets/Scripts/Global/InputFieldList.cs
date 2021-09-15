using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Settings
{
    public class InputFieldList : MonoBehaviour
    {
        public List<InputField> inputfields;
        public Button ConfirmButton;

        public int currentindex = 0;

        private void Start()
        {
            foreach (var inputfield in inputfields)
            {
                inputfield.onEndEdit.AddListener(delegate { Emit(); });
            }
        }

        private void Update()
        {
            if(gameObject.activeInHierarchy)
            {
                foreach(var inputfield in inputfields)
                {
                    if(inputfield.isFocused)
                    {
                        currentindex = inputfields.IndexOf(inputfield);
                        if(Input.GetKeyUp(KeyCode.Tab))
                        {
                            if (currentindex + 1 < inputfields.Count)
                            inputfields[currentindex + 1].Select();
                        }
                    }
                }
            }
        }

        private void Emit()
        {
            if (currentindex + 1 >= inputfields.Count && ConfirmButton != null)
                ConfirmButton.Select();
        }
    }
    
}
