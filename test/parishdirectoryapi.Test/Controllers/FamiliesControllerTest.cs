/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FsCheck;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using parishdirectoryapi.Controllers;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Models;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;
using Xunit;

namespace parishdirectoryapi.Test.Controllers
{
    public class FamiliesControllerTest
    {
        private const string ChurchId = "smioc";
        [Fact]
        public async Task TestPost()
        {
            var dataRepository = Substitute.For<IDataRepository>();
            var loginProvider = Substitute.For<ILoginProvider>();
            var logger = Substitute.For<ILogger<FamiliesController>>();

            loginProvider.CreateLogin(Arg.Any<User>()).Returns(true);
            dataRepository.AddFamily(Arg.Any<Family>()).Returns(true);
            dataRepository.AddMembers(Arg.Any<IEnumerable<Member>>()).Returns(true);

            var controller = GetFamiliesController(dataRepository, loginProvider, logger);

            var samples = GetFamilyViewModelArb().Sample(100, 100);
            foreach (var familyViewModel in samples)
            {
                dataRepository.ClearReceivedCalls();

                await controller.Post(familyViewModel);
                try
                {
                    await dataRepository
                        .Received()
                        .AddFamily(Arg.Is<Family>(family =>
                            family.ChurchId == ChurchId &&
                            Compare(familyViewModel, family) &&
                            family.Members.Count == familyViewModel.Members.Count()));
                    if (familyViewModel.Members.Any())
                    {
                        await dataRepository
                            .Received()
                            .AddMembers(Arg.Is<IEnumerable<Member>>(members => TestMembers(familyViewModel, members)));
                    }
                    else
                    {
                        await dataRepository
                            .DidNotReceive()
                            .AddMembers(Arg.Any<IEnumerable<Member>>());
                    }
                }
                catch
                {
                    Console.WriteLine(JsonConvert.SerializeObject(familyViewModel));
                    throw;
                }
            }
        }


        [Fact]
        public async Task TestAddMembers()
        {
            var dataRepository = Substitute.For<IDataRepository>();
            var loginProvider = Substitute.For<ILoginProvider>();
            var logger = Substitute.For<ILogger<FamiliesController>>();
            var controller = GetFamiliesController(dataRepository, loginProvider, logger);

            var sample = GetFamilyMemberViewModelArb().ArrayOf(2).Sample(5, 5);
            foreach (var members in sample)
            {
                const string familyId = "1";
                var familyOrig = new Family { FamilyId = familyId, ChurchId = ChurchId, LoginId = "aa" };

                dataRepository.GetFamily(Arg.Any<string>(), Arg.Any<string>())
                    .Returns(familyOrig);


                dataRepository.AddMembers(Arg.Any<IEnumerable<Member>>()).Returns(true);

                dataRepository.ClearReceivedCalls();

                await controller.AddMembers(familyId, members);
                if (members.Length > 0)
                {
                    await dataRepository
                        .Received()
                        .UpdateFamily(Arg.Is<Family>(family =>
                            members.Length == family.Members.Count));


                    await dataRepository
                        .Received()
                        .AddMembers(Arg.Is<IEnumerable<Member>>(m =>
                            members.Length == m.Count()));
                }
                else
                {
                    await dataRepository
                        .DidNotReceive()
                        .AddFamily(Arg.Any<Family>());

                    await dataRepository
                        .DidNotReceive()
                        .AddMembers(Arg.Any<IEnumerable<Member>>());
                }
            }

        }

