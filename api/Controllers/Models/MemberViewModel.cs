using parishdirectoryapi.Models;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Controllers.Models
{
    public class MemberViewModel
    {
        public string MemberId { get; internal set; }

        [Required]
        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string LastName { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public string NickName { get; set; }
        public string Phone { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfWedding { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
    }
}