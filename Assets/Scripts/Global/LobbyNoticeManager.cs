using Assets.Scripts.Utils;
using Module.Apis.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Settings
{
    public static class LobbyNoticeManager
    {
        public static Paging<NoticeData> Data { get; set; }

        public static void Requested(List<NoticeData> news) 
        {
            GC();

            // 데이터 로드
            var datas = Load();

            // 기존데이터 추가
            datas.AddRange(news);

            // 정렬
            var query = datas
                .AsQueryable()
                .OrderByDescending(c=>c.Index);

            // 페이지 빌더
            Data = PageingBuilder<NoticeData>.Build(query, 5, 5);

            // 데이터 저장
            Save(datas);
        }

        public static void SetPage(int index) 
        {
            var datas = Load();

            Data.PageIndex = index;
            Data.Refrash(datas.AsQueryable());
        }


        public static int GetLastIndex() 
        {
            var datas = Load();
            return datas.Max(c => c.Index);
        }


        #region Data Save

        const string location = "Notice";

        /// <summary>
        /// 단순 로드만 담당한다.
        /// </summary>
        /// <returns></returns>
        private static List<NoticeData> Load()
        {
            List<NoticeData> data;

            var json = PlayerPrefs.GetString(location, "[]");

            try
            {
                data = JsonConvert.DeserializeObject<List<NoticeData>>(json);

            }
            catch (System.Exception e) 
            {
                data = new List<NoticeData>();
            }

            return data;
        }


        /// <summary>
        /// 단순 저장만 담당한다.
        /// </summary>
        /// <param name="data"></param>
        private static void Save(List<NoticeData> data)
        {
            var s = JsonConvert.SerializeObject(data);
            PlayerPrefs.SetString(location, s);
        }

        /// <summary>
        /// 데이터를 삭제한다.
        /// </summary>
        public static void GC()
        {
            var now = DateTime.UtcNow;
            var datas = Load();

            var removes = datas.Where(c => c.Disappear < now).ToList();

            foreach (var r in removes)
            {
                datas.Remove(r);
            }

            Save(datas);
        }

        public static void AddRange(List<NoticeData> data)
        {
            var datas = Load();
            datas.AddRange(data);
            Save(datas);
        }

        #endregion

    }
}
