using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace Assets.Scripts.Baccarrat
{
    public class ChipContainer : MonoBehaviour
    {
        public int amount;
        public Transform GoldChip;
        public Transform ChipContainer_1000;
        public Transform ChipContainer_10000;
        public Transform ChipContainer_100000;
        public Transform ChipContainer_1000000;
        public GameObject[] PlayerEffect = new GameObject[4];

        public GameObject[] ChipContainer_chips_1000;
        public GameObject[] ChipContainer_chips_10000;
        public GameObject[] ChipContainer_chips_100000;
        public GameObject[] ChipContainer_chips_1000000;


        private void Awake()
        {
            this.Initialize();
        }

        public void Initialize()
        {
            for (int i = 0; i < 10; i++)
            {
                ChipContainer_chips_1000[i] = ChipContainer_1000.GetChild(i).gameObject;
                ChipContainer_chips_10000[i] = ChipContainer_10000.GetChild(i).gameObject;
                ChipContainer_chips_100000[i] = ChipContainer_100000.GetChild(i).gameObject;
                ChipContainer_chips_1000000[i] = ChipContainer_1000000.GetChild(i).gameObject;
            }
        }

        public Vector3 TopChipPosition(ref GameObject[] container) //제일 위에 있는 칩의 위치를 반환합니다.
        {
            int index = 0;
            for (int i = 0; i < 10; i++)
            {
                if(container[i].activeInHierarchy)
                    index = i;
            }

            return container[index].transform.position;
        }

        public void UpdateChipContainerNonCo(int amount) //   값에 따라 칩 컨테이너의 갯수를 업데이트하는 코루틴입니다.
        {
            this.amount = amount;
            amount /= 1000;

            if (amount % 10 == 0)
            {
            for (int j = 0; j < 10; j++)
                {
                    this.ChipContainer_chips_1000[j].SetActive(false);
                }
            }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_1000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_1000[i - 1].SetActive(false);
            }
        }


        amount /= 10;

        if (amount % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                this.ChipContainer_chips_10000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_10000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_10000[i - 1].SetActive(false);
            }
        }


        amount /= 10;

        if (amount % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                this.ChipContainer_chips_100000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_100000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_100000[i - 1].SetActive(false);
            }
        }

        amount /= 10;

        if((amount / 10) % 10 != 0)
        {
            if (amount % 10 > 0)
            {
                for (int j = 0; j < 10; j++)
                {
                    this.ChipContainer_chips_1000000[j].SetActive(true);
                }
            }

            return;
        }

        if (amount % 10 == 0)
        {
            amount /= 10;

                for (int j = 0; j < 10; j++)
                {
                    this.ChipContainer_chips_1000000[j].SetActive(false);
                }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_1000000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_1000000[i - 1].SetActive(false);
            }
        }



        }

        public IEnumerator UpdateChipContainer(int amount) //   값에 따라 칩 컨테이너의 갯수를 업데이트하는 코루틴입니다.
        {
            this.amount = amount;
            amount /= 1000;

            if (amount % 10 == 0)
            {
            for (int j = 0; j < 10; j++)
                {
                    this.ChipContainer_chips_1000[j].SetActive(false);
                }
            }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_1000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_1000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }


        amount /= 10;

        if (amount % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                this.ChipContainer_chips_10000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_10000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_10000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }


        amount /= 10;

        if (amount % 10 == 0)
        {
            for (int j = 0; j < 10; j++)
            {
                this.ChipContainer_chips_100000[j].SetActive(false);
            }
        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_100000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_100000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }

        amount /= 10;

        if (amount % 10 == 0)
        {
            amount /= 10;
        
            if (amount % 10 > 0)
            {
                for (int j = 0; j < 10; j++)
                {
                    this.ChipContainer_chips_1000000[j].SetActive(true);
                }
            }
            else
            {
                for (int j = 0; j < 10; j++)
                {
                    this.ChipContainer_chips_1000000[j].SetActive(false);
                }
             }

        }
        else
        {
            for (int i = 1; i < 11; i++)
            {
                if (amount % 10 >= i)
                    this.ChipContainer_chips_1000000[i - 1].SetActive(true);
                else
                    this.ChipContainer_chips_1000000[i - 1].SetActive(false);

                yield return new WaitForSeconds(0.0001f);
            }
        }



        }
        public void ClearAllChips(TrailRenderer chiptrail)
        {
            if(chiptrail != null)
            chiptrail.enabled = true;

            this.amount = 0;
            for(int i = 0; i < 10; i++)
            {
                ChipContainer_chips_1000[i].SetActive(false);
                ChipContainer_chips_10000[i].SetActive(false);
                ChipContainer_chips_100000[i].SetActive(false);
                ChipContainer_chips_1000000[i].SetActive(false);

				ChipContainer_chips_1000[i].GetComponent<SpriteRenderer>().DOFade(1, 0.1f);
				ChipContainer_chips_10000[i].GetComponent<SpriteRenderer>().DOFade(1, 0.1f);
				ChipContainer_chips_100000[i].GetComponent<SpriteRenderer>().DOFade(1, 0.1f);
				ChipContainer_chips_1000000[i].GetComponent<SpriteRenderer>().DOFade(1, 0.1f);
            }

            for(int i=0; i<4;i++)
            {
                PlayerEffect[i].GetComponent<SpriteRenderer>().DOFade(1, 0.1f);
            }
        }
    }
}
