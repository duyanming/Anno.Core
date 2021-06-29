using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Anno.Log
{
    using Newtonsoft.Json;
    /// <summary>
    /// 日志工具
    /// </summary>
    public static class Log
    {
        /// <summary>
        /// 日志锁
        /// </summary>
        private static readonly object Locker = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Info(object message, Type type = null)
        {
            WriteLogSync(message, type, LogType.Info);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Debug(object message, Type type = null)
        {
            if (JudgeIsDebug.IsDebug)
            {
                WriteLogSync(message, type, LogType.Debug);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void DebugConsole(object message)
        {
            if (JudgeIsDebug.IsDebug)
            {
                WriteLine(message, (ConsoleColor)new Random().Next(1, 14));
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void WriteLine(object message, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}]: {message}");
            Console.ResetColor();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void ConsoleWriteLine(string message)
        {
            Console.ForegroundColor = (ConsoleColor)new Random().Next(1, 14);
            Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}]: {message}");
            Console.ResetColor();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Warn(object message, Type type = null)
        {
            WriteLogSync(message, type, LogType.Warn);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Error(object message, Type type = null)
        {
            WriteLogSync(message, type, LogType.Error);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Fatal(object message, Type type = null)
        {
            WriteLogSync(message, type, LogType.Fatal);
        }
        /// <summary>
        /// 分布式追踪
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Trace(object message, Type type = null)
        {
            WriteLogSync(message, type, LogType.Trace);
        }
        /// <summary>
        /// 写日志异步 队列
        /// </summary>
        /// <param name="logStr">消息</param>
        /// <param name="type">类型</param>
        /// <param name="logType">日志类型</param>
        static void WriteLogSync(object logStr, Type type = null, LogType logType = LogType.Default)
        {
            var msg = string.Empty;
            try
            {
                msg = JsonConvert.SerializeObject(logStr);
            }
            catch
            {
                msg = logStr.ToString();
            }
            //当前线程ID
            string threadId = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString();

            /*
             * 写日志的时候不阻塞 业务
             */
            Task.Run(() =>
            {
                //防止文件占用
                lock (Locker)
                {
                    string logFile = DateTime.Today.ToString("yyyy-MM-dd");
                    //目录
                    string dir = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "log");
                    string logDir = string.Concat(dir, Path.DirectorySeparatorChar, logType.ToString(), logFile, ".log");
                    FileStream file = null;
                    if (!File.Exists(logDir))
                    {

                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        file = new FileStream(logDir, FileMode.CreateNew);
                    }
                    else
                    {
                        file = new FileStream(logDir, FileMode.Append);
                    }
                    StreamWriter writer = new StreamWriter(file, Encoding.UTF8);
                    /*
                     * 添加空行
                     */
                    writer.WriteLine("------------------------------------LOG分隔符---------------------------------------------");

                    writer.WriteLine($"记录时间:    {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ");
                    writer.WriteLine($"线程ID:       [{threadId}] ");
                    writer.WriteLine($"日志等级:    {logType.ToString()} ");
                    if (type != null)
                        writer.WriteLine($"类型:          {type?.FullName} ");

                    writer.WriteLine(msg);

                    writer.Flush();
                    writer.Close();
                    file.Close();
                }
            });
        }
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 默认
        /// </summary>
        Default,
        /// <summary>
        /// 信息
        /// </summary>
        Info,
        /// <summary>
        /// 调试
        /// </summary>
        Debug,
        /// <summary>
        /// 警告
        /// </summary>
        Warn,
        /// <summary>
        /// 错误
        /// </summary>
        Error,
        /// <summary>
        /// 重要
        /// </summary>
        Fatal,
        /// <summary>
        /// 分布式追踪
        /// </summary>
        Trace
    }
}
