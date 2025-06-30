using System;
using System.IO;

namespace CsvProcessorApp.Services
{
    public static class FileLogger
    {
        private static readonly string LogFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "log.txt");

        public static void LogUpload(string fileName, string fileType, bool success, string username)
        {
            var logLine = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | User: {username} | FileType: {fileType} | FileName: {fileName} | Success: {success}";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
                File.AppendAllText(LogFilePath, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Logger] Failed to write log: {ex.Message}");
            }
        }
    }
}
