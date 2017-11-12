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

            var familyVM = GetFamilyViewModel();

            var controller = new FamiliesController(dataRepository, loginProvider, logger);
            await controller.Post(familyVM);

            await dataRepository
               .Received()
               .AddFamily(Arg.Is<Family>(family => Compare(familyVM, family)));

            //await dataRepository
            //    .Received()
            //    .AddMembers(members=> members.Select(m=>m.Me))
        }

        private bool Compare(FamilyViewModel familyVM, Family family)
        {
            return string.Equals(familyVM.LoginEmail, family.LoginId) &&
             string.Equals(familyVM.FamilyId, family.FamilyId) &&
             AddressEquals(familyVM.Profile.Address, family.Address);
        }
         

        private bool AddressEquals(Address a, Address b)
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
                    Address = new Models.Address
                    {
                        Country = "c",
                        Pincode = 1,
                        State = "s",
                        StreetAddress1 = "s1",
                        StreetAddress2 = "s2",
                        Suburb = "Sb"
                    },
                    HomeParish = new Models.Parish
                    {
                        Address = new Models.Address
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
                }
            };
            return payload;
        }
    }
}
