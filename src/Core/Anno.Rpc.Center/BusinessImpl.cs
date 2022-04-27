using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno.Rpc.Storage;
using Anno.Rpc.Adapter;
using System.Collections.Concurrent;

namespace Anno.Rpc.Center
{
    public class BusinessImpl : BrokerCenter.Iface
    {
        private static ConcurrentDictionary<string, BaseAdapter> _adapters = new ConcurrentDictionary<string, BaseAdapter>();
        public BusinessImpl()
        {
            _adapters.TryAdd(StorageCommand.KVCOMMAND, new KvStorageAdapter());
            _adapters.TryAdd(StorageCommand.APIDOCCOMMAND, new ApiDocStorageAdapter());
            _adapters.TryAdd(StorageCommand.ANNOMICROSERVICE, new AnnoMicroManagementStorageAdapter());
        }
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
            return Distribute.GetMicro(channel);
        }

        public string Invoke(Dictionary<string, string> input)
        {
            BaseAdapter adapter;
            string rlt;
            try
            {
                if (input.ContainsKey(StorageCommand.COMMAND) && _adapters.TryGetValue(input[StorageCommand.COMMAND], out adapter))
                {
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
