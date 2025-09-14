using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using P2P.Model.Auth;

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

        //Foreign key
        public int? DepartmentId { get; set; }
        [ForeignKey(nameof(DepartmentId))]
        public virtual Department? Department { get; set; }

        public int? SectionId { get; set; }
        [ForeignKey(nameof(SectionId))]
        public virtual Section? Section { get; set; }

        public int? SiteId { get; set; }
        [ForeignKey(nameof(SiteId))]
        public virtual Site? Site { get; set; }


        public ICollection<ApplicationUserRole>? UserRoles { get; set; }

    }
}