using Assets.Scripts.Utils;
using Module.Apis.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace Assets.Scripts.Pages
{
    public static class QuestionPage
    {

        const int pageSize = 5;
        const int pagingSize = 5;

        private static List<QuestionData> _origin;

        public static Paging<QuestionData> Data { get; private set; }

        // 1번 인덱스로 초기화
        public static void SetByData(List<QuestionData> origin) 
        {
            _origin = origin;
            var query = origin.OrderByDescending(c => c.Index).AsQueryable();
            Data = PageingBuilder<QuestionData>.Build(query, pageSize, pagingSize);
        }

        // 페이지 이동
        public static void SetByPage(int index)
        {
            if (index < 0) 
            {
                return;
            }

            if (index > Data.TotalPages) 
            {
                return;
            }

            Data.PageIndex = index;
            var query = _origin.OrderByDescending(c => c.Index).AsQueryable();
            Data.Refrash(query);
        }


    }
}
