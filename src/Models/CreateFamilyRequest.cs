using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Models
{

    public class CreateFamilyRequest
    {
        public string ChurchId { get; set; }
        [Required]
        public string FamilyId { get; set; }
        [Required]
        public string LoginEmail { get; set; }
    }
}