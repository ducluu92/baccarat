using System;
using UnityEngine;

namespace Assets.Scripts.Baccarrat
{
    public class Highlights : MonoBehaviour
    {
        //Upper : 선택했을때 빠르게 점멸하는 외곽선.
        //UpperHigh : 선택했을때 빠르게 점멸하는 영역.
        //Particle : 선택했을때 나오는 파티클.
        //AvailableZone : 배팅마감 후 남아있는 외곽선 영역.
        //AmbientEffet : 배팅전 천천히 깜빡이는 외곽선.
        public GameObject Upper, Particle, UpperHigh,AvailableZone,AmbientEffect;
    }
}
