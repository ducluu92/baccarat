using Module.Utils.Times;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Loggers
{
    public class MyLog
    {
        const string m_strPath = "Assets/Resources/Logs/";

        private static object lockObj = new object();


        public static void Write(string location, string data) 
        {
            // https://docs.unity3d.com/ScriptReference/Debug-isDebugBuild.html
            if (!UnityEngine.Debug.isDebugBuild)
            {
                return;
            }

            /* 로그 안찍을 대상 추가*/
            switch (location)
            {
                case LogLocation.Ignore:
                //case LogLocation.Received:
                    return;
                default:
                    break;
            }

            var stamp = TimeStamper.TimeStampByString();
            string s = $"[{location}:{stamp}]:{data}";

            Monitor.Enter(lockObj);

            UnityEngine.Debug.Log(s);
            System.Diagnostics.Debug.WriteLine(s);

            //FileStream f = new FileStream($"{m_strPath}{DateTime.UtcNow.ToString("yyyy/MM/dd")}.log" , FileMode.Append, FileAccess.Write);
            //StreamWriter writer = new StreamWriter(f, System.Text.Encoding.Unicode);
            //writer.WriteLine(s);
            //writer.Close();

            Monitor.Exit(lockObj);
        }

        public static IEnumerator IDebug(string data)
        {
            yield return null;

            var stamp = TimeStamper.TimeStampByString();
            string s = $"[{stamp}] : {data}";

            UnityEngine.Debug.Log(s);
            System.Diagnostics.Debug.WriteLine(s);

            Monitor.Enter(lockObj);

            FileStream f = new FileStream($"{m_strPath} + {DateTime.UtcNow.ToString("yyyy/MM/dd")}.log", FileMode.Append, FileAccess.Write);
            StreamWriter writer = new StreamWriter(f, System.Text.Encoding.Unicode);
            yield return writer.WriteLineAsync(s);

            writer.Close();

            Monitor.Enter(lockObj);


        }

        /// <summary>
        /// 서버로 리포트
        /// </summary>
        /// <returns></returns>
        public static IEnumerator Report()
        {
            yield return null;

            var uri = "";

            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {


            }
        }

    }
}
