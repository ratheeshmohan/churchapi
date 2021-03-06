namespace parishdirectoryapi.Models
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Member
    {
        public string MemberId { get; set; }

        public string ChurchId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public Gender Gender { get; set; }
        public string Phone { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfWedding { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }

        public bool? RevealDateOfBirth { get; set; }
        public bool? RevealDateOfWedding { get; set; }
        public bool? RevealPhone { get; set; }
        public bool? RevealEmail { get; set; }
    }
}