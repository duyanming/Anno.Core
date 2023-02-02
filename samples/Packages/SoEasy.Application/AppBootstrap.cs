using Anno.EngineData;
using Anno.Loader;
using SoEasy.Application.Po;
using SoEasy.Application.Repositories;
using System;

namespace SoEasy.Application
{
    public class AppBootstrap : IPlugsConfigurationBootstrap
    {
        public void ConfigurationBootstrap()
        {
            var userRepository = IocLoader.Resolve<IBaseRepository<UserEntity>>();
            Console.WriteLine($"ConfigurationBootstrap:{GetType().FullName},{userRepository.GetType().FullName}");
        }

        public void PreConfigurationBootstrap()
        {
            //var services = IocLoader.GetServiceDescriptors();
            Console.WriteLine($"PreConfigurationBootstrap:{GetType().FullName}");
        }
    }
}
