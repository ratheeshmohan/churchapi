using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using parishdirectoryapi.Models;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Security;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Services;
using System.Linq;
using System.Collections.Generic;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/family")]
    [ValidateModel]
    public class FamilyController : BaseController
    {
        public FamilyController(IDataRepository dataRepository) : base(dataRepository) { }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userContext = GetUserContext();
            var churchId = userContext.ChurchId;
            var familyId = userContext.FamilyId;

            var family = await DataRepository.GetFamily(churchId, familyId);
            if (family == null)
            {
                return NotFound();
            }

            var familyVM = new Models.Family()
            {
                FamilyId = familyId,
                Members = new FamilyMemeber[0],
                FamilyProfile = new FamilyProfile
                {
                    Address = family.Address,
                    HomeParish = family.HomeParish,
                    PhotoUrl = family.PhotoUrl,
                }
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
                    var members = await DataRepository.GetMembers(churchId, members2RoleMap.Keys);
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
    }
}
