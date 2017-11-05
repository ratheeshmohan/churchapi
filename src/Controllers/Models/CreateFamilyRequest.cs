using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Controllers.Models
{

    public class CreateFamilyRequest
    {
        [Required]
        public string FamilyId { get; set; }
        [Required]
        public string LoginEmail { get; set; }
    }
}