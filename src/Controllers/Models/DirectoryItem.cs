using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using parishdirectoryapi.Models;

namespace parishdirectoryapi.Controllers.Models
{
    public class DirectoryMember
    {
        public MemberViewModel Member { get; set; }
        public FamilyRole Role { get; set; }
    }

    public class DirectoryItem
    {
        [Required]
        public string FamilyId { get; set; }
        public Address Address { get; set; }
        public Parish HomeParish { get; set; }
        public string PhotoUrl { get; set; }
        public IEnumerable<DirectoryMember> Members { get; set; }
    }
}
