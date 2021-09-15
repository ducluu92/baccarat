using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Baccarrat.InGames
{
    public enum ChipType
    {
        /// <summary>
        /// 선택 없음
        /// </summary>
        None,

        /// <summary>
        /// 1,000
        /// </summary>
        ChipThousand,
        
        /// <summary>
        /// 10,000
        /// </summary>
        Chip10Thousand,

        /// <summary>
        /// 100,000
        /// </summary>
        Chip100Thousand,

        /// <summary>
        /// 1,000,000
        /// </summary>
        ChipMillion,

        /// <summary>
        /// 맥스 배팅
        /// </summary>
        ChipMax,

    }
}
