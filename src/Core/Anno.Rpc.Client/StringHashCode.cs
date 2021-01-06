using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Anno
{
    public static class StringHashCode
    {
        /// <summary>
        /// 计算文件的哈希值
        /// </summary>
        /// <param name="buffer">被操作的源数据流</param>
        /// <param name="algo">加密算法</param>
        /// <returns>哈希值16进制字符串</returns>
        public static string HashCode(this string str, string algo = "md5")
        {
            byte[] buffer = Encoding.UTF8.GetBytes(str);
            HashAlgorithm hashAlgorithm;
            switch (algo)
            {
                case "sha1":
                    hashAlgorithm = new SHA1CryptoServiceProvider();
                    break;
                case "md5":
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
                default:
                    hashAlgorithm = new MD5CryptoServiceProvider();
                    break;
            }

            var hash = hashAlgorithm.ComputeHash(buffer);
            var sb = new StringBuilder();
            foreach (var t in hash)
            {
                sb.Append(t.ToString("x2"));
            }

            return sb.ToString();
        }
    }
    public class JudgeIsDebug
    {
        private JudgeIsDebug() { }
        private static bool? isDebug = null;
        private static object isDebuglocker = new object();
        public static bool IsDebug
        {
            get
            {
                if (isDebug == null)
                {
                    lock (isDebuglocker)
                    {
                        if (isDebug != null)
                        {
                            return isDebug ?? true;
                        }
                        isDebug = JudgeDebug();
                    }
                }
                return isDebug ?? true;
            }
        }
        private static bool JudgeDebug()
        {
            Assembly assembly = Assembly.GetEntryAssembly();
            bool debug = false;
            foreach (var attribute in assembly.GetCustomAttributes(false))
            {
                if (attribute.GetType() == typeof(System.Diagnostics.DebuggableAttribute))
                {
                    if (((System.Diagnostics.DebuggableAttribute)attribute)
                        .IsJITTrackingEnabled)
                    {
                        debug = true;
                        break;
                    }
                }
            }
            return debug;
        }
    }
}
