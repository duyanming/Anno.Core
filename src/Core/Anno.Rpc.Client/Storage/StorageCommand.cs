using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    public static class StorageCommand
    {
        /// <summary>
        /// 存储指令Key
        /// </summary>
        public const string COMMAND = "#ANNOCOMMAND#";
        /// <summary>
        /// KV存储命令
        /// </summary>
        public const string KVCOMMAND = "KV";
        /// <summary>
        /// API文档存储命令
        /// </summary>
        public const string APIDOCCOMMAND = "APIDOC";

        /// <summary>
        /// 管理微服务信息
        /// </summary>
        public const string ANNOMICROSERVICE = "ANNOSVR";
    }
}
