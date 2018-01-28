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
        private readonly IImageService _imageService;

        public FamiliesController(IDataRepository dataRepository, ILoginProvider loginProvider,
        IImageService imageService, ILogger<FamiliesController> logger) : base(dataRepository)
        {
            _loginProvider = loginProvider;
            _imageService = imageService;
            _logger = logger;
        }

        #region Admin Routes

        [HttpPost("families/{familyId}/addLogins")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> AddLogin(string familyId, [FromBody] string[] emailIds)
        {
            if (emailIds == null || emailIds.Length == 0)
            {
                return BadRequest("Empty emailIds");
            }
            var emails = emailIds.Distinct();

            var registrations = await Task.WhenAll(emails.Select(_loginProvider.IsRegistered));
            if (registrations.Any(r => r))
            {
                var registeredEmails = emails.Zip(registrations, (e, r) => Tuple.Create(e, r)).Where(t => t.Item2).Select(t => t.Item1);
                return BadRequest($"Email Id(s) are already registered. {string.Join(",", registeredEmails)}");
            }

            var context = GetUserContext();
            var createdLogins = await Task.WhenAll(emails.Select(email => _loginProvider.CreateLogin(
                                                new User
                                                {
                                                    LoginId = email,
                                                    ChurchId = context.ChurchId,
                                                    FamlyId = familyId,
                                                    Email = email,
                                                    Role = UserRole.User
                                                })));

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);

            var logins = new List<string>();
            if (family.Logins != null)
            {
                logins.AddRange(family.Logins);
            }

            var failedLogins = new List<string>();
            foreach (var tup in emails.Zip(createdLogins, System.Tuple.Create))
            {
                if (tup.Item2)
                {
                    logins.Add(tup.Item1);
                }
                else
                {
                    failedLogins.Add(tup.Item1);
                    _logger.LogError($"Failed to add loginId : {tup.Item2} in the context {context}");
                }
            }

            if (failedLogins.Count > 0)
            {
                return BadRequest($"Failed to create logins.{string.Join(",", failedLogins.ToArray())}");
            }

            family = new Family
            {
                ChurchId = context.ChurchId,
                FamilyId = familyId,
                Logins = logins
            };
            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("families/{familyId}/removeLogins")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> RemoveLogin(string familyId, [FromBody] string[] emailIds)
        {
            if (emailIds == null || emailIds.Length == 0)
            {
                return BadRequest();
            }
            var emails = emailIds.Distinct();

            var context = GetUserContext();

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family == null || !emails.All(family.Logins.Contains))
            {
                return BadRequest();
            }

            var deletions = await Task.WhenAll(emails.Select(_loginProvider.DeleteLogin));


            var logins = family.Logins;
            var failedDeletions = new List<string>();
            foreach (var tup in emails.Zip(deletions, System.Tuple.Create))
            {
                if (tup.Item2)
                {
                    logins.Remove(tup.Item1);
                }
                else
                {
                    failedDeletions.Add(tup.Item1);
                    _logger.LogError($"Failed to delete login {tup.Item1} from Cognito");
                }
            }

            if (failedDeletions.Count > 0)
            {
                return new StatusCodeResult(500);
            }

            family = new Family
            {
                ChurchId = context.ChurchId,
                FamilyId = familyId,
                Logins = logins
            };

            var result = await DataRepository.UpdateFamily(family);
            return result ? Ok() : StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPost("families")]
        [Authorize(Policy = AuthPolicy.ChurchAdministratorPolicy)]
        public async Task<IActionResult> Post([FromBody] FamilyViewModel familyVm)
        {
            var context = GetUserContext();

            var family = ToFamily(familyVm, context.ChurchId);
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
        public async Task<IActionResult> Post(string familyId, [FromBody] FamilyViewModel familyVm)
        {
            var context = GetUserContext();
            familyVm.FamilyId = familyId;

            var family = ToFamily(familyVm, context.ChurchId);
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
            family.PhotoUrl = _imageService.CreateDownloadableLink(family.PhotoUrl);
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
            family.RevealAddress = profile.RevealAddress;
            family.RevealHomeParish = profile.RevealHomeParish;

            if (!string.IsNullOrEmpty(profile.PhotoUrl))
            {
                var origFamily = await DataRepository.GetFamily(context.ChurchId, context.FamilyId);
                if (!string.IsNullOrEmpty(origFamily.PhotoUrl))
                {
                    await _imageService.DeletObject(origFamily.PhotoUrl);
                }
            }

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

        #region All users Routes

        [HttpGet("families/{familyId}/members")]
        [Authorize(Policy = AuthPolicy.AllUserPolicy)]
        public async Task<IActionResult> GetFamilyMembers(string familyId)
        {
            var context = GetUserContext();
            if (context.Role == UserRole.User && familyId != context.FamilyId)
            {
                return new UnauthorizedResult();
            }

            var family = await DataRepository.GetFamily(context.ChurchId, familyId);
            if (family == null)
            {
                return BadRequest();
            }
            if (family.Members == null)
            {
                return Ok(new Member[0]);
            }

            var members = await DataRepository.GetMembers(context.ChurchId, family.Members.Select(m => m.MemberId));
            return Ok(members);
        }
        #endregion

        private Family ToFamily(FamilyViewModel familyVm, string churchId)
        {
            return new Family
            {
                ChurchId = churchId,
                FamilyId = familyVm.FamilyId,
                Address = familyVm.Address,
                PhotoUrl = familyVm.PhotoUrl,
                HomeParish = familyVm.HomeParish,
                Members = familyVm.Members
            };
        }
    }
}
