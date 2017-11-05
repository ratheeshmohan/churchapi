using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
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
    public class FamiliesController : Controller
    {
        ILogger Logger { get; }
        private IDataRepository _dataRepository;

        public FamiliesController(IDataRepository dataRepository, ILogger<FamilyController> logger)
        {
            _dataRepository = dataRepository;
            Logger = logger;
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
        public async Task<IActionResult> Post([FromBody]Models.CreateFamilyRequest request)
        {
            var churchId = GetUserContext().ChurchId;
            var family = new Family
            {
                ChurchId = churchId,
                FamilyId = request.FamilyId,
                LoginId = request.LoginEmail
            };

            Logger.LogInformation($"Creating a new family using ChurchId {family.ChurchId} and FamilyId {family.FamilyId}");

            var createUserTask = CreateIAMUser(request.LoginEmail, family.FamilyId);
            var addFamilyTask = _dataRepository.CreateFamily(family.ChurchId, family.FamilyId);

            await Task.WhenAll(createUserTask, addFamilyTask);

            string errorResult = $"Family with Id {family.FamilyId} and LoginEmail {request.LoginEmail} already exists";
            if (createUserTask.Result && addFamilyTask.Result)
            {
                Logger.LogInformation($"Creating family completed for ChurchId {family.ChurchId} and FamilyId {family.FamilyId}");
                return Created($"/api/families/{family.FamilyId}", "");
            }
            else if (createUserTask.Result && !addFamilyTask.Result)
            {
                await DeleteIAMUser(request.LoginEmail, family.FamilyId);

                errorResult = $"FamilyId {family.FamilyId} already exists";
                Logger.LogInformation($"Rolled back added IAM User ChurchId {family.ChurchId} and LoginEmail {request.LoginEmail}");
            }
            else if (!createUserTask.Result && addFamilyTask.Result)
            {
                var isDeleted = await _dataRepository.DeleteFamily(family.ChurchId, family.FamilyId);
                if (!isDeleted)
                {
                    Logger.LogError($"Failed to rollback the created used. Failed to delete family from database.");
                }
                errorResult = $"LoginEmail {request.LoginEmail} already exists";
                Logger.LogInformation($"Rolled back added family ChurchId {family.ChurchId} and FamilyId {family.FamilyId}");
            }

            Logger.LogInformation($"Creating family failed for ChurchId {family.ChurchId} and FamilyId {family.FamilyId}. {errorResult}");
            return BadRequest(errorResult);
        }


        [HttpPost("{familyId}/addmembers")]
        public async Task<IActionResult> Post(string familyId, [FromBody]MemberViewModel[] members)
        {
            var churchId = GetUserContext().ChurchId;
            var family = await _dataRepository.GetFamily(churchId, familyId);
            if (family == null)
            {
                return NotFound();
            }

            var map = new Dictionary<MemberViewModel, Member>();
            foreach (var m in members)
            {
                var member = CreateMember(m);
                member.FamilyId = familyId;
                map[m] = member;
            }

            var result = await _dataRepository.AddMembers(churchId, map.Values.ToArray());
            if (!result)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            if (family.Members == null)
            {
                family.Members = new List<FamilyMember>();
            }
            foreach (var keyval in map)
            {
                family.Members.Add(new FamilyMember { MemberId = keyval.Value.MemberId, Role = keyval.Key.Role });
            }

            //Update Family.
            await _dataRepository.UpdateFamily(family);
            return Ok();
        }

        private Member CreateMember(MemberViewModel memberVM)
        {
            var member = memberVM.ToMember();
            member.MemberId = System.Guid.NewGuid().ToString();
            return member;
        }

        private Task<bool> CreateIAMUser(string email, string familyId)
        {
            //TODO
            return Task.FromResult(true);
        }

        private Task<bool> DeleteIAMUser(string email, string familyId)
        {
            //TODO
            return Task.FromResult(true);
        }

        private UserContext GetUserContext()
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = null, ChurchId = "smioc", LoginId = "admin@gmail.com" };
        }
    }
}