using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchAndMove
{
    public static class RecentListUtil
    {
        //public const int MaxSize = 10;

        public static void Upsert(IList<string> list, string value, int maxSize = 10)
        {
            int? existing = null;
            
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                {
                    existing = i;
                    break;
                }
            }

            if (existing != null)
            {
                list.RemoveAt(existing.Value);
            }

            list.Insert(0, value);

            if (list.Count > maxSize)
            {
                list.RemoveAt(list.Count - 1);
            }
        }
    }
}
