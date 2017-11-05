using parishdirectoryapi.Models;

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
    }

    /*
        public class MappingProfile : Profile 
        {
        public MappingProfile() {
            // Add as many of these lines as you need to map your objects
            CreateMap<Member, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
        public static class ModelMappers
        {
            public static Family ToFamily(this FamilyPofile profile)
            {
                return new Family
                {
                    Address = profile.Address,
                    HomeParish = profile.HomeParish,
                    PhotoUrl = profile.PhotoUrl,
                   /* Husband = profile.Husband,
                    Wife = profile.Wife,
                    HusbandParents = profile.HusbandParents,
                    WifeParents = profile.WifeParents,
                    HusbandGrandParents = profile.HusbandGrandParents,
                    WifeGrandParents = profile.WifeGrandParents,
                    InLaws = profile.InLaws,
                    Childrens = profile.Childrens
                };
            }

            internal static FamilyPofile ToFamilyProfile(this Family family)
            {
                return new FamilyPofile
                {
                    Address = family.Address,
                    HomeParish = family.HomeParish,
                    PhotoUrl = family.PhotoUrl,
                   Husband = family.Husband,
                    Wife = family.Wife,
                    HusbandParents = family.HusbandParents,
                    WifeParents = family.WifeParents,
                    HusbandGrandParents = family.HusbandGrandParents,
                    WifeGrandParents = family.WifeGrandParents,
                    InLaws = family.InLaws,
                    Childrens = family.Childrens
                };
            }
        }*/
}