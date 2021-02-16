using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFrequency
{
    public class ConsoleIO
    {
        public static void OutputResults(int top, string connectionString)
        {
            try
            {
                using (var context = new RecordContext(connectionString))
                {
                    var result = context.Stats.OrderByDescending(n => n.Count).Take(top).AsEnumerable();

                    Console.WriteLine();
                    Console.WriteLine("Top most commonly found words:");
                    foreach (var node in result)
                    {
                        Console.WriteLine("{0} - {1} times", node.Word, node.Count);
                    }

                    Console.WriteLine();
                    Console.WriteLine("{0} words in dictionary", context.Stats.Count());
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        public static bool ReadUserParameters(out int bufferSize, out int top, out string url)
        {
            bufferSize = 100;
            top = 10;
            url = "http://www.gutenberg.org/files/2600/2600-0.txt";

            Console.WriteLine("Use default parameters? y/n");
            if (Console.ReadLine() == "n")
            {
                try
                {
                    Console.Write("Stream buffer size, bytes:");
                    string input1 = Console.ReadLine();
                    Int32.TryParse(input1, out bufferSize);

                    Console.Write("Number of highest frequency words to output:");
                    string input2 = Console.ReadLine();
                    Int32.TryParse(input2, out top);

                    Console.Write("Url:");
                    url = Console.ReadLine();

                    if (bufferSize > 0 && top > 0 && !string.IsNullOrEmpty(url))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Write(ex);
                    return false;
                }
            }

            return true;
        }


    }
}
