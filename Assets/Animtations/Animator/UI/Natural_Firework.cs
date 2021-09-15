using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Natural_Firework : MonoBehaviour
{
    public Sprite Silver, Gold;
    public SpriteRenderer Crown;
    public int num;

    public void SetCrown()
    {
        if (num == 0)
            Crown.sprite = Silver;
        else if (num == 1)
            Crown.sprite = Gold;
    }
}
