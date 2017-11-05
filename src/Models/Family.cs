using System;
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

    public class FamilyMember
    {
        public string MemberId { get; set; }
        public FamilyRole Role { get; set; }
    }

    public class Family
    {
        public string ChurchId { get; set; }
        public string FamilyId { get; set; }
        public string LoginId { get; set; }


        public Address Address { get; set; }
        public Parish HomeParish { get; set; }
        public string PhotoUrl { get; set; }

        public List<FamilyMember> Members { get; set; }
    }
}