using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFrequency
{
    public class SqliteDB
    {
        public static void SaveBuffer(int maxList, ref List<Record> buffer, RecordContext context)
        {
            if (buffer.Count() > maxList)
            {
                context.Records.AddRange(buffer);
                context.SaveChanges();
                buffer = new List<Record>();
            }
        }
    }
}
