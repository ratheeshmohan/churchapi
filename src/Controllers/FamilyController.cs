using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Security;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Services;
using parishdirectoryapi.Extenstions;
using System.Linq;
using System.Collections.Generic;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/family")]
    [ValidateModel]
    public class FamilyController : Controller
    {
        private IDataRepository _dataRepository;

        public FamilyController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userContext = GetUserContext();
            var churchId = userContext.ChurchId;
            var familyId = userContext.FamilyId;

            var family = await _dataRepository.GetFamily(churchId, familyId);
            if (family == null)
            {
                return NotFound();
            }

            var familyVM = new FamilyViewModel();
            familyVM.Members = new MemberViewModel[0];
            familyVM.Profile = new FamilyProfile
            {
                Address = family.Address,
                HomeParish = family.HomeParish,
                PhotoUrl = family.PhotoUrl,
            };

            if (family.Members != null)
            {
                var members2RoleMap = new Dictionary<string, FamilyRole>();
                foreach (var m in family.Members)
                {
                    members2RoleMap[m.MemberId] = m.Role;
                }
                if (members2RoleMap.Keys.Any())
                {
                    var members = await _dataRepository.GetMembers(churchId, members2RoleMap.Keys);
                    familyVM.Members = members.Select(m =>
                    {
                        var vm = m.ToMemberViewModel();
                        vm.Role = members2RoleMap[vm.MemberId];
                        return vm;
                    });
                }
            }
            return Ok(familyVM);
        }

        private UserContext GetUserContext()
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = "fam0001", ChurchId = "smioc", LoginId = "ratheeshmohan@gmail.com" };
        }
    }
}
