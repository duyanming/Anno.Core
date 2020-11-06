/****************************************************** 
Writer:Du YanMing
Mail:dym880@163.com
Create Date:2020/7/8 18:45:34 
Functional description： AnnoInfoAttribute
******************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.Const.Attribute
{
    /// <summary>
    /// 描述注解
    /// </summary>
    public class AnnoInfoAttribute : System.Attribute
    {
        /// <summary>
        /// 描述
        /// </summary>
        public String Desc { get; set; }
    }
}
