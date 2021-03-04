using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Anno.EngineData
{
    public class ExpressionAnalysis
    {
        public static string TransmitFilter(Group group,string table)
        {
            group.rules.RemoveAll(r => r.field == "undefined");
            List<Rule> rules = group.rules;
            if (rules == null || rules.Count == 0)
            {
                return string.Empty;
            }
            StringBuilder where = new StringBuilder();
            where.AppendFormat("(");
            foreach (var rule in rules)
            {
                where.Append(GetFilter(rule,table));
                where.Append("  and ");
            }

            where.Remove(where.Length - 5, 5);
            if (group.groups != null && group.groups.Count(g => g.rules.Count(r => r.field != "undefined") > 0) > 0)
            {
                where.AppendFormat($" {group.op} ");
                where.AppendFormat("(");
                int spLength =0;
                group.groups.Where(g => g.rules.Count(r => r.field != "undefined") > 0).ToList().ForEach(g =>
                {
                    where.AppendFormat(TransmitFilter(g, table));
                    var op = $" {g.op} ";
                    spLength = op.Length;
                    where.AppendFormat(op);
                });
                if (group.groups.Count(g => g.rules.Count(r => r.field != "undefined") > 0) > 0)
                {
                    where.Remove(where.Length - spLength, spLength);
                }
                where.AppendFormat(")");
            }
            where.AppendFormat(")");
            return where.ToString();
        }

        private static string GetFilter(Rule rule,string table)
        {
            string where = string.Empty;
            var field = rule.field;
            field = field.Replace(table, ".");
            switch (rule.op)
            {
                case "equal":
                    return $" {field}={GetValue(rule.type, rule.value)} ";
                case "notequal":
                    return $" {field}<>{GetValue(rule.type, rule.value)} ";
                case "startwith":
                    return $" {field} like '{rule.value}%' ";
                case "endwith":
                    return $" {field} like '%{rule.value}' ";
                case "like":
                    return $" {field} like '%{rule.value}%' ";
                case "greater":
                    return $" {rule.field}>{GetValue(rule.type, rule.value)} ";
                case "greaterorequal":
                    return $" {field}>={GetValue(rule.type, rule.value)} ";
                case "less":
                    return $" {field}<{GetValue(rule.type, rule.value)} ";
                case "lessorequal":
                    return $" {field}<={GetValue(rule.type, rule.value)} ";
                case "in":
                    return $" {field} in ({rule.value}) ";
                case "notin":
                    return $" {field} not in ({rule.value}) ";
            }


            return where;
        }

        private static string GetValue(string type, string value)
        {

            if (type == "number" || type == "int" || type == "float")
            {
                return value;
            }
            return $"'{value}'";
        }
    }
    public class Rule
    {
        public string field { get; set; }
        public string op { get; set; }
        public string value { get; set; }
        public string type { get; set; }
    }

    public class Group
    {
        public List<Rule> rules { get; set; }
        public List<Group> groups { get; set; }
        public string op { get; set; }
    }
}
