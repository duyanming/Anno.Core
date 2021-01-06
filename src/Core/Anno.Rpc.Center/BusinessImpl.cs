using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno.Rpc.Storage;
using Anno.Rpc.Adapter;

namespace Anno.Rpc.Center
{
    public class BusinessImpl : BrokerCenter.Iface
    {
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool add_broker(Dictionary<string, string> input)
        {
            ThriftConfig tc = ThriftConfig.CreateInstance();
            return tc.Add(input);
        }
        /// <summary>
        /// 获取微服务地址
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public List<Micro> GetMicro(string channel)
        {
            return Distribute.GetMicro(channel).ToList();
        }

        public string Invoke(Dictionary<string, string> input)
        {
            BaseAdapter adapter;
            string rlt;
            try
            {
                if (input.ContainsKey(StorageCommand.COMMAND))
                {
                    var command = input[StorageCommand.COMMAND];
                    switch (command)
                    {
                        case StorageCommand.KVCOMMAND:
                            adapter = new KvStorageAdapter();
                            break;
                        default:
                            adapter = new ApiDocStorageAdapter();
                            break;
                    }
                    rlt = adapter.Invoke(input);
                }
                else
                {
                    return FailMessage("未知指令,参考[Anno.Rpc.Storage.StorageCommand]下指令。");
                }
            }
            catch (Exception ex)
            {
                return FailMessage(ex.Message);
            }
            return rlt;
        }
        /// <summary>
        /// 构建错误消息Json字符串
        /// </summary>
        /// <param name="message">错误消息</param>
        /// <param name="status">默认False</param>
        /// <returns>"{\"Msg\":\""+message+"\",\"Status\":false,\"Output\":null,\"OutputData\":null}"</returns>
        internal static string FailMessage(string message, bool status = false)
        {
            return "{\"Msg\":\"" + message + "\",\"Status\":" + status.ToString().ToLower() + ",\"Data\":null}";
        }
    }
}
