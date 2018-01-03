using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using parishdirectoryapi.Models;

namespace parishdirectoryapi.Controllers.Models
{
    public class FamilyViewModel
    {
        [Required]
        public string FamilyId { get; set; }
        public Address Address { get; set; }
        public Parish HomeParish { get; set; }
        public string PhotoUrl { get; set; }
        public List<FamilyMember> Members { get; set; }
    }
}
