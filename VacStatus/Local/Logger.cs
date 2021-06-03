using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VacStatus.Local
{
    class Logger
    {
        private readonly string _path = @$"{AppDomain.CurrentDomain.BaseDirectory}..\..\..\";

        public void Log(string message, LogType logType)
        {
            var fileName = "log.txt";

            string logHeader = $"{DateTime.Now}";

            var logTypeText = logType.ToString();

            var logMessage = $"[{logHeader}] [{logTypeText}] {message} {Environment.NewLine}";

            File.AppendAllText(Path.Combine(_path, fileName), logMessage);
        }

        public enum LogType
        {
            Error = 1,
            Info = 2,
            Ban = 3,
            Warn = 4
        }
    }
}
