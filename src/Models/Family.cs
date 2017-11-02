using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace parishdirectoryapi.Models
{
    public enum Sex
    {
        Male,
        Female
    }

    public class Parish
    {
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class Family
    {
        public string ChurchId { get; set; }
        [Required]
        public string FamilyId { get; set; }
        [Required]
        public Address Address { get; set; }
        public Parish HomeParish { get; set; }
        public string PhotoUrl { get; set; }
        public MemberProfile Husband { get; set; }
        public MemberProfile Wife { get; set; }

        public MemberProfile[] HusbandParents { get; set; }
        public MemberProfile[] WifeParents { get; set; }

        public MemberProfile[] HusbandGrandParents { get; set; }
        public MemberProfile[] WifeGrandParents { get; set; }

        public MemberProfile[] InLaws { get; set; }
        public MemberProfile[] Childrens { get; set; }
    }

    public class MemberProfile
    {
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