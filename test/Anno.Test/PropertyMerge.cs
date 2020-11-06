using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Anno.Test
{
    public class PropertyMerge
    {
        dynamic expando = new System.Dynamic.ExpandoObject(); //动态类型字段 可读可写

        public PropertyMerge()
        {
            expando.Id = 1;
            expando.Name = "Test";
        }

        public dynamic Merge(dynamic dyn)
        {
            List<string> fieldList = new List<string>() { "CName", "Age", "Sex" }; //From config or db

            var dic = (IDictionary<string, object>)expando;
            foreach (var fieldItem in fieldList)
            {
                dic[fieldItem] = "set " + fieldItem + " value";
            }
           var ps= dyn.GetType().GetProperties();
            foreach (var p in ps)
            {
                dic[p.Name] = p.GetValue(dyn);
            }
            return expando;
        }
    }
}
