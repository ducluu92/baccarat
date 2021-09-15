using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

namespace Assets.Scripts.Baccarrat.InGames
{
    public class Chip : MonoBehaviour
    {
        public ChipType type;

        public Sprite MaxChipOn;
        public Sprite MaxChipOff;

        public IEnumerator Select()
        {
            yield return null;

            if (type!=ChipType.ChipMax)
            {
                var lightObj = transform.GetChild(0);

                // sacle up
                lightObj.DOScale(1.0f, 0.1f);
                lightObj.DOScale(new Vector3(115.0f, 90.0f, 100.0f), 0.1f);

                gameObject.transform.DOScale(0.92f, 0.1f);
            }
            else
            {
                var lightObj = transform.GetChild(0);
                var ringObj = transform.GetChild(1);


                // sacle up
                lightObj.DOScale(1.0f, 0.1f);
                lightObj.DOScale(new Vector3(115.0f, 90.0f, 100.0f), 0.1f);

                gameObject.transform.DOScale(0.92f, 0.1f);

                // ring effect on
                ringObj.gameObject.SetActive(true);

                // ring image change
                gameObject.GetComponent<Image>().sprite = MaxChipOn;

            }
        }

        public IEnumerator UnSelect()
        {
            yield return null;
            
            if (type != ChipType.ChipMax)
            {
                var lightObj = transform.GetChild(0);

                // sacle down
                lightObj.DOScale(0.0f, 0.1f);
                gameObject.transform.DOScale(0.85f, 0.1f);
            }
            else
            {
                var lightObj = transform.GetChild(0);
                var ringObj = transform.GetChild(1);

                // sacle down
                lightObj.DOScale(0.0f, 0.1f);
                gameObject.transform.DOScale(0.85f, 0.1f);

                // ring effect off
                ringObj.gameObject.SetActive(false);

                // ring image change
                gameObject.GetComponent<Image>().sprite = MaxChipOff;
            }
        }
    }
}
