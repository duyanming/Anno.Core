/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/10/26 17:41:52 
Functional description： KvStorage
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    using LiteDB;
    public class KvStorage
    {
        private static LiteDatabase db;
        private static ILiteCollection<AnnoKV> col;
        public KvStorage()
        {
            if (db == null)
            {
                db = AnnoDataBase.Db;
                col = db.GetCollection<AnnoKV>();
                col.EnsureIndex(x => x.Id);
                col.EnsureIndex(x => x.Value);
            }
        }

        internal string Invoke(Dictionary<string, string> input)
        {
            AnnoDataResult result = new AnnoDataResult();
            result.Status = false;
            try
            {
                if (input.ContainsKey(KVCONST.Opt))
                {
                    switch (input[KVCONST.Opt])
                    {
                        case KVCONST.InsertBatch:
                            var datas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnnoKV>>(input[KVCONST.Data]);
                            result.Data = col.InsertBulk(datas);
                            result.Status = true;
                            break;
                        case KVCONST.InsertOne:
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoKV>(input[KVCONST.Data]);
                            result.Data = col.Insert(data);
                            result.Status = true;
                            break;
                        case KVCONST.Update:
                            data = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoKV>(input[KVCONST.Data]);
                            result.Data = col.Update(data);
                            result.Status = true;
                            break;
                        case KVCONST.Upsert:
                            data = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoKV>(input[KVCONST.Data]);
                            result.Data = col.Upsert(data);
                            result.Status = true;
                            break;
                        case KVCONST.UpsertBatch:
                            datas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnnoKV>>(input[KVCONST.Data]);
                            result.Data = col.Upsert(datas);
                            result.Status = true;
                            break;
                        case KVCONST.Delete:
                            if (input.ContainsKey(KVCONST.Id))
                            {
                                result.Data = col.Delete(input[KVCONST.Id]);
                                result.Status = true;
                            }
                            else
                            {
                                result.Data = "Please provide Id.";
                            }
                            break;
                        case KVCONST.FindById:
                            if (input.ContainsKey(KVCONST.Id))
                            {
                                string id = input[KVCONST.Id];
                                result.Data = col.FindById(id);
                                result.Status = true;
                            }
                            else
                            {
                                result.Data = "Please provide Id.";
                            }
                            break;
                        case KVCONST.FindAll:
                            result.Data = col.FindAll();
                            result.Status = true;
                            break;
                        default:
                            result.Status = false;
                            result.Msg = "Undefined operations";
                            break;
                    }
                }
                else
                {
                    result.Msg = "Undefined operations";
                }
            }
            catch (Exception ex)
            {
                result.Data = ex.Message;
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(result);
        }
    }
}
