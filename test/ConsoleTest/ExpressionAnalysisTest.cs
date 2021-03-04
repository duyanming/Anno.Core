using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anno.EngineData;

namespace ConsoleTest
{
    public class ExpressionAnalysisTest
    {
        public void Handle()
        {
            string jsonSql = string.Empty;
            jsonSql = @"{'rules':[{'field':'AppNameTarget','op':'like','value':'GetUsrFc','type':'string'}],'groups':[{'rules':[{'field':'Uname','op':'like','value':'GetUsrFc','type':'string'}],'op':'or'},{'rules':[{'field':'Askchannel','op':'like','value':'GetUsrFc','type':'string'}],'op':'or'},{'rules':[{'field':'Askrouter','op':'like','value':'GetUsrFc','type':'string'}],'op':'or'},{'rules':[{'field':'Askmethod','op':'like','value':'GetUsrFc','type':'string'}],'op':'or'},{'rules':[{'field':'Ip','op':'like','value':'GetUsrFc','type':'string'}],'op':'or'}],'op':'or'}";
            var group = Newtonsoft.Json.JsonConvert.DeserializeObject<Group>(jsonSql);
            var sql = ExpressionAnalysis.TransmitFilter(group, "__table__");
            Console.WriteLine(sql);
        }
    }
}
