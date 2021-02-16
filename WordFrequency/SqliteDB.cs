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
        public static void SaveDictionary(ConcurrentDictionary<string, int> freqeuncyDictionary, string connectionString)
        {
            try
            {
                using (var context = new RecordContext(connectionString))
                {
                    var buffer = new List<Stat>();
                    foreach (var item in freqeuncyDictionary)
                    {
                        buffer.Add(new Stat() { Id = 1, Word = item.Key, Count = item.Value });
                    }
                    context.Stats.AddRange(buffer);
                    context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }
    }
}
