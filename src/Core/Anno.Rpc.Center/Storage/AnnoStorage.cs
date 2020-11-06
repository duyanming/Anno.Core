using System;
using System.Collections.Generic;
using System.Text;
using LiteDB;

namespace Anno.Rpc.Storage
{
    public class AnnoStorage
    {
        private static LiteDatabase db;
        private static ILiteCollection<AnnoData> col;
        public AnnoStorage()
        {
            if (db == null)
            {
                db = AnnoDataBase.Db;
                col = db.GetCollection<AnnoData>();
                col.EnsureIndex(x => x.Id);
                col.EnsureIndex(x => x.App);
            }
        }

        internal string Invoke(Dictionary<string, string> input)
        {
            AnnoDataResult result = new AnnoDataResult();
            result.Status = false;
            try
            {
                if (input.ContainsKey(CONST.Opt))
                {
                    switch (input[CONST.Opt])
                    {
                        case CONST.InsertBatch:
                            var datas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnnoData>>(input[CONST.Data]);
                            result.Data = col.InsertBulk(datas);
                            result.Status = true;
                            break;
                        case CONST.InsertOne:
                            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoData>(input[CONST.Data]);
                            result.Data = col.Insert(data).AsString;
                            result.Status = true;
                            break;
                        case CONST.Update:
                            data = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoData>(input[CONST.Data]);
                            result.Data = col.Update(data);
                            result.Status = true;
                            break;
                        case CONST.Upsert:
                            data = Newtonsoft.Json.JsonConvert.DeserializeObject<AnnoData>(input[CONST.Data]);
                            result.Data = col.Upsert(data);
                            result.Status = true;
                            break;
                        case CONST.UpsertBatch:
                            datas = Newtonsoft.Json.JsonConvert.DeserializeObject<List<AnnoData>>(input[CONST.Data]);
                            result.Data = col.Upsert(datas);
                            result.Status = true;
                            break;
                        case CONST.Delete:
                            if (input.ContainsKey(CONST.Id))
                            {
                                result.Data = col.Delete(input[CONST.Id]);
                                result.Status = true;
                            }
                            else
                            {
                                result.Data = "Please provide Id.";
                            }
                            break;
                        case CONST.DeleteByApp:
                            if (input.ContainsKey(CONST.App))
                            {
                                string app = input[CONST.App];
                                result.Data = col.DeleteMany(it => it.App == app);
                                result.Status = true;
                            }
                            else
                            {
                                result.Data = "Please provide App.";
                            }
                            break;
                        case CONST.FindByApp:

                            if (input.ContainsKey(CONST.App))
                            {
                                string app = input[CONST.App];
                                result.Data = col.Find(it => it.App == app);
                                result.Status = true;
                            }
                            else
                            {
                                result.Data = "Please provide App.";
                            }
                            break;
                        case CONST.FindById:
                            if (input.ContainsKey(CONST.Id))
                            {
                                string id = input[CONST.Id];
                                result.Data = col.FindById(id);
                                result.Status = true;
                            }
                            else
                            {
                                result.Data = "Please provide Id.";
                            }
                            break;
                        case CONST.FindAll:
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
