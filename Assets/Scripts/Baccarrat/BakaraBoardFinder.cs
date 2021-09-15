using Module.Apis.Bakaras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Baccarrat
{
    public static class BakaraBoardFinder
    {
        public static void Get<T>(out int minX, T[,] arr, int size = 40)
        {
            int savedX = 0;

            int arraysize = arr.GetLength(0);

            for (int x = 0; x < arraysize; x++)
            {
                if (arr[x, 0] == null)
                {
                    break;
                }
                else
                {
                    if (x > size - 1)
                    {
                        savedX++;
                    }
                }

            }

            if (savedX + size <= arraysize - 1)
                minX = savedX;
            else
                minX = arraysize - size - 1;

            if (minX < 0)
                minX = 0;
        }
    }
}
