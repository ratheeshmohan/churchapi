using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Models;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;
// ReSharper disable PossibleMultipleEnumeration

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api")]
    [ValidateModel]
    [Authorize]
    public class FamiliesController : BaseController
    {
        private readonly ILogger<FamiliesController> _logger;
        private readonly ILoginProvider _loginProvider;

        public FamiliesController(IDataRepository dataRepository, ILoginProvider loginProvider,
            ILogger<FamiliesController> logger) : base(dataRepository)
        {
            _loginProvider = loginProvider;
            _logger = logger;
        }

        #region Admin Routes

        [HttpPost("families")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Post([FromBody] FamilyViewModel familyViewModel)
        {
            var context = GetUserContext();
            _logger.LogInformation($"Creating new family using {context}");

            if (familyViewModel.Members != null)
            {
                foreach (var m in familyViewModel.Members)
                {
                    m.Member.MemberId = GetUniqueMemberId();
                }
            }

            var family = familyViewModel.ToFamily(context.ChurchId);
            var createFamilyT = DataRepository.AddFamily(family);

            var createLoginT = _loginProvider.CreateLogin(
                new User
                {
                    LoginId = familyViewModel.LoginEmail,
                    ChurchId = context.ChurchId,
                    FamlyId = familyViewModel.FamilyId,
                    Email = familyViewModel.LoginEmail,
                    Role = UserRole.User
                });

            await Task.WhenAll(createLoginT, createFamilyT);

            if (createLoginT.Result && createFamilyT.Result)
            {
                await InsertMembers(context.ChurchId, familyViewModel.FamilyId, familyViewModel.Members);

                _logger.LogInformation($"Creating new family Succed using {context}");
                return Created($"/api/families/{familyViewModel.FamilyId}", "");
            }

            //Cleanups
            _logger.LogError($"Failed to create new family using {context}");
            if (createLoginT.Result)
            {
                var hasRollbacked = await _loginProvider.DeleteLogin(familyViewModel.LoginEmail);
                if (!hasRollbacked)
                {
                    _logger.LogError($"Failed to rollback created login {familyViewModel.LoginEmail} in Cognito");
                }
            }

            if (createFamilyT.Result)
            {
                var isDeleted = await DataRepository.DeleteFamily(context.ChurchId, familyViewModel.FamilyId);
                if (!isDeleted)
                {
                    _logger.LogError($"Failed to rollback addded family '{familyViewModel.FamilyId}' from database");
                }
            }
            return BadRequest();
        }

        [HttpPost("families/{familyId}/updateprofile")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> UpdateProfile(string familyId,
            [FromBody] FamilyProfileViewModel profile)
        {
            var context = GetUserContext();

            var family = new Family
            {
                ChurchId = context.ChurchId,
                FamilyId = familyId,
                PhotoUrl = profile.PhotoUrl,
                Address = profile.Address,
                HomeParish = profile.HomeParish
            };

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("families/{familyId}/addmembers")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> AddMembers(string familyId,
            [FromBody] FamilyMemberViewModel[] familyMembers)
        {
            if (familyMembers == null || familyMembers.Length == 0)
            {
                return BadRequest();
            }

            var context = GetUserContext();

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family == null)
            {
                return BadRequest();
            }

            foreach (var m in familyMembers)
            {
                m.Member.MemberId = GetUniqueMemberId();
            }

            if (family.Members == null)
            {
                family.Members = new List<FamilyMember>();
            }
            family.Members.AddRange(
                familyMembers.Select(m => new FamilyMember
                {
                    MemberId = m.Member.MemberId,
                    Role = m.Role
                }));

            var updateTask = DataRepository.UpdateFamily(family);
            var addMemberTask = InsertMembers(context.ChurchId, familyId, familyMembers);
            await Task.WhenAll(updateTask, addMemberTask);
            return Ok();
        }

        [HttpPost("families/{familyId}/removemembers")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> RemoveMembers(string familyId, [FromBody]string[] memberIds)
        {
            if (memberIds == null || memberIds.Length == 0)
            {
                return BadRequest();
            }

            var context = GetUserContext();

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family?.Members == null)
            {
                return BadRequest();
            }

            var keep = new List<FamilyMember>();
            var remove = new List<string>();
            foreach (var member in family.Members)
            {
                if (memberIds.Contains(member.MemberId))
                {
                    remove.Add(member.MemberId);
                }
                else
                {
                    keep.Add(member);
                }
            }

            if (!memberIds.SequenceEqual(remove))
            {
                return BadRequest();
            }

            //Update Family.
            family.Members = keep;
            var updateTask = DataRepository.UpdateFamily(family);
            var removeTask = DataRepository.RemoveMember(context.ChurchId, remove);

            await Task.WhenAll(updateTask, removeTask);
            return Ok();
        }

        [HttpPost("families/{familyId}/updatemembers")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> UpdateMembers(string familyId,
            [FromBody] MemberViewModel[] memberViewModels)
        {
            var context = GetUserContext();

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family?.Members == null)
            {
                return BadRequest();
            }

            var familyMembers = family.Members.Select(m => m.MemberId).ToList();
            if (!memberViewModels.All(m => !string.IsNullOrEmpty(m.MemberId) && familyMembers.Contains(m.MemberId)))
            {
                return BadRequest();
            }

            var members = memberViewModels.Select(m =>
            {
                var member = m.ToMember();
                member.FamilyId = familyId;
                member.ChurchId = context.ChurchId;
                return member;
            });

            await Task.WhenAll(members.Select(m => DataRepository.UpdateMember(m)));
            return Ok();
        }

        [HttpGet("families/{familyId}")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Get(string familyId)
        {
            var context = GetUserContext();

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family == null)
            {
                return NotFound();
            }

            var familyViewModel = new FamilyViewModel
            {
                FamilyId = familyId,
                Members = new FamilyMemberViewModel[0],
                Profile = new FamilyProfileViewModel
                {
                    Address = family.Address,
                    HomeParish = family.HomeParish,
                    PhotoUrl = family.PhotoUrl
                }
            };

            if (family.Members == null) return Ok(familyViewModel);

            var members2RoleMap = new Dictionary<string, FamilyRole>();
            foreach (var m in family.Members)
            {
                members2RoleMap[m.MemberId] = m.Role;
            }
            if (!members2RoleMap.Keys.Any()) return Ok(familyViewModel);

            var members = await DataRepository.GetMembers(context.ChurchId, members2RoleMap.Keys);
            familyViewModel.Members = members.Select(m => new FamilyMemberViewModel
            {
                Role = members2RoleMap[m.MemberId],
                Member = m.ToMemberViewModel()
            });
            return Ok(familyViewModel);
        }

        #endregion

        #region User Routes

        [HttpPost("family/updateprofile")]
        public Task<IActionResult> UpdateFamilyProfile([FromBody] FamilyProfileViewModel profile)
        {
            var context = GetUserContext();
            return UpdateProfile(context.FamilyId, profile);
        }

        [HttpPost("family/updatemembers")]
        public Task<IActionResult> UpdateFamilyMembers([FromBody] MemberViewModel[] memberViewModels)
        {
            var context = GetUserContext();
            return UpdateMembers(context.FamilyId, memberViewModels);
        }

        [HttpGet("family")]
        public Task<IActionResult> GetFamily()
        {
            var context = GetUserContext();
            return Get(context.FamilyId);
        }

        #endregion

        private async Task<bool> InsertMembers(string churchId, string familyId,
            IEnumerable<FamilyMemberViewModel> familyMembers)
        {
            if (!familyMembers.Any())
            {
                return true;
            }
            var members = familyMembers.Select(m =>
            {
                var member = m.Member.ToMember();
                member.ChurchId = churchId;
                member.FamilyId = familyId;
                return member;
            });

            return await DataRepository.AddMembers(members);
        }

        private static string GetUniqueMemberId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
