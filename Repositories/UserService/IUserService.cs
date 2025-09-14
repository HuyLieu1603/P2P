using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dashboard.Common;
using P2P.ViewModel.User;

namespace P2P.Repositories.UserService
{
    public interface IUserService
    {
        public Task<DataResult<bool>> CreateUserAsync(CreateUserDTO createUserDTO);
    }
}