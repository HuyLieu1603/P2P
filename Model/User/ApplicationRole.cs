using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace P2P.Model.User
{
    public class ApplicationRole : IdentityRole
    {

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public ApplicationRole() : base() { }

        public ApplicationRole(string roleName) : base(roleName) { }
        public string? DisplayName { get; set; }

        public string? CategoryName { get; set; }

        public int? CategoryOrder { get; set; }

        public ICollection<ApplicationUserRole> UserRoles { get; set; }

    }

    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public virtual ApplicationUser User { get; set; }

        public virtual ApplicationRole Role { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.


}