        private static FamiliesController GetFamiliesController(IDataRepository dataRepository, ILoginProvider loginProvider,
            ILogger<FamiliesController> logger)
        {
            var controller = new FamiliesController(dataRepository, loginProvider, logger);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim( AuthPolicy.UserName, "admin@g.com"),
                new Claim(AuthPolicy.ChurchIdClaimName, ChurchId)
            }));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext {User = user}
            };
            return controller;
        }

        private static bool TestMembers(FamilyViewModel familyViewModel, IEnumerable<Member> members)
        {
            return familyViewModel.Members.Zip(members, (a, b) =>
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

        private static bool Compare(FamilyViewModel familyViewModel, Family family)
        {
            return string.Equals(familyViewModel.LoginEmail, family.LoginId) &&
             string.Equals(familyViewModel.FamilyId, family.FamilyId) &&
             ObjectDeepEquals(familyViewModel.Profile.Address, family.Address) &&
             ObjectDeepEquals(familyViewModel.Profile.HomeParish, family.HomeParish) &&
             string.Equals(familyViewModel.Profile.PhotoUrl, family.PhotoUrl);
        }

        private static bool ObjectDeepEquals(object a, object b)
        {
            if (a != null && b != null)
            {
                return string.Equals(JsonConvert.SerializeObject(a),
                     JsonConvert.SerializeObject(a));
            }
            return a == null && b == null;
        }

        private static Gen<Address> GetAddressArb()
        {
            return from country in Arb.Generate<string>()
                   from pincode in Arb.Generate<int>()
                   from state in Arb.Generate<string>()
                   from streetAddress1 in Arb.Generate<string>()
                   from streetAddress2 in Arb.Generate<string>()
                   from suburb in Arb.Generate<string>()
                   select new Address
                   {
                       Country = country,
                       Pincode = pincode,
                       State = state,
                       StreetAddress1 = streetAddress1,
                       StreetAddress2 = streetAddress2,
                       Suburb = suburb
                   };
        }

        private static Gen<Parish> GetParishArb()
        {
            return from name in Arb.Generate<string>()
                   from address in GetAddressArb()
                   select new Parish
                   {
                       Name = name,
                       Address = address
                   };
        }

        private static Gen<FamilyProfileViewModel> GetFamilyProfileViewModelArb()
        {
            return from photoUrl in Arb.Generate<string>()
                   from parish in GetParishArb()
                   from address in GetAddressArb()
                   select new FamilyProfileViewModel
                   {
                       PhotoUrl = photoUrl,
                       HomeParish = parish,
                       Address = address
                   };
        }

        private static Gen<FamilyMemberViewModel> GetFamilyMemberViewModelArb()
        {
            return from firstName in Arb.Generate<string>()
                   from lastName in Arb.Generate<string>()
                   from middleName in Arb.Generate<string>()
                   from nickName in Arb.Generate<string>()
                   from dateOfBirth in Arb.Generate<string>()
                   from dateOfWedding in Arb.Generate<string>()
                   from emailId in Arb.Generate<string>()
                   from facebookUrl in Arb.Generate<string>()
                   from linkedInUrl in Arb.Generate<string>()
                   from memberId in Arb.Generate<string>()
                   from phone in Arb.Generate<string>()
                   from gender in Arb.Generate<Gender>()
                   from role in Arb.Generate<FamilyRole>()
                   select new FamilyMemberViewModel
                   {
                       Role = role,
                       Member = new MemberViewModel
                       {
                           FirstName = firstName,
                           LastName = lastName,
                           MiddleName = middleName,
                           NickName = nickName,
                           DateOfBirth = dateOfBirth,
                           DateOfWedding = dateOfWedding,
                           EmailId = emailId,
                           FacebookUrl = facebookUrl,
                           Gender = gender,
                           LinkedInUrl = linkedInUrl,
                           MemberId = memberId,
                           Phone = phone
                       }
                   };
        }

        private static Gen<FamilyViewModel> GetFamilyViewModelArb()
        {
            return from familyId in Arb.Generate<string>()
                   from loginEmail in Arb.Generate<string>()
                   from familyProfile in GetFamilyProfileViewModelArb()
                   from memberCount in Arb.Generate<int>()
                   from members in GetFamilyMemberViewModelArb().ArrayOf(memberCount)
                   select new FamilyViewModel
                   {
                       FamilyId = familyId,
                       LoginEmail = loginEmail,
                       Profile = familyProfile,
                       Members = members
                   };
        }
    }
}
*/