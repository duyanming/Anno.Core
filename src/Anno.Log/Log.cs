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
    using System.Collections.Concurrent;

    /// <summary>
    /// 日志工具
    /// </summary>
    public static class Log
    {
        private readonly static object writeLineLocker = new object();

        private static bool _defaultRuning = false;
        private static ConcurrentQueue<LogDataInfo> _defaultLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _infoRuning = false;
        private static ConcurrentQueue<LogDataInfo> _infoLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _debugRuning = false;
        private static ConcurrentQueue<LogDataInfo> _debugLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _warnRuning = false;
        private static ConcurrentQueue<LogDataInfo> _warnLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _errorRuning = false;
        private static ConcurrentQueue<LogDataInfo> _errorLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _fatalRuning = false;
        private static ConcurrentQueue<LogDataInfo> _fatalLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _traceRuning = false;
        private static ConcurrentQueue<LogDataInfo> _traceLog = new ConcurrentQueue<LogDataInfo>();

        private static bool _annoRuning = false;
        private static ConcurrentQueue<LogDataInfo> _annoLog = new ConcurrentQueue<LogDataInfo>();

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
            lock (writeLineLocker)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($" [{DateTime.Now:HH:mm:ss:ffff}]: ");
                Console.ForegroundColor = color;
                Console.WriteLine($"{message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 没有开头 [{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}]:
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void WriteLineNoDate(object message, ConsoleColor color)
        {
            lock (writeLineLocker)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($" {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public static void WriteLineNoDate(string message)
        {
            WriteLineNoDate(message, ConsoleColor.White);
        }
        /// <summary>
        /// 没有开头 [{DateTime.Now:yyyy-MM-dd HH:mm:ss:ffff}]:
        /// </summary>
        /// <param name="message"></param>
        /// <param name="color"></param>
        public static void WriteLineAlignNoDate(object message, ConsoleColor color = ConsoleColor.White)
        {
            lock (writeLineLocker)
            {
                Console.ForegroundColor = color;
                Console.WriteLine($"                  {message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// 可以替换为WriteLineNoDate
        /// </summary>
        /// <param name="message"></param>
        [Obsolete("请使用WriteLineNoDate")]
        public static void ConsoleWriteLine(string message)
        {
            WriteLineNoDate($"[{DateTime.Now:HH:mm:ss:ffff}]: {message}");
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
        /// Anno框架日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Anno(object message, Type type = null)
        {
            WriteLogSync(message, type, LogType.Anno);
        }
        /// <summary>
        /// AnnoRpc 框架日志
        /// </summary>
        /// <param name="message"></param>
        /// <param name="type"></param>
        public static void Anno(string message)
        {
            Anno(message, null);
        }
        /// <summary>
        /// 写日志异步 队列
        /// </summary>
        /// <param name="logStr">消息</param>
        /// <param name="type">类型</param>
        /// <param name="logType">日志类型</param>
        static void WriteLogSync(object logStr, Type type = null, LogType logType = LogType.Default)
        {
            switch (logType)
            {
                case LogType.Info:
                    _infoLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                case LogType.Debug:
                    _debugLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                case LogType.Warn:
                    _warnLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                case LogType.Error:
                    _errorLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                case LogType.Fatal:
                    _fatalLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                case LogType.Trace:
                    _traceLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                case LogType.Anno:
                    _annoLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
                default:
                    _defaultLog.Enqueue(new LogDataInfo() { logStr = logStr, type = type, threadId = System.Threading.Thread.CurrentThread.ManagedThreadId, logType = logType });
                    break;
            }
            switch (logType)
            {
                case LogType.Info:
                    if (_infoRuning) return;
                    break;
                case LogType.Debug:
                    if (_debugRuning) return;
                    break;
                case LogType.Warn:
                    if (_warnRuning) return;
                    break;
                case LogType.Error:
                    if (_errorRuning) return;
                    break;
                case LogType.Fatal:
                    if (_fatalRuning) return;
                    break;
                case LogType.Trace:
                    if (_traceRuning) return;
                    break;
                case LogType.Anno:
                    if (_annoRuning) return;
                    break;
                default:
                    if (_defaultRuning) return;
                    break;
            }
            WriteFile(logType);
        }

        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="logType"></param>
        static Task WriteFile(LogType logType)
        {
            return Task.Run(() =>
            {
                string logFile = DateTime.Today.ToString("yyyy-MM-dd");
                //目录
                string dir = string.Concat(AppDomain.CurrentDomain.BaseDirectory, "log");
                string logDir = string.Concat(dir, Path.DirectorySeparatorChar, logType.ToString(), logFile, ".log");
                FileStream file = null;
                lock (Locker)
                {
                    try
                    {
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
                    }
                    catch { return; }
                }
                StreamWriter writer = new StreamWriter(file, Encoding.UTF8);

                LogDataInfo log;
                switch (logType)
                {
                    case LogType.Info:
                        try
                        {
                            _infoRuning = true;
                            while (_infoLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _infoRuning = false;
                        }

                        break;
                    case LogType.Debug:
                        try
                        {
                            _debugRuning = true;
                            while (_debugLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _debugRuning = false;
                        }

                        break;
                    case LogType.Warn:
                        try
                        {
                            _warnRuning = true;
                            while (_warnLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _warnRuning = false;
                        }
                        break;
                    case LogType.Error:
                        try
                        {
                            _errorRuning = true;
                            while (_errorLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _errorRuning = false;
                        }
                        break;
                    case LogType.Fatal:
                        try
                        {
                            _fatalRuning = true;
                            while (_fatalLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _fatalRuning = false;
                        }
                        break;
                    case LogType.Trace:
                        try
                        {
                            _traceRuning = true;
                            while (_traceLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _traceRuning = false;
                        }
                        break;

                    case LogType.Anno:
                        try
                        {
                            _annoRuning = true;
                            while (_annoLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _annoRuning = false;
                        }
                        break;
                    default:
                        try
                        {
                            _defaultRuning = true;
                            while (_defaultLog.TryDequeue(out log))
                            {
                                WriteLogFile(log, writer);
                            }
                        }
                        catch (Exception) { }
                        finally
                        {
                            writer.Flush();
                            writer.Close();
                            file.Close();
                            _defaultRuning = false;
                        }
                        break;
                }

            });
        }

        static void WriteLogFile(LogDataInfo log, StreamWriter writer)
        {
            var msg = string.Empty;
            if (log.logStr is string)
            {
                msg = log.logStr.ToString();
            }
            else if (log.logStr is null) {
                msg = "null";
            }
            else
            {
                try
                {
                    msg = JsonConvert.SerializeObject(log.logStr);
                }
                catch
                {
                    msg = log.logStr.ToString();
                }
            }
            /*
             * 添加空行
             */
            writer.WriteLine("------------------------------------LOG分隔符---------------------------------------------");

            writer.WriteLine($"{"记录时间:".PadRight(10, ' ')} {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ");
            writer.WriteLine($"{"线程标识:".PadRight(10, ' ')} [{log.threadId}] ");
            writer.WriteLine($"{"日志等级:".PadRight(10, ' ')} {log.logType} ");
            if (log.type != null)
                writer.WriteLine($"{"日志类型:".PadRight(10, ' ')} {log.type.FullName} ");

            writer.WriteLine(msg);
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
        Trace,
        /// <summary>
        /// Anno框架级别的日志
        /// </summary>
        Anno
    }
}
