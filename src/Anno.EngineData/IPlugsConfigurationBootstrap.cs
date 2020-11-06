using System;
using System.Collections.Generic;
using System.Text;

namespace Anno.EngineData
{
    /// <summary>
    /// 插件启动配置
    /// </summary>
    public interface IPlugsConfigurationBootstrap
    {
        /// <summary>
        ///IOC之前
        /// </summary>
        void PreConfigurationBootstrap();
        /// <summary>
        /// 插件启动配置
        /// </summary>
        void ConfigurationBootstrap();
    }
}
