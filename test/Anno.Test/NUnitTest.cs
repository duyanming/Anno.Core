using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using NUnit;
using NUnit.Framework;

namespace Anno.Test
{
    [TestFixture]
    public class NUnitTest
    {
        [Test(Description = "登陆")]
        [Category("Login")]
        [TestCase("yrm", "123456")]
        public void TesLogin(string account, string pwd)
        {

        }
        public string PredicateNewTest(Predicate<string> item)
        {
            string[] arrayString = new string[]
            {
                "One","Two","Three","Four","Fice","Six","Seven","Eight","Nine","Ten"
            };
            foreach (string str in arrayString)
            {
                if (item(str))
                {
                    return str;
                }
            }
            return null;
        }
        [Test]
        public void PredicateNewTest()
        {
            string str = PredicateNewTest((c) => { return c.Length > 3; });
        }

        /// <summary>
        /// Linq之Expression进阶
        /// </summary>
        [Test]
        public void ExpressionsMehod()
        {
            //string dir = BaseDir;
            //创建表达式树
            Expression<Func<int, bool>> expTree = num => num >= 5;
            //获取输入参数
            ParameterExpression param = expTree.Parameters[0];
            //获取lambda表达式主题部分
            BinaryExpression body = (BinaryExpression)expTree.Body;
            //获取num>=5的右半部分
            ConstantExpression right = (ConstantExpression)body.Right;
            //获取num>=5的左半部分
            ParameterExpression left = (ParameterExpression)body.Left;
            //获取比较运算符
            ExpressionType type = body.NodeType;
            Console.WriteLine("解析后：{0}   {1}    {2}", left, type, right);

            Expression<Func<string, string, List<string>>> expTreeNew = (x, y) =>
                new List<string>
                {
                    x,y
                };
        }
        [SetUp]
        public  void InitConfig()
        {

        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("我是清理者");
        }
    }
}
