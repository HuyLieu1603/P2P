using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using P2P.Repositories.UserService;

namespace P2P.Controllers
{
    public class UsersController
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
    }
}