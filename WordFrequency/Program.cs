using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WordFrequency
{
    class Program
    {
        static void Main(string[] args)
        {
            int bufferSize = 0;
            int top = 0;
            int maxList = 0;
            string url = "";
            bool valid = false;
            var freqeuncyDictionary = new ConcurrentDictionary<string, int>();

            while (!valid)
            {
                valid = ReadUserParameters(out bufferSize, out top, out maxList, out url);

                if (!valid)
                {
                    Console.WriteLine("Invalid input, try again? y/n");
                    if (Console.ReadLine() == "n")
                    {
                       return;
                    }
                }
            }

            string tempFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "temp.txt");
            Task saveTask = HttpResponseSaver.SaveToFile(url, tempFile, bufferSize);
            saveTask.GetAwaiter().GetResult();

            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "words.sqlite");
            string connectionString = new SQLiteConnectionStringBuilder() { DataSource = dbPath, ForeignKeys = true }.ConnectionString;
            if (File.Exists(dbPath)) { File.Delete(dbPath); }


            BlockingCollection<string> inputLines = new BlockingCollection<string>();
            char[] separators = new[] { ' ', ',', '.', '!', '?', '"', ';', ':', '[', ']', '(', ')', '\n', '\r', '\t' };


            var readLines = Task.Factory.StartNew(() =>
            {
                foreach (var line in File.ReadLines(tempFile))
                {
                    inputLines.Add(line);
                }
                inputLines.CompleteAdding();
            });


            var processLines = Task.Factory.StartNew(() =>
            {
                List<Record> buffer = new List<Record>();

                using (var context = new RecordContext(connectionString))
                {
                    foreach (var line in inputLines.GetConsumingEnumerable())
                    {
                        var words = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var word in words)
                        {
                            buffer.Add(new Record() { Id = 1, Word = word });
                            if (freqeuncyDictionary.ContainsKey(word))
                            {
                                freqeuncyDictionary[word] = freqeuncyDictionary[word] + 1;
                            }
                            else
                            {
                                freqeuncyDictionary.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
                            }
                        }

                        //SqliteDB.SaveBuffer(maxList, ref buffer, context);
                    };

                    //SqliteDB.SaveBuffer(0, ref buffer, context);
                }
            });


            Task[] taskArray = new Task[2] { readLines, processLines };

            Console.WriteLine();
            Console.WriteLine("Job in progress...");


            try
            {
                Task.WaitAll(taskArray);

                Console.WriteLine("Job finished");
            }
            catch (AggregateException e)
            {
                for (int j = 0; j < e.InnerExceptions.Count; j++)
                {
                    Log.Write(e.InnerExceptions[j]);
                }
            }

            //OutputResults(top, maxList, connectionString);

            Console.WriteLine();
            Console.WriteLine("Done");


            Console.ReadKey();
        }

        private static void OutputResults(int top, int maxList, string connectionString)
        {
            var buffer2 = new List<Stat>();
            int index = 0;

            Console.WriteLine();
            Console.WriteLine("Top most commonly found words:");

            try
            {
                using (var context = new RecordContext(connectionString))
                {
                    var result = context.Records.GroupBy(p => p.Word)
                                                .Select(g => new
                                                {
                                                    Word = g.Key,
                                                    Count = g.Count(),
                                                }).OrderByDescending(n => n.Count).AsEnumerable();

                    foreach (var node in result)
                    {
                        index++;
                        if (index < top) Console.WriteLine("{0} - {1} times", node.Word, node.Count);
                        buffer2.Add(new Stat() { Id = 1, Word = node.Word, Count = node.Count });
                        if (buffer2.Count() > maxList) { context.Stats.AddRange(buffer2); context.SaveChanges(); buffer2 = new List<Stat>(); }
                    }
                    if (buffer2.Count() != 0) { context.Stats.AddRange(buffer2); context.SaveChanges(); }


                    Console.WriteLine();
                    Console.WriteLine("{0} words counted", context.Records.Count());
                    Console.WriteLine("{0} words in dictionary", result.Count());
                }
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
        }

        private static bool ReadUserParameters(out int bufferSize, out int top, out int maxList, out string url)
        {
            bufferSize = 100;
            top = 10;
            maxList = 1000;
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

                    Console.Write("Max in-memory list size for db writing:");
                    string input3 = Console.ReadLine();
                    Int32.TryParse(input3, out maxList);

                    Console.Write("Url:");
                    url = Console.ReadLine();

                    if (bufferSize>0 && top>0 && maxList>0 && !string.IsNullOrEmpty(url))
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
