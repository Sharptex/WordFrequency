using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordFrequency
{
    public class Log
    {
        public static void Write(Exception exception)
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "log.txt");
           
            try
            {
                using (var writer = new StreamWriter(logPath, true))
                {
                    writer.WriteLine("Time: {0} Error occurred: {1}  Message: {2}", DateTime.Now, exception.StackTrace, exception.Message);
                }

                Console.WriteLine(exception.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong");
            }
        }
    }
}
