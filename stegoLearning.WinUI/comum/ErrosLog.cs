using System;
using System.IO;
using Windows.Storage;

namespace stegoLearning.WinUI.comum
{
    internal static class ErrosLog
    {
        public static void EscreverErroEmLog(Exception exception)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            string path = storageFolder.Path;
            string file = "erros_log.txt";

            using (StreamWriter streamWriter = File.AppendText($"{path}\\{file}"))
            {
                streamWriter.Write("\r\nLog Entry: ");
                streamWriter.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                streamWriter.WriteLine("\nMESSAGE:");
                streamWriter.WriteLine($"{exception.Message}");
                streamWriter.WriteLine("\nSTACKTRACE:");
                streamWriter.WriteLine($"{exception.StackTrace}");
                streamWriter.WriteLine("\n-------------------------------");
            }
        }
    }
}
