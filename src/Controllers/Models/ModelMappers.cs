using parishdirectoryapi.Models;
using System.Linq;

namespace parishdirectoryapi.Controllers.Models
{
    public static class ModelMappers
    {
        public static MemberViewModel ToMemberViewModel(this Member member)
        {
            return new MemberViewModel
            {
                MemberId = member.MemberId,
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                NickName = member.NickName,
                Gender = member.Gender,
                Phone = member.Phone,
                EmailId = member.EmailId,
                DateOfBirth = member.DateOfBirth,
                DateOfWedding = member.DateOfWedding,
                FacebookUrl = member.FacebookUrl,
                LinkedInUrl = member.LinkedInUrl
            };
        }

        public static Member ToMember(this MemberViewModel member)
        {
            return new Member
            {
                MemberId = member.MemberId,
                FirstName = member.FirstName,
                MiddleName = member.MiddleName,
                LastName = member.LastName,
                NickName = member.NickName,
                Gender = member.Gender,
                Phone = member.Phone,
                EmailId = member.EmailId,
                DateOfBirth = member.DateOfBirth,
                DateOfWedding = member.DateOfWedding,
                FacebookUrl = member.FacebookUrl,
                LinkedInUrl = member.LinkedInUrl
            };
        }
        public static Family ToFamily(this FamilyViewModel familyViewModel, string churchId)
        {
            var family = new Family
            {
                ChurchId = churchId,
                LoginId = familyViewModel.LoginEmail,
                FamilyId = familyViewModel.FamilyId
            };

            if (familyViewModel.Profile != null)
            {
                family.PhotoUrl = familyViewModel.Profile.PhotoUrl;
                family.Address = familyViewModel.Profile.Address;
                family.HomeParish = familyViewModel.Profile.HomeParish;
            }

            if (familyViewModel.Members != null)
            {
                family.Members = familyViewModel.Members.Select(
                   m => new FamilyMember
                   {
                       MemberId = m.Member.MemberId,
                       Role = m.Role
                   }).ToList();

            }
            return family;
        }
    }
}