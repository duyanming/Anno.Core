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
    public class AnnoDataBase
    {
        private static LiteDatabase db;
        private static object locker = new object();
        public static LiteDatabase Db
        {
            get
            {
                if (db == null)
                {
                    lock (locker)
                    {
                        if (db == null)
                        {
                            db = new LiteDatabase("Anno.db");
                        }
                        return db;
                    }
                }
                else
                {
                    return db;
                }
            }
        }
    }
}
