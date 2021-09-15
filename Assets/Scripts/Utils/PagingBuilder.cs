using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Utils
{
	public static class PageingBuilder<T>
	{
        public static Paging<T> Build(
             IQueryable<T> query, int pageSize = 10, int pagingSize = 10)
        {
            var pg = new Paging<T>
            {
                PageIndex = 1,
                PageSize = pageSize,
                PagingSize = pagingSize,
                MovePages = new List<int>()
            };

            pg.Refrash(query);

            return pg;
        }

    }
}
