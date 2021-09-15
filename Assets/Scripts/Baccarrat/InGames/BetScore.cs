using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Baccarrat.InGames
{

    /// <summary>
    /// 배팅 스코어를 관리한다.
    /// </summary>
    public class BetScore : MonoBehaviour
    {
        bool isNeedUpdate;

        // 업데이트 시점에 따라서
        int allBetting;
        int myBetting;
        int otherBetting;


        private void Start()
        {

        }

        private void Update()
        {
            if (isNeedUpdate) 
            {
                UpdateText();
            }
        }

        private void UpdateText() 
        {
            // 텍스트 빌드 로직
            var s = $"{myBetting} / {allBetting}";

            // 업데이트 로직

        }

        public void Betting(int value, bool isMine) 
        {
            if (isMine)
            {
                myBetting += value;
            }
            else 
            {
                otherBetting += value;
            }

            allBetting += value;
            isNeedUpdate = true;
        }


        public void Clear() 
        {
            myBetting = 0;
            otherBetting = 0;
            allBetting = 0;
            isNeedUpdate = true;
        }

        public void ClearByMine() 
        {
            allBetting -= myBetting;
            myBetting = 0;
            isNeedUpdate = true;
        }

    }
}
