using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
    [Route("api/churches/{churchId}/[controller]")]
    [ValidateModel]
    public class FamiliesController : BaseController
    {
        private readonly ILogger _logger;
        private readonly ILoginProvider _loginProvider;

        public FamiliesController(IDataRepository dataRepository, ILoginProvider loginProvider,
            ILogger logger) : base(dataRepository)
        {
            _loginProvider = loginProvider;
            _logger = logger;
        }

        //TODO:
        //1. this.Request.Headers["Authorization"]
        //Add Microsoft.AspNetCore.Authentication.JwtBearer middleware and check if this call is made by {family.ChurchId}'s church administrator
        /* 
             [HttpGet]
             //[Admin Only ROLE]
             public async Task<IActionResult> Get()
             {
                 var user = GetUserContext();

             }

                   [HttpGet("/{familyId}")]
                   //[Admin Only ROLE]
                   public async Task<IActionResult> Get(string familyId)
                   {

                   }

                           [HttpPut("/{familyId}")]
                           //[Admin Only ROLE]
                           public async Task<IActionResult> Put(FamilyPofile profile)
                           {

                           }
                   */
        [HttpPost]
        //[Admin Only ROLE]
        public async Task<IActionResult> Post(string churchId, [FromBody] FamilyViewModel familyViewModel)
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

            var family = familyViewModel.ToFamily(churchId);
            var createFamilyT = DataRepository.AddFamily(family);

            var createLoginT = _loginProvider.CreateLogin(
                new User
                {
                    LoginId = familyViewModel.LoginEmail,
                    ChurchId =churchId,
                    FamlyId = familyViewModel.FamilyId,
                    Email = familyViewModel.LoginEmail
                });

            await Task.WhenAll(createLoginT, createFamilyT);

            if (createLoginT.Result && createFamilyT.Result)
            {
                await InsertMembers(churchId, familyViewModel.FamilyId, familyViewModel.Members);

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
                    _logger.LogError($"Failed to rollback  created login");
                }
            }

            if (createFamilyT.Result)
            {
                var isDeleted = await DataRepository.DeleteFamily(churchId, familyViewModel.FamilyId);
                if (!isDeleted)
                {
                    _logger.LogError($"Failed to rollback  add family from database");
                }
            }
            return BadRequest();
        }

        [HttpPost("{familyId}/updateprofile")]
        public async Task<IActionResult> UpdateProfile(string churchId, string familyId,
            [FromBody] FamilyProfileViewModel profile)
        {
            var family = new Family
            {
                ChurchId = churchId,
                FamilyId = familyId,
                PhotoUrl = profile.PhotoUrl,
                Address = profile.Address,
                HomeParish = profile.HomeParish
            };

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("{familyId}/addmembers")]
        public async Task<IActionResult> AddMembers(string churchId, string familyId,
            [FromBody] FamilyMemberViewModel[] familyMembers)
        {
            var family = await DataRepository.GetFamily(churchId, familyId);
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
            var addMemberTask = InsertMembers(churchId, familyId, familyMembers);
            await Task.WhenAll(updateTask, addMemberTask);
            return Ok();
        }

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

        [HttpPost("{familyId}/removemembers")]
        public async Task<IActionResult> RemoveMembers(string churchId, string familyId, [FromBody] string[] memberIds)
        {
            var family = await DataRepository.GetFamily(churchId, familyId);
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
            var removeTask = DataRepository.RemoveMember(churchId, remove);

            await Task.WhenAll(updateTask, removeTask);
            return Ok();
        }

        [HttpPost("{familyId}/updatemembers")]
        public async Task<IActionResult> UpdateMembers(string churchId, string familyId,
            [FromBody] MemberViewModel[] memberViewModels)
        {
            var family = await DataRepository.GetFamily(churchId, familyId);
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
                member.ChurchId = churchId;
                return member;
            });

            await Task.WhenAll(members.Select(m => DataRepository.UpdateMember(m)));
            return Ok();
        }

        [HttpGet("{familyId}")]
        public async Task<IActionResult> Get(string churchId, string familyId)
        {
            var family = await DataRepository.GetFamily(churchId, familyId);
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

            var members = await DataRepository.GetMembers(churchId, members2RoleMap.Keys);
            familyViewModel.Members = members.Select(m => new FamilyMemberViewModel
            {
                Role = members2RoleMap[m.MemberId],
                Member = m.ToMemberViewModel()
            });
            return Ok(familyViewModel);
        }

        private static string GetUniqueMemberId()
        {
            return Guid.NewGuid().ToString();
        }

        private new static UserContext GetUserContext() //Temp: use base class
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = null, ChurchId = "smioc", LoginId = "admin@gmail.com" };
        }
    }
}
