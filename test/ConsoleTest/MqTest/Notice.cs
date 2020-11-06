using Anno.EventBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleTest.MqTest
{
    public class Notice:EventData
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Msg { get; set; }
    }
}
