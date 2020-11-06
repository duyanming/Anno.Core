using System;
using System.Collections.Generic;
using System.Text;
using Anno.EventBus;

namespace ConsoleTest.MqTest
{
    public class TTSend : IEventHandler<TT>
    {
        public void Handler(TT entity)
        {
            Console.WriteLine($"你好{entity.Name},{entity.Msg}");
        }
    }
    public class TTend : IEventHandler<TT>
    {
        public void Handler(TT entity)
        {
            Console.WriteLine($"消息发送完毕！");
        }
    }
}
