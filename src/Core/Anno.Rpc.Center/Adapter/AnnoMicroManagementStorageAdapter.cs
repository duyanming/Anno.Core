using Anno.Rpc.Center;
using Anno.Rpc.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Adapter
{
    internal class AnnoMicroManagementStorageAdapter : BaseAdapter
    {
        public AnnoMicroManagementStorageAdapter()
        {

        }
        internal override string Invoke(Dictionary<string, string> command)
        {
            AnnoDataResult result = new AnnoDataResult();
            result.Status = false;
            try
            {
                ThriftConfig tc = ThriftConfig.CreateInstance();
                result.Status = tc.ChangeMicroServiceWeight(command);
            }
            catch (Exception ex)
            {
                result.Data = ex.Message;
            }
            finally { }
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);

        }
    }
}
