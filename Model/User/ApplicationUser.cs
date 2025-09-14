using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace P2P.Model.User
{
    public class ApplicationUser : IdentityUser
    {
        public required string EmpId { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }

        public override string? Email { get; set; }

        public bool LoginEnabled { get; set; } = true;

        public string Avatar { get; set; } = "default.jpg";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? CreateBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string? UpdateBy { get; set; }

        

        public ICollection<ApplicationUserRole>? UserRoles { get; set; }

    }
}