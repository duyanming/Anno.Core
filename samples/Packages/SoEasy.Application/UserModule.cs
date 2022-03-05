using System;
using System.Collections.Generic;
using System.Text;

namespace SoEasy.Application
{
    using Anno.EngineData;
    using SoEasy.Application.Po;
    using SoEasy.Application.Repositories;

    public class UserModule : BaseModule
    {
        private UserRepository userRepository;
        public UserModule(UserRepository _userRepository)
        {
            userRepository = _userRepository;
        }
        public UserEntity GetById(int id)
        {
            return userRepository.GetById(id);
        }
    }
}
