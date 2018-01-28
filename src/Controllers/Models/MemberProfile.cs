
namespace parishdirectoryapi.Controllers.Models
{
    public class MemberProfile
    {
        public string NickName { get; set; }
        /*    public string Phone { get; set; }
            public string EmailId { get; set; }*/
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public bool? RevealDateOfBirth { get; set; }
        public bool? RevealDateOfWedding { get; set; }
        public bool? RevealPhone { get; set; }
        public bool? RevealEmail { get; set; }
    }
}