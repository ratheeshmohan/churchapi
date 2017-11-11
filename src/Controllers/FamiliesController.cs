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

        public FamiliesController(IDataRepository dataRepository, ILoginProvider loginProvider, ILogger<FamilyController> logger) : base(dataRepository)
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
        public async Task<IActionResult> Post([FromBody] Models.Family family)
        {
            var churchId = GetUserContext().ChurchId;

            var context = $"ChurchId = {churchId} FamilyId = {family.FamilyId} LoginEmail = {family.LoginEmail}";
            _logger.LogInformation($"Creating new family using {context}");

            var familyDO = new parishdirectoryapi.Models.Family()
            {
                ChurchId = churchId,
                FamilyId = family.FamilyId
            };

            if (family.FamilyProfile != null)
            {
                familyDO.PhotoUrl = family.FamilyProfile.PhotoUrl;
                familyDO.Address = family.FamilyProfile.Address;
                familyDO.HomeParish = family.FamilyProfile.HomeParish;
            }

            IEnumerable<Member> membersDO = null;
            if (family.Members != null)
            {
                membersDO = family.Members.Select(m =>
               {
                   var member = CreateMember(m);
                   member.ChurchId = churchId;
                   member.FamilyId = family.FamilyId;
                   return member;
               });

                familyDO.Members = membersDO.Zip(family.Members, (a, b) => new FamilyMember { MemberId = a.MemberId, Role = b.Role }).ToList();
            }

            var createFamilyT = DataRepository.AddFamily(familyDO);
            var createLoginT = _loginProvider.CreateLogin(family.LoginEmail, new LoginMetadata { FamlyId = family.FamilyId });

            await Task.WhenAll(createLoginT, createFamilyT);

            if (createLoginT.Result && createFamilyT.Result)
            {
                if (membersDO != null)
                {
                    await DataRepository.AddMembers(membersDO);
                }
                _logger.LogInformation($"Creating new family Succed using {context}");
                return Created($"/api/families/{family.FamilyId}", "");
            }

            //Cleanups
            _logger.LogError($"Failed to create new family using {context}");
            if (createLoginT.Result)
            {
                var hasRollbacked = await _loginProvider.DeleteLogin(family.LoginEmail);
                if (!hasRollbacked)
                {
                    _logger.LogError($"Failed to rollback  created login");
                }
            }

            if (createFamilyT.Result)
            {
                var isDeleted = await DataRepository.DeleteFamily(churchId, family.FamilyId);
                if (!isDeleted)
                {
                    _logger.LogError($"Failed to rollback  add family from database");
                }
            }
            return BadRequest();
        }

        [HttpPost("{familyId}/updateprofile")]
        public async Task<IActionResult> UpdateProfile(string familyId, [FromBody]FamilyProfile profile)
        {
            var churchId = GetUserContext().ChurchId;

            var family = new parishdirectoryapi.Models.Family()
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
        public async Task<IActionResult> AddMembers(string familyId, [FromBody]FamilyMemeber[] members)
        {
            var churchId = GetUserContext().ChurchId;
            var familyDO = await DataRepository.GetFamily(churchId, familyId);
            if (familyDO == null)
            {
                return BadRequest();
            }

            var membersDO = members.Select(m =>
            {
                var member = CreateMember(m);
                member.ChurchId = churchId;
                member.FamilyId = familyId;
                return member;
            });

            if (familyDO.Members == null)
            {
                familyDO.Members = new List<FamilyMember>();
            }
            familyDO.Members.AddRange(membersDO.Zip(members, (a, b) => new FamilyMember { MemberId = a.MemberId, Role = b.Role }));

            var updateTask = DataRepository.UpdateFamily(familyDO);
            var addMemberTask = DataRepository.AddMembers(membersDO);
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
        public async Task<IActionResult> UpdateMembers(string familyId, [FromBody]FamilyMemeber[] members)
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
        
        private Member CreateMember(FamilyMemeber memberVM)
        {
            var member = memberVM.ToMember();
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
 