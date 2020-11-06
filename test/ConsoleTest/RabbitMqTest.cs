using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ConsoleTest.MqTest;
using Anno.EventBus;

namespace ConsoleTest
{
    class RabbitMqTest
    {
        public void Handle()
        {
            //IEventBus bus = Anno.EventBus.RabbitMQ.RabbitMQEventBus.Instance;
            IEventBus bus = EventBus.Instance;
            bus.SubscribeAll();
            //Notice notice = new Notice()
            //{
            //    Id = 1100,
            //    EventSource = this,
            //    Name = "杜燕明",
            //    Msg = "后天放假，祝节假日快乐！"
            //};

            //TT tt = new TT()
            //{
            //    Id = 1100,
            //    EventSource = notice,
            //    Name = "TT杜燕明",
            //    Msg = "TT后天放假，祝节假日快乐！"
            //};
            //bus.Publish(notice);

            //bus.Publish(tt);

        }
    }
}
