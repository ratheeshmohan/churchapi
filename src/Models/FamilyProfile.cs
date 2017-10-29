using System.Collections.Generic;

namespace parishdirectoryapi.Controllers.Models
{

    public enum Sex
    {
        Male,
        Female
    }

    public class MemberProfile
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string DMOB { get; set; }
        public Sex Gender { get; set; }
        public string LinkedInUrl { get; set; }
        public string FacebookUrl { get; set; }
        public string EmailId { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class FamilyProfile
    {
        public string ChurchId { get; set; }
        public string FamilyId { get; set; }
        public List<string> LoginIds{get;set;}
        public string Address { get; set; }
        public string HomeParish { get; set; }
        public string PhotoUrl { get; set; }
        public MemberProfile PrimaryMember { get; set; }
        public MemberProfile Wife { get; set; }
        public MemberProfile HFather { get; set; }
        public MemberProfile HMother { get; set; }
        public MemberProfile WFather { get; set; }
        public MemberProfile WMother { get; set; }
        public MemberProfile[] Childrens { get; set; }
    }
}