using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace P2P.ViewModel.User
{
    public class CreateUserDTO
    {
        public required string EmpId { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public bool LoginEnabled { get; set; } = true;
        public string? Password { get; set; }
    }
}