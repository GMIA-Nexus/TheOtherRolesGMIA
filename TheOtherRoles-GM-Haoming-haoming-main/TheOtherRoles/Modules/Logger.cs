using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using BepInEx;
using LogLevel = BepInEx.Logging.LogLevel;
using System.Text;

namespace TheOtherRoles.Patches
{
    class Logger
    {
        private static ManualLogSource logSource { get; set; }

        internal static void SetLogSource(ManualLogSource Source)
        {
            if (ConsoleManager.ConsoleEnabled) System.Console.OutputEncoding = Encoding.UTF8;
            logSource = Source;
        }

        public static void info(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Info);
        public static void message(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Message);
        public static void warn(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Warning);
        public static void error(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Error);
        public static void debug(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Debug);
        public static void fatal(object text, string Tag = "") => SendLog(text.ToString(), Tag, LogLevel.Fatal);

        public static void SendLog(string text, string tag = "", LogLevel logLevel = LogLevel.Info)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            if (!string.IsNullOrWhiteSpace(tag)) text = $"[{time}] [{tag}] {text}";
            else text = $"[{time}] {text}";

            switch (logLevel)
            {
                case LogLevel.Message:
                    logSource.LogMessage(text);
                    break;
                case LogLevel.Error:
                    logSource.LogError(text);
                    break;
                case LogLevel.Warning:
                    logSource.LogWarning(text);
                    break;
                case LogLevel.Fatal:
                    logSource.LogFatal(text);
                    break;
                case LogLevel.Info:
                    logSource.LogInfo(text);
                    break;
                case LogLevel.Debug:
                    logSource.LogDebug(text);
                    break;
                default:
                    logSource.LogInfo(text);
                    break;
            }
        }

        public static void FastLog(LogLevel errorLevel, object @object)
        {
            var Logger = logSource;
            var Message = @object as string;
            switch (errorLevel)
            {
                case LogLevel.Message:
                    Logger.LogMessage(Message);
                    break;
                case LogLevel.Error:
                    Logger.LogError(Message);
                    break;
                case LogLevel.Warning:
                    Logger.LogWarning(Message);
                    break;
                case LogLevel.Fatal:
                    Logger.LogFatal(Message);
                    break;
                case LogLevel.Info:
                    Logger.LogInfo(Message);
                    break;
                case LogLevel.Debug:
                    Logger.LogDebug(Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(errorLevel), errorLevel, null);
            }
        }

        public static void LogObject(object @object)
        {
            FastLog(LogLevel.Error, @object);
        }
    }
}
