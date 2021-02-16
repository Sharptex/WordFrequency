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
            string url = "";
            bool valid = false;
            var freqeuncyDictionary = new ConcurrentDictionary<string, int>();

            while (!valid)
            {
                valid = ConsoleIO.ReadUserParameters(out bufferSize, out top, out url);

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
            Console.WriteLine("Loading...");
            saveTask.GetAwaiter().GetResult();
            Console.WriteLine("Loaded");

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
                    //Console.WriteLine("prod");
                }
                inputLines.CompleteAdding();
            });


            var processLines = Task.Factory.StartNew(() =>
            {
                List<Record> buffer = new List<Record>();

                Parallel.ForEach(inputLines.GetConsumingEnumerable(), line =>
                {
                    var words = line.Split(separators, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in words)
                    {
                        if (freqeuncyDictionary.ContainsKey(word))
                        {
                            freqeuncyDictionary[word] = freqeuncyDictionary[word] + 1;
                        }
                        else
                        {
                            freqeuncyDictionary.AddOrUpdate(word, 1, (key, oldValue) => oldValue + 1);
                        }
                    }
                });
            });


            Task[] taskArray = new Task[2] { readLines, processLines };

            Console.WriteLine();
            Console.WriteLine("Job in progress...");


            try
            {
                Task.WaitAll(taskArray);
            }
            catch (AggregateException e)
            {
                for (int j = 0; j < e.InnerExceptions.Count; j++)
                {
                    Log.Write(e.InnerExceptions[j]);
                }
            }

            SqliteDB.SaveDictionary(freqeuncyDictionary, connectionString);

            ConsoleIO.OutputResults(top, connectionString);

            Console.WriteLine();
            Console.WriteLine("Done");


            Console.ReadKey();
        }


    }
}
