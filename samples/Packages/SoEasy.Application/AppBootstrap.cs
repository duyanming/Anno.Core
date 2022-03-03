using Anno.EngineData;
using System;

namespace SoEasy.Application
{
    public class AppBootstrap : IPlugsConfigurationBootstrap
    {
        public void ConfigurationBootstrap()
        {
            Console.WriteLine("ConfigurationBootstrap:" + this.GetType().FullName);
        }

        public void PreConfigurationBootstrap()
        {
            Console.WriteLine("PreConfigurationBootstrap:" + this.GetType().FullName);
        }
    }
}
