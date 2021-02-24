/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/11/6 13:09:28 
Functional description： LogTest
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest
{
    public class LogTest
    {
        public void Handle() {
            Anno.Log.Log.Debug("debug");

            Anno.Log.Log.DebugConsole("debug");
            Anno.Log.Log.Info("debug",typeof(LogTest));
        }
    }
}
