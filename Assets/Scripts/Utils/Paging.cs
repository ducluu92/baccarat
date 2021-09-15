using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
    public class Paging<T>
    {
        public List<T> Paged { get; private set; }

        public List<int> MovePages { get; set; }

        /// <summary>
        /// 페이지 현재 위치
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 총 페이지 갯수
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// 페이지 크기
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 하단 페이지 크기
        /// </summary>
        public int PagingSize { get; set; }



        public void Refrash(IQueryable<T> query)
        {
            // 총 페이지 수 
            TotalPages = BuildTotalPages(query);
            Paged = BuildPage(query);
            SetMovePages();
        }

        private int BuildTotalPages(IQueryable<T> query)
        {
            int count = query.Count();
            return (int)Math.Ceiling(count / (double)PageSize);
        }

        private List<T> BuildPage(IQueryable<T> query)
        {
            return query
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .ToList();
        }

        private void SetMovePages()
        {

            int half = PagingSize / 2;
            MovePages.Clear();

            // 총 페이지가 정의된 사이즈 이하인경우 이렇게 사용
            if (TotalPages < PagingSize)
            {
                MovePages.AddRange(MovePageBulider(1, TotalPages));
                return;
            }

            // 처음인경우 
            if (PageIndex < half)
            {
                MovePages.AddRange(MovePageBulider(1, PagingSize));
                return;
            }

            // 마지막에 가까울 경우
            if (PageIndex + half > TotalPages)
            {
                MovePages.AddRange(MovePageBulider(TotalPages - PagingSize, TotalPages));
                return;
            }

            // 일반 상황인경우
            MovePages.AddRange(MovePageBulider(PageIndex - half, PageIndex + half));

        }

        /// <summary>
        /// 하단의 페이지 빌더
        /// </summary>
        private List<int> MovePageBulider(int startValue, int endValue)
        {
            List<int> list = new List<int>();

            for (int i = startValue; i <= endValue; i++)
            {
                list.Add(i);
            }

            return list;
        }

    }
}
