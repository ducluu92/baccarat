using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Assets.Scripts.Baccarrat.InGames
{
    public class BettingChipController : MonoBehaviour
    {
        public Chip[] ChipArray;

        private Chip PrevSelectedChip;

        public void Betting(int index)
        {
            InGameBag.Instance.SelectedChip = ChipArray[index].type;

            Debug.Log("InGameBag.Instance.SelectedChip => " + InGameBag.Instance.SelectedChip);

            // 현재 칩은 선택 효과 표시
            StartCoroutine(ChipArray[index].Select());

            // 이전 칩은 선택 취소 효과 표시
            if(PrevSelectedChip != null && PrevSelectedChip != ChipArray[index])
                StartCoroutine(PrevSelectedChip.UnSelect());

            PrevSelectedChip = ChipArray[index];
        }
    }
}
