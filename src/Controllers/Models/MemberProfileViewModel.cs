using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Controllers.Models
{
    public class MemberProfileViewModel
    {
        [Required]
        public string MemberId { get; set; }
        public string NickName { get; set; }
        public string Phone { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfWedding { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
    }
}