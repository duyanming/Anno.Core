/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/10/26 17:31:07 
Functional description： AnnoKV
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    public class AnnoKV
    {
        public AnnoKV() { }
        public AnnoKV(string key,string value) {
            this.Id = key;
            this.Value = value;
        }
        /// <summary>
        /// 键(Key)
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 值
        /// </summary>
        public string Value { get; set; }
    }
}
