using System;
using System.Collections.Generic;
using System.Text;
using Anno.EventBus;

namespace ConsoleTest.MqTest
{
    public class MailSend : IEventHandler<Notice>
    {
        public void Handler(Notice entity)
        {
            Console.WriteLine($"你好{entity.Name},{entity.Msg}");
        }
    }
    public class Mailend : IEventHandler<Notice>
    {
        public void Handler(Notice entity)
        {
            Console.WriteLine($"消息发送完毕！");
        }
    }
}
