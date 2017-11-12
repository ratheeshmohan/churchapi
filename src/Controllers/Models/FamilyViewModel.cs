using parishdirectoryapi.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Controllers.Models
{
    public class FamilyMemberViewModel
    {
        public FamilyRole Role { get; set; }
        public MemberViewModel Member { get; set; }
    }

    public class FamilyViewModel
    {
        [Required]
        public string FamilyId { get; set; }
        [Required]
        public string LoginEmail { get; set; }

        public FamilyProfileViewModel Profile { get; set; }
        public IEnumerable<FamilyMemberViewModel> Members { get; set; }
    }
}