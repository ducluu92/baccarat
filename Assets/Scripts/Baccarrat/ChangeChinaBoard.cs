using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeChinaBoard : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite SpriteOneBoard;
    public Sprite SpriteSixBoard;

    public GameObject OneBoard;
    public GameObject SixBoard;

    private bool isOn = false;

    void Start()
    {
        gameObject.GetComponent<Image>().sprite = SpriteOneBoard;
    }

    public void ChangeBoard()
    {
        if (!isOn) 
        {
            // 원매 처리
            gameObject.GetComponent<Image>().sprite = SpriteSixBoard;
            OneBoard.SetActive(false);
            SixBoard.SetActive(true);
            isOn = true;
        }       
        else
        {
            // 육매 처리
            gameObject.GetComponent<Image>().sprite = SpriteOneBoard;
            OneBoard.SetActive(true);
            SixBoard.SetActive(false);
            isOn = false;
        }
    }
}
