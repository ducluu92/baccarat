using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextEffectController : MonoBehaviour
{
    private UITextGradient textEffectTarget = null;
    private bool isUp = true;

    public float effectOffset = 10.0f;

    void Start()
    {
        textEffectTarget = GetComponent<UITextGradient>();
    }
    
    void Update()
    {
        if(textEffectTarget != null)
        {
            if(isUp)
            {
                textEffectTarget.m_angle += effectOffset;

                if(textEffectTarget.m_angle >= 180.0f)
                {
                    isUp = false;
                }
            }
            else
            {
                textEffectTarget.m_angle -= effectOffset;

                if (textEffectTarget.m_angle <= -180.0f)
                {
                    isUp = true;
                }
            }
        }
    }
}
