using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {
    public static string _STRING = "This works too!";

    //both can be used
    public int value;   //1)

    public int Value    //2)
    {
        get { return value; }
    }

    public void IncreaseValue(int amount)   //can be called on a onClick event to simulate changing values
    {
        value += amount;
    }

    //works with other types too
    public RectTransform trans;

    void Update()
    {
        if(trans != null)
        {
            trans.position = Input.mousePosition;
        }
    }

    public string X
    {
        get { return "x:" + trans.position.x; }
    }

    public float XPlusY
    {
        get { return trans.position.x + trans.position.y; }
    }
}
