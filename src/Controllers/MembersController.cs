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
        private ILogger<MembersController> Logger { get; }

        public MembersController(IDataRepository dataRepository,
            ILogger<MembersController> logger) : base(dataRepository)
        {
            Logger = logger;
        }

        [HttpGet("{memberId}")]
        public async Task<IActionResult> Get(string memberId)
        {
            var churchId = GetUserContext().ChurchId;
            var members = await DataRepository.GetMembers(churchId, new[] { memberId });
            var res = members.FirstOrDefault();
            return res == null ? NotFound() : (IActionResult)Ok(res);
        }

        [HttpPost("{memberId}")]
        public async Task<IActionResult> Post(string memberId, [FromBody] MemberProfileViewModel profile)
        {
            var churchId = GetUserContext().ChurchId;
            var member = new Member
            {
                ChurchId = churchId,
                MemberId = profile.MemberId,
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
    }
}