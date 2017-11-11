using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Controllers.Models
{

    public class Family
    {
        [Required]
        public string FamilyId { get; set; }
        [Required]
        public string LoginEmail { get; set; }

        public FamilyProfile FamilyProfile { get; set; }
        public IEnumerable<FamilyMemeber> Members { get; set; }
    }
}