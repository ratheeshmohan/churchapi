namespace parishdirectoryapi.Models
{
    public enum Sex
    {
        Male,
        Female
    }

    public class Member
    {
        public string ChurchId { get; set; }
        public string MemberId { get; set; }
        public string FamilyId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public Sex Gender { get; set; }
        public string Phone { get; set; }
        public string EmailId { get; set; }
        public string DateOfBirth { get; set; }
        public string DateOfWedding { get; set; }
        public string FacebookUrl { get; set; }
        public string LinkedInUrl { get; set; }
    }
}