using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using parishdirectoryapi.Controllers;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace parishdirectoryapi.Test.Controllers
{
    public class FamiliesControllerTest
    {
        [Fact]
        public async Task TestPost()
        {
            var dataRepository = Substitute.For<Services.IDataRepository>();
            var loginProvider = Substitute.For<Services.ILoginProvider>();
            var logger = Substitute.For<ILogger<FamiliesController>>();

            loginProvider.CreateLogin(Arg.Any<string>(), Arg.Any<LoginMetadata>()).Returns(true);
            dataRepository.AddFamily(Arg.Any<Family>()).Returns(true);
            dataRepository.AddMembers(Arg.Any<IEnumerable<Member>>()).Returns(true);

            var familyVM = GetFamilyViewModel();

            var controller = new FamiliesController(dataRepository, loginProvider, logger);
            await controller.Post(familyVM);

            await dataRepository
               .Received()
               .AddFamily(Arg.Is<Family>(family =>
                     Compare(familyVM, family)));

            await dataRepository
                .Received()
                .AddMembers(Arg.Is<IEnumerable<Member>>(members => TestMembers(familyVM, members)));
        }

        private bool TestMembers(FamilyViewModel familyVM, IEnumerable<Member> members)
        {
            return familyVM.Members.Zip(members, (a, b) =>
             (
                   a.Member.FirstName == b.FirstName &&
                   a.Member.MiddleName == b.MiddleName &&
                   a.Member.LastName == b.LastName &&
                   a.Member.Gender == b.Gender &&
                   a.Member.Phone == b.Phone &&
                   a.Member.DateOfBirth == b.DateOfBirth &&
                   a.Member.DateOfWedding == b.DateOfWedding &&
                   a.Member.FacebookUrl == b.FacebookUrl &&
                   a.Member.LinkedInUrl == b.LinkedInUrl

             )).All(_ => _);
        }

        private bool Compare(FamilyViewModel familyVM, Family family)
        {
            return string.Equals(familyVM.LoginEmail, family.LoginId) &&
             string.Equals(familyVM.FamilyId, family.FamilyId) &&
             ObjectDeepEquals(familyVM.Profile.Address, family.Address) &&
             ObjectDeepEquals(familyVM.Profile.HomeParish, family.HomeParish) &&
             string.Equals(familyVM.Profile.PhotoUrl, family.PhotoUrl);
        }

        private bool ObjectDeepEquals(object a, object b)
        {
            if (a != null && b != null)
            {
                return string.Equals(JsonConvert.SerializeObject(a),
                     JsonConvert.SerializeObject(a));
            }
            if (a == null && b == null)
            {
                return true;
            }
            return false;
        }

        private FamilyViewModel GetFamilyViewModel()
        {
            var payload = new FamilyViewModel()
            {
                FamilyId = "Fam001",
                LoginEmail = "Rat@gmail.com",
                Profile = new FamilyProfileViewModel
                {
                    Address = new Address
                    {
                        Country = "c",
                        Pincode = 1,
                        State = "s",
                        StreetAddress1 = "s1",
                        StreetAddress2 = "s2",
                        Suburb = "Sb"
                    },
                    HomeParish = new Parish
                    {
                        Address = new Address
                        {
                            Country = "c1",
                            Pincode = 12,
                            State = "C1",
                            StreetAddress1 = "x1",
                            StreetAddress2 = "sx2",
                            Suburb = "XCb"
                        },
                        Name = "St"
                    },
                    PhotoUrl = "AAaA.com"
                },
                Members = new[]
                {
                    new FamilyMemberViewModel
                    {
                        Role = FamilyRole.Child,
                        Member = new MemberViewModel
                        {
                            FirstName="F",
                            LastName="L",
                            MiddleName="M",
                            NickName="N",
                            DateOfBirth="19-Nov",
                            DateOfWedding="20-Sept",
                            EmailId = "rat@gmail.com",
                            FacebookUrl = "sdsf.com",
                            Gender = Gender.Female,
                            LinkedInUrl="Asd",
                            MemberId="asdasd",
                            Phone = "Asdcsdsfdsf"
                        }
                    }
                }
                
            };
            return payload;
        }
    }
}
