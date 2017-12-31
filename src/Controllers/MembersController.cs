using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Services;
using System.Threading.Tasks;
using System.Linq;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Models;
using Microsoft.AspNetCore.Authorization;
using parishdirectoryapi.Security;
using System;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ValidateModel]
    public class MembersController : BaseController
    {
        private readonly ILogger<MembersController> _logger;

        public MembersController(IDataRepository dataRepository,
            ILogger<MembersController> logger) : base(dataRepository)
        {
            _logger = logger;
        }

        #region Admin Routes

        [HttpPost]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Post([FromBody]Member[] members)
        {
            foreach (var m in members)
            {
                m.MemberId = GetUniqueMemberId();
                m.ChurchId = GetUserContext().ChurchId;
            }

            var result = await DataRepository.AddMembers(members);
            if (result)
            {
                return Ok(members.Select(m => m.MemberId).ToArray());
            }
            else
            {
                _logger.LogError($"Failed to create new memebers");
                return new StatusCodeResult(500);
            }
        }

        [HttpPut("{memberId}")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Put(string memberId, [FromBody] Member member)
        {
            member.MemberId = memberId;
            member.ChurchId = GetUserContext().ChurchId;

            var res = await DataRepository.UpdateMember(member);
            return res ? Ok() : new StatusCodeResult(500);
        }

        #endregion

        [HttpGet("{memberId}")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        [Authorize(Policy = AuthPolicy.ChurchMemberPolicy)]
        public async Task<IActionResult> Get(string memberId)
        {
            var context = GetUserContext();
            if (context.Role == UserRole.User)
            {
                var isAuthorised = await IsAuthorised(memberId);
                if (!isAuthorised)
                {
                    return new UnauthorizedResult();
                }
            }

            var churchId = context.ChurchId;
            var members = await DataRepository.GetMembers(churchId, new[] { memberId });
            var res = members.FirstOrDefault();
            return res == null ? NotFound() : (IActionResult)Ok(res);
        }

        [HttpPost("{memberId}/profile")]
        [Authorize(Policy = AuthPolicy.ChurchMemberPolicy)]
        public async Task<IActionResult> UpdateProfile(string memberId, [FromBody] MemberProfile profile)
        {
            var isAuthorised = await IsAuthorised(memberId);
            if (!isAuthorised)
            {
                return new UnauthorizedResult();
            }

            var churchId = GetUserContext().ChurchId;
            var member = new Member
            {
                ChurchId = churchId,
                MemberId = memberId,
                NickName = profile.NickName,
                Phone = profile.Phone,
                EmailId = profile.EmailId,
                FacebookUrl = profile.FacebookUrl,
                LinkedInUrl = profile.LinkedInUrl,
            };

            var res = await DataRepository.UpdateMember(member);
            return res ? Ok() : new StatusCodeResult(500);
        }

        private async Task<bool> IsAuthorised(string memberId)
        {
            var context = GetUserContext();

            var familyId = context.FamilyId;
            var churchId = context.ChurchId;
            var family = await DataRepository.GetFamily(churchId, familyId);
            return family.Members.Any(x => x.MemberId == memberId);
        }

        private static string GetUniqueMemberId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}