using System;
using System.Collections.Generic;

namespace TsGui.Foundation
{
    public interface ILogger
    {
        void AddLog(LogData logData);
    }

    public enum LogLevel
    {
        Low    = 0,
        Middle = 1,
        High   = 2
    }

    public class LogData
    {
        public LogData(string message, LogLevel logLevel)
        {
            Message = message;
            LogLevel = logLevel;
        }

        public string Message { get; protected set; }
        public LogLevel LogLevel { get; protected set; }
    }

    public class ConsoleLogger : ILogger
    {
        private readonly IReadOnlyDictionary<LogLevel, ConsoleColor> _colorTable = null;

        private class ConsoleWrapper
        {
            public static ConsoleWrapper Instance { get; } = new ConsoleWrapper();

            public ConsoleColor ConsoleColor
            {
                get => Console.ForegroundColor;
                set => Console.ForegroundColor = value;
            }
        }

        public ConsoleLogger()
            : this(null)
        {

        }

        public ConsoleLogger(IReadOnlyDictionary<LogLevel, ConsoleColor> table )
        {
            if (table is null)
            {
                var dictionary = new Dictionary<LogLevel, ConsoleColor>
                {
                    [LogLevel.Low] = ConsoleColor.White,
                    [LogLevel.Middle] = ConsoleColor.Yellow,
                    [LogLevel.High] = ConsoleColor.Red
                };

                table = dictionary;
            }
            _colorTable = table;
        }

        public void AddLog(LogData logData)
        {
            using (ConsoleWrapper.Instance.ToUndoDisposable(x => x.ConsoleColor, _colorTable[logData.LogLevel]))
            {
                Console.WriteLine(logData.Message);
            }
        }
    }
}