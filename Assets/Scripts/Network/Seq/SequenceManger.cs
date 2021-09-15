using Assets.Scripts.Loggers;
using Module.Apis.ApiDefinition;
using Module.Apis.Seqs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Network.Seq
{


    public static class SequenceManger
    {
        private static long seq = 0;
        private static bool isCall = false; 
        private static bool _isDebug = false;

        private static object lockObj = new object();


        public static void SetDebug(bool isDebug) 
        {
            _isDebug = isDebug;
        }

        /// <summary>
        /// 로그인 성공시 호출
        /// </summary>
        public static void LoginSucceed()
        {
            seq = 10000;
            isCall = false;
        }

        /// <summary>
        /// 서버 호출시 불러서 Sequnce에 준다.
        /// </summary>
        /// <returns></returns>
        public static SequenceStatusByCall Call(out long squence)
        {
            squence = 0;
            return SequenceStatusByCall.OK;

            lock (lockObj) 
            {
                if (isCall)
                {
                    squence = -1;
                    return SequenceStatusByCall.Called;
                }
            }

            seq += SeqenceDefinition.clientUpdate;
            squence = seq;
            isCall = true;

            MyLog.Write(LogLocation.Sequence, $"Req-Sequence : {seq}");
            return SequenceStatusByCall.OK;
        }

        /// <summary>
        /// Api 호출 완료시 사용할 것
        /// </summary>
        /// <param name="responce">받아온 시퀸스 값</param>
        /// <returns></returns>
        public static SequenceStatusByBack CallBack(long responce)
        {
            responce = 0;
            return SequenceStatusByBack.OK;

            // Api 호출 여부 검사
            if (!isCall)
            {
                return SequenceStatusByBack.FailByNotCall;
            }

            isCall = false;

            // 복사
            long local = seq;

            // 서버에서 줄값 
            long est = local + SeqenceDefinition.serverUpdate;

            //
            if (est == responce)
            {
                seq = est;
                MyLog.Write(LogLocation.Sequence, $"Res-Sequence : {seq}");
                return SequenceStatusByBack.OK;
            }
            else
            {
                return SequenceStatusByBack.FailByNotSame;
            }
        }

        public static void CallFail() 
        {
            isCall = false;
        }




    }
}
