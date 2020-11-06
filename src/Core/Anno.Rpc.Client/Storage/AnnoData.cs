using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    public class AnnoData
    {
        /// <summary>
        /// 来自哪个App
        /// </summary>
        public string App { get; set; }
        /// <summary>
        /// 键(Key)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
    public class DataValue
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public List<ParametersValue> Parameters { get; set; }
    }
    public class ParametersValue
    {
        public string Name { get; set; }
        public string Desc { get; set; }
        public int Position { get; set; }
        public string ParameterType { get; set; }

    }
}
