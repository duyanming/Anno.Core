using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Client
{
    /// <summary>
    /// 缓存数据服务
    /// </summary>
    public class MicroCache
    {
        private string _id = string.Empty;
        private Micro _mi = null;
        /// <summary>
        /// 最后访问时间
        /// </summary>
        public DateTime LasTime { get; internal set; }
        /// <summary>
        /// 服务
        /// </summary>
        public Micro Mi
        {
            get { return _mi; }
            internal set
            {
                _mi = value;
                _id = $"{_mi.Ip }:{ _mi.Port}";
            }
        }
        /// <summary>
        /// 功能列表
        /// </summary>
        public List<string> Tags { get; internal set; }

        /// <summary>
        /// IP端口唯一标志（IP+Port）
        /// </summary>
        public string Id => _id;
    }
}
