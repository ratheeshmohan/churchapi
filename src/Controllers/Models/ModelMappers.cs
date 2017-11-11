using parishdirectoryapi.Models;

namespace parishdirectoryapi.Controllers.Models
{
    public static class ModelMappers
    {
        public static FamilyMemeber ToMemberViewModel(this Member member)
        {
            return new FamilyMemeber
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

        public static Member ToMember(this FamilyMemeber member)
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
    }
}