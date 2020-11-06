/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/11/6 11:14:07 
Functional description： JudgeIsDebug
******************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Anno.Log
{
    internal class JudgeIsDebug
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
                            return isDebug??true;
                        }
                        isDebug = JudgeDebug();
                    }
                }
                return isDebug??true;
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
