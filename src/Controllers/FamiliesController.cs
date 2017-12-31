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

        [HttpPost("families/{familyId}/addLogin")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> AddLogin(string familyId, [FromBody] string emailId)
        {
            if (emailId == null)
            {
                return BadRequest();
            }

            var context = GetUserContext();

            var loginCreated = await _loginProvider.CreateLogin(
                                     new User
                                     {
                                         LoginId = emailId,
                                         ChurchId = context.ChurchId,
                                         FamlyId = familyId,
                                         Email = emailId,
                                         Role = UserRole.User
                                     });
            if (!loginCreated)
            {
                _logger.LogError($"Failed to add new loginId : {emailId} in the context {context}");
                return BadRequest();
            }

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);

            var logins = new List<string>();
            if (family.Logins != null)
            {
                logins.AddRange(family.Logins);
            }
            logins.Add(emailId);

            family = new Family
            {
                ChurchId = context.ChurchId,
                FamilyId = familyId,
                Logins = logins
            };

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("families/{familyId}/removeLogin")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> RemoveLogin(string familyId, [FromBody] string emailId)
        {
            if (emailId == null)
            {
                return BadRequest();
            }

            var context = GetUserContext();

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family == null || !family.Logins.Contains(emailId))
            {
                return BadRequest();
            }

            var deletedLogin = await _loginProvider.DeleteLogin(emailId);
            if (!deletedLogin)
            {
                _logger.LogError($"Failed to delete login {emailId} from Cognito");
                return new StatusCodeResult(500);
            }

            family = new Family
            {
                ChurchId = context.ChurchId,
                FamilyId = familyId,
                Logins = family.Logins.Where(id => id != emailId).ToList()
            };

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("families")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Post([FromBody] Family family)
        {
            var context = GetUserContext();

            family.ChurchId = context.ChurchId;
            var result = await DataRepository.AddFamily(family);
            if (result)
            {
                _logger.LogInformation($"Creating new family Succed using {context}");
                return Created($"/api/families/{family.FamilyId}", "");
            }
            return BadRequest();
        }

        [HttpPost("families/{familyId}")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Put(string familyId,
            [FromBody] Family family)
        {
            var context = GetUserContext();
            family.ChurchId = context.ChurchId;
            family.FamilyId = familyId;

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
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
            return Ok(family);
        }
        #endregion

        #region User Routes

        [HttpPost("family/profile")]
        [Authorize(Policy = AuthPolicy.ChurchMemberPolicy)]
        public async Task<IActionResult> UpdateFamilyProfile([FromBody] FamilyProfileViewModel profile)
        {
            var context = GetUserContext();
            var family = new Family
            {
                FamilyId = context.FamilyId,
                ChurchId = context.ChurchId
            };
            family.Address = profile.Address;
            family.PhotoUrl = profile.PhotoUrl;
            family.HomeParish = profile.HomeParish;

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpGet("family")]
        [Authorize(Policy = AuthPolicy.ChurchMemberPolicy)]
        public Task<IActionResult> GetFamily()
        {
            var context = GetUserContext();
            return Get(context.FamilyId);
        }

        #endregion
    }
}
