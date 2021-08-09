/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/10/27 10:46:21 
Functional description： GetDataBase
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Rpc.Storage
{
    using LiteDB;
    using System.IO;

    public class AnnoDataBase
    {
        private static string connectionString = "Filename=Anno.db;Connection=Shared";
        private static bool dbExists = false;
        public static LiteDatabase Db
        {
            get
            {
                LiteDatabase db = null;
                if (!dbExists)
                {
                    if (!File.Exists(connectionString))
                    {
                        db = new LiteDatabase(connectionString);
                        var colKv = db.GetCollection<AnnoKV>();
                        colKv.EnsureIndex(x => x.Id,true);
                        colKv.EnsureIndex(x => x.Value);

                        var colDoc = db.GetCollection<AnnoData>();
                        colDoc.EnsureIndex(x => x.Id,true);
                        colDoc.EnsureIndex(x => x.Value);
                    }
                    else
                    {
                        db = new LiteDatabase(connectionString);
                    }
                }
                else
                {
                    db = new LiteDatabase(connectionString);
                }
                return db;
            }
        }
    }
}
