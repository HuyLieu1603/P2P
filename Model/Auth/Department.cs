using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace P2P.Model.Auth
{
    public class Department : TimeStampConfig
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string DepartmentName { get; set; }

        public string? Description { get; set; }

        public int SiteId { get; set; }
        [ForeignKey(nameof(SiteId))]
        public virtual Site? Site { get; set; }

        public ICollection<Section>? Sections { get; set; }
    }
}