using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Server
{
    /// <summary>
    /// 参数助手
    /// </summary>
    public static class ArgsValue
    {
        /// <summary>
        /// 根据名称 获取参数字符串
        /// </summary>
        /// <param name="name">名称 例如 【-p】</param>
        /// <param name="args">字符串数组</param>
        /// <returns></returns>
        public static string GetValueByName(string name,string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (string.Equals(args[i].Trim(), name.Trim(), StringComparison.CurrentCultureIgnoreCase)&& args.Length>=(i+1))
                {
                    return args[i + 1];
                }
            }
            return null;
        }
    }
}
