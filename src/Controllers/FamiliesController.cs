using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
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
    [Route("api/families")]
    [ValidateModel]
    public class FamiliesController : BaseController
    {
        private ILogger _logger { get; }
        private ILoginProvider _loginProvider;

        public FamiliesController(IDataRepository dataRepository, ILoginProvider loginProvider,
            ILogger<FamiliesController> logger) : base(dataRepository)
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
        public async Task<IActionResult> Post([FromBody] FamilyViewModel familyViewModel)
        {
            var churchId = GetUserContext().ChurchId;

            var context = $"ChurchId = {churchId} FamilyId = {familyViewModel.FamilyId} " +
                $"LoginEmail = {familyViewModel.LoginEmail}";
            _logger.LogInformation($"Creating new family using {context}");

            var family = new Family()
            {
                ChurchId = churchId,
                FamilyId = familyViewModel.FamilyId
            };

            if (familyViewModel.Profile != null)
            {
                family.PhotoUrl = familyViewModel.Profile.PhotoUrl;
                family.Address = familyViewModel.Profile.Address;
                family.HomeParish = familyViewModel.Profile.HomeParish;
            }

            IEnumerable<Member> membersDO = null;
            if (familyViewModel.Members != null)
            {
                membersDO = familyViewModel.Members.Select(m =>
               {
                   var member = CreateMember(m.Member);
                   member.ChurchId = churchId;
                   member.FamilyId = familyViewModel.FamilyId;
                   return member;
               });

                family.Members = membersDO.Zip(familyViewModel.Members,
                    (a, b) => new FamilyMember
                    {
                        MemberId = a.MemberId,
                        Role = b.Role
                    }).ToList();
            }

            var createFamilyT = DataRepository.AddFamily(family);
            var createLoginT = _loginProvider.CreateLogin(familyViewModel.LoginEmail,
                new LoginMetadata { FamlyId = familyViewModel.FamilyId });

            await Task.WhenAll(createLoginT, createFamilyT);
            if (createLoginT.Result && createFamilyT.Result)
            {
                if (membersDO != null)
                {
                    await DataRepository.AddMembers(membersDO);
                }
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
        public async Task<IActionResult> UpdateProfile(string familyId,
            [FromBody]FamilyProfileViewModel profile)
        {
            var churchId = GetUserContext().ChurchId;
            var family = new Family()
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
        public async Task<IActionResult> AddMembers(string familyId,
            [FromBody]FamilyMemberViewModel[] familyMembers)
        {
            var churchId = GetUserContext().ChurchId;
            var family = await DataRepository.GetFamily(churchId, familyId);
            if (family == null)
            {
                return BadRequest();
            }

            var members = familyMembers.Select(m =>
            {
                var member = CreateMember(m.Member);
                member.ChurchId = churchId;
                member.FamilyId = familyId;
                return member;
            });

            if (family.Members == null)
            {
                family.Members = new List<FamilyMember>();
            }
            family.Members.AddRange(
                members.Zip(familyMembers, (a, b) => new FamilyMember { MemberId = a.MemberId, Role = b.Role }));

            var updateTask = DataRepository.UpdateFamily(family);
            var addMemberTask = DataRepository.AddMembers(members);
            await Task.WhenAll(updateTask, addMemberTask);
            return Ok();
        }

        [HttpPost("{familyId}/removemembers")]
        public async Task<IActionResult> RemoveMembers(string familyId, [FromBody]string[] memberIds)
        {
            var churchId = GetUserContext().ChurchId;

            var family = await DataRepository.GetFamily(churchId, familyId);
            if (family == null || family.Members == null)
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
        public async Task<IActionResult> UpdateMembers(string familyId, [FromBody]MemberViewModel[] members)
        {
            var churchId = GetUserContext().ChurchId;
            var family = await DataRepository.GetFamily(churchId, familyId);
            if (family == null || family.Members == null)
            {
                return BadRequest();
            }

            var familyMembers = family.Members.Select(m => m.MemberId).ToList();
            if (!members.All(m => !string.IsNullOrEmpty(m.MemberId) && familyMembers.Contains(m.MemberId)))
            {
                return BadRequest();
            }

            var membersDO = members.Select(m =>
            {
                var member = m.ToMember();
                member.FamilyId = familyId;
                member.ChurchId = churchId;
                return member;
            });

            await Task.WhenAll(membersDO.Select(m => DataRepository.UpdateMember(m)));
            return Ok();
        }

        [HttpGet("{familyId}")]
        public async Task<IActionResult> Get(string familyId)
        {
            var userContext = GetUserContext();
            var churchId = userContext.ChurchId;

            var family = await DataRepository.GetFamily(churchId, familyId);
            if (family == null)
            {
                return NotFound();
            }

            var familyVM = new FamilyViewModel()
            {
                FamilyId = familyId,
                Members = new FamilyMemberViewModel[0],
                Profile = new FamilyProfileViewModel
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
                        return new FamilyMemberViewModel
                        {
                            Role = members2RoleMap[m.MemberId],
                            Member = m.ToMemberViewModel()
                        };
                    });
                }
            }
            return Ok(familyVM);
        }

        private Member CreateMember(MemberViewModel memberViewModel)
        {
            var member = memberViewModel.ToMember();
            member.MemberId = System.Guid.NewGuid().ToString();
            return member;
        }

        private new UserContext GetUserContext() //Temp: use base class
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = null, ChurchId = "smioc", LoginId = "admin@gmail.com" };
        }
    }
}
 