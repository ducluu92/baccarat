using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public Text timerCountText;
    public Image fillValueImage;
    public GameObject TimerEffect;
    public GameObject BoardEffect;
    public Animator TimerCircleEffect;
    public Animation BettingEndAnim;
    public Room room;


    private AudioSource TickSound;


    private float IndexCounter = 10.0f;
    private float prevCounter = 10.0f;
    private float currentCounter = 10.0f;
    private float maxCounter = 10.0f;

    private float timer = 0.0f;
    private float delay = 0.7f;

    private bool isTimerTickPlay = false;

    private bool isTimerEnabled = false;
    public bool isSelected = false;

    void Start()
    {
        TickSound = gameObject.GetComponent<AudioSource>();
    }

    public void ToggleSelect(bool boolean)
    {
        this.isSelected = boolean;
    }

    void Update()
    {
        if (isTimerEnabled)
        {
            timer += Time.deltaTime;

            if (timer >= delay)
            {
                if ((int)IndexCounter != (int)currentCounter)
                {
                    if (!this.isSelected)
                    {
                        BoardEffect.SetActive(false);
                        BoardEffect.SetActive(true);
                    }
                    else
                    {
                        BoardEffect.SetActive(false);
                    }

                    TimerEffect.SetActive(false);
                    TimerEffect.SetActive(true);

                    IndexCounter = currentCounter;
                }

                timer = 0.0f;
                prevCounter = currentCounter;
                currentCounter -= delay;

                StartCoroutine(updateProgressbarImage(prevCounter,currentCounter));

                if (fillValueImage.fillAmount >= 0.7f)
                    this.TimerCircleEffect.SetInteger("SpeedIndex", 0);
                else if (fillValueImage.fillAmount >= 0.2f && fillValueImage.fillAmount < 0.7f)
                    this.TimerCircleEffect.SetInteger("SpeedIndex", 1);
                else
                {
                    if (currentCounter <= 10.0f)
                    {
                        if (!isTimerTickPlay)
                        {
                            isTimerTickPlay = true;
                            TickSound.loop = true;
                            TickSound.Play();

                            this.TimerCircleEffect.SetInteger("SpeedIndex", 3);
                        }
                    }   
                }

                if (currentCounter <= 0.0f)
                {
                    TickSound.Stop();

                    isTimerEnabled = false;
                    gameObject.SetActive(false);

                    if (room != null)
                    {
                        room.DisableButtons();
                        room.ClearAllSelection();
                    }



                    BettingEndAnim.wrapMode = WrapMode.Once;
                    BettingEndAnim.Play();
                }
            }
        }
    }

    public IEnumerator updateProgressbarImage(float previous, float current)
    {
        float temp = previous;

        while(temp >= current)
        {
            float gTxtOffset = (temp / maxCounter) * 110.0f;
            float bTxtOffset = (temp / maxCounter) * -25.0f;

            float gImgOffset = (temp / maxCounter) * 173.0f;
            float bImgOffset = (temp / maxCounter) * 230.0f;

            timerCountText.text = Convert.ToString((int)temp);
            timerCountText.color = new Color(1.0f, (82.0f + gTxtOffset) / 255.0f, (25.0f + bTxtOffset) / 255.0f);

            fillValueImage.color = new Color(1.0f, (82.0f + gImgOffset) / 255.0f, (25.0f + bImgOffset) / 255.0f);
            fillValueImage.fillAmount = (0.7f * (temp / maxCounter));

            temp = temp - 0.01f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public int CurrentTime()
    {
        return (int)currentCounter;
    }

    public bool IsTimerEnabled()
    {
        return isTimerEnabled;
    }

    public void StartTimer(int count)
    {
        maxCounter = count;
        currentCounter = count;
        IndexCounter = currentCounter;

        timerCountText.text = Convert.ToString(currentCounter);
        fillValueImage.fillAmount = 0.7f * (currentCounter / maxCounter);

        gameObject.SetActive(true);

        isTimerTickPlay = false;
        isTimerEnabled = true;
    }

    public void Clear()
    {
        timerCountText.text = "0";
        fillValueImage.fillAmount = 0.0f;

        TimerEffect.SetActive(false);
        BoardEffect.SetActive(true);
        gameObject.SetActive(false);

        isTimerTickPlay = false;
        isTimerEnabled = false;
    }
}
