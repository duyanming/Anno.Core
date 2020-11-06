using System;
using System.Collections.Generic;
using System.Text;
using Anno.EventBus;

namespace ConsoleTest.MqTest
{
    public class TT:EventData
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Msg { get; set; }
    }
}
