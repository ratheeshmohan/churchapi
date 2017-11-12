using parishdirectoryapi.Models;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Controllers.Models
{
    public class MemberViewModel
    {
        [Required]
        public string MemberId { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string MiddleName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public Sex Gender { get; set; }
        [Required]
        public FamilyRole Role { get; set; }

        public string NickName { get; set; }
        public string Phone { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfWedding { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
    }
}