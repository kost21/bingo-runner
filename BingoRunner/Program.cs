using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace BingoRunner
{
    class Program
    {
        private static string errorLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Trace.log");

        private static void WriteLog(LogLevel logLevel, string logMessage)
        {
            string strLevel = string.Empty;

            switch (logLevel)
            {
                case LogLevel.Debug:
                    strLevel = "[DEBUG]:";
                    break;
                case LogLevel.Error:
                    strLevel = "[ERROR]:";
                    break;
                case LogLevel.Fatal:
                    strLevel = "[FATAL]:";
                    break;
                case LogLevel.Info:
                    strLevel = "[INFO]:";
                    break;
                case LogLevel.Warn:
                    strLevel = "[WARNING]:";
                    break;
                default:
                    break;
            }

            try
            {
                using (StreamWriter sw = new StreamWriter(errorLogFilePath, true, Encoding.Default))
                {
                    string message = string.Format("{0} {1} {2}", DateTime.Now.ToString(), strLevel, logMessage);
                    sw.WriteLine(message);
                }
            }
            catch
            {

            }
        }
        static void Main(string[] args)
        {
            try
            {
                WriteLog(LogLevel.Info, "Arguments count: " + args.Count());
                foreach (string arg in args)
                {
                    WriteLog(LogLevel.Info, arg);
                }

                if (args.Count() != 6)
                {
                    WriteLog(LogLevel.Error, "Not enough arguments: " + args.Count());
                    return;
                }

                string exe2pdf = args[0];
                string cmd2pdf = args[1];
                string statusFilePath = args[2];
                string demo2pdf = args[3];
                string startText = args[4];
                string finishText = args[5];

                if (string.IsNullOrWhiteSpace(exe2pdf)
                    || string.IsNullOrWhiteSpace(cmd2pdf)
                    || string.IsNullOrWhiteSpace(statusFilePath)
                    || string.IsNullOrWhiteSpace(demo2pdf)
                    || string.IsNullOrWhiteSpace(startText)
                    || string.IsNullOrWhiteSpace(finishText))
                {
                    WriteLog(LogLevel.Error, "One or more arguments are empty!");
                    return;
                }

                File.WriteAllText(statusFilePath, startText);

                var startInfo = new ProcessStartInfo
                {
                    FileName = exe2pdf,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardInput = true,
                    Arguments = cmd2pdf,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var process = Process.Start(startInfo);
                if (!process.HasExited)
                {
                    if (bool.Parse(demo2pdf) == true)
                    {
                        using (var streamWriter = process.StandardInput)
                        {
                            var message = "1";
                            streamWriter.WriteLine(message);
                            streamWriter.Close();
                        }
                    }

                    process.WaitForExit();
                    process.Close();
                    process.Dispose();
                }

                File.WriteAllText(statusFilePath, finishText);
            }
            catch (Exception ex)
            {
                WriteLog(LogLevel.Error, "Crytical error: " + ex.Message);
            }
        }

        public enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error,
            Fatal
        }
    }
}
