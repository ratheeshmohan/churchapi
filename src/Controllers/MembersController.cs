using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Services;
using System.Threading.Tasks;
using System.Linq;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Models;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/churches/{churchId}/[controller]")]
    [ValidateModel]
    public class MembersController : BaseController
    {
        public MembersController(IDataRepository dataRepository,
            ILogger<MembersController> logger) : base(dataRepository)
        {
        }

        [HttpGet("{memberId}")]
        public async Task<IActionResult> Get(string memberId)
        {
            var isAuthorised = await IsAuthorised(memberId);
            if (!isAuthorised)
            {
                return new UnauthorizedResult();
            }

            var churchId = GetUserContext().ChurchId;
            var members = await DataRepository.GetMembers(churchId, new[] { memberId });
            var res = members.FirstOrDefault();
            return res == null ? NotFound() : (IActionResult)Ok(res);
        }

        [HttpPost("{memberId}")]
        public async Task<IActionResult> Post(string memberId, [FromBody] MemberUpdateViewModel profile)
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
                DateOfBirth = profile.DateOfBirth,
                DateOfWedding = profile.DateOfWedding,
                FacebookUrl = profile.FacebookUrl,
                LinkedInUrl = profile.LinkedInUrl,
            };

            var res = await DataRepository.UpdateMember(member);
            return res ? Ok() : new StatusCodeResult(500);
        }


        private async Task<bool> IsAuthorised(string memberId)
        {
            var familyId = GetUserContext().FamilyId;
            var churchId = GetUserContext().ChurchId;
            var family = await DataRepository.GetFamily(churchId, familyId);
            return family.Members.Any(x => x.MemberId == memberId);
        }
    }
}