using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using UnityEngine.Experimental.UIElements;

namespace Assets.Scripts.Baccarrat.InGames
{
    public class InGameBag
    {
        #region SingleTone

        private static readonly Lazy<InGameBag> _instance = new Lazy<InGameBag>(() => new InGameBag());

        public static InGameBag Instance { get { return _instance.Value; } }

        #endregion

        /// <summary>
        /// 선택한 칩 등록
        /// </summary>
        public ChipType SelectedChip { get; set; } = ChipType.None;

        /// <summary>
        /// 내 위치 등록
        /// </summary>
        public int MyIndex { get; set; } = -1;

        /// <summary>
        /// 맥스 배팅 인지 아닌지 체크
        /// </summary>
        public bool IsMaxBetNow { get; set; } = false;

        /// <summary>
        /// 중도입장을 한 상황인지 아닌지 체크
        /// </summary>
        public bool IsMiddlePosition { get; set; } = false;

        /// <summary>
        /// 칩이 클릭 되고 있는 중인지 확인
        /// </summary>
        public bool IsChipClickedNow { get; set; } = false;

        /// <summary>
        /// 배팅을 해도 되는 상황인지 체크하는 변수
        /// </summary>
        public bool IsBettingOK { get; set; } = false;

        /// <summary>
        /// 특수 배팅(2배, 이전)을 해도 되는 상황인지 체크하는 변수
        /// </summary>
        public bool IsSpecialBettingOK { get; set; } = false;

        public void ClearAll() 
        {
            MyIndex = -1;
            SelectedChip = ChipType.None;
        }

    }
}
