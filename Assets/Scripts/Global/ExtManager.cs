using Assets.Scripts.Network.Seq;
using Module.Apis.ApiDefinition;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public class ExitManager
    {
        public static bool DisplayBySeq(long seq)
        {
            bool res = true;

            var err = SequenceManger.CallBack(seq);
            switch (err)
            {
                //OS 디폴트 메시지창으로 에러 출력하고 어플 종료
                case SequenceStatusByBack.FailByNotSame: //서버와 클라이언트 시퀀스가 다른 경우
#if UNITY_EDITOR
                    EditorUtility.DisplayDialog("ERROR", "FailByNotSame", "OK");
#endif
                    break;
                case SequenceStatusByBack.FailByNotCall: //부른적 없는 시퀀스일 경우
#if UNITY_EDITOR
                    EditorUtility.DisplayDialog("ERROR", "FailByNotCall", "OK");
#endif
                    break;
                default:
                    res = false;
                    break;
            }

            return res;
        }

        public static bool DisplayByError(CommonError error) 
        {
            return error != CommonError.OK;
        }
            


    }
}
