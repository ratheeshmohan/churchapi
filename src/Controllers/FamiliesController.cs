using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using parishdirectoryapi.Controllers.Models;
using Amazon.DynamoDBv2.Model;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/churches/{churchId}/families")]
    [ValidateModel]
    public class FamiliesController : Controller
    {
        ILogger Logger { get; }

        public FamiliesController(ILogger<FamiliesController> logger)
        {
            Logger = logger;
        }

        [HttpGet("{familyId}")]
        public async Task<IActionResult> Get(string churchId, string familyId)
        {
            Logger.LogInformation($"Getting details of family using  churchId {churchId} and familyId {familyId} ");

            var family = await DynamodbWrapper.DDBContext.LoadAsync<Family>(churchId, familyId);

            Logger.LogInformation($"Found family using  churchId {churchId} and familyId {familyId} : {family != null}");
            return family == null ? NotFound() : (IActionResult)Ok(family);
        }

        [HttpPost]
        public async Task<IActionResult> Post(string churchId, [FromBody]Family family)
        {
            family.ChurchId = churchId;

            //TODO:
            //1. this.Request.Headers["Authorization"]
            //Add Microsoft.AspNetCore.Authentication.JwtBearer middleware and check if this call is made by {family.ChurchId}'s church administrator

            Logger.LogInformation($"Creating a new family for ChurchId {family.ChurchId} and FamilyId {family.FamilyId}");

            var createUserTask = CreateIAMUser(family.LoginEmail, family.FamilyId);
            var addFamilyTask = AddFamily(family);

            await Task.WhenAll(createUserTask, addFamilyTask);

            string errorResult = $"Family with Id {family.FamilyId} and LoginEmail {family.LoginEmail} already exists";
            if (createUserTask.Result && addFamilyTask.Result)
            {
                Logger.LogInformation($"Creating family completed for ChurchId {family.ChurchId} and FamilyId {family.FamilyId}");
                return Created($"/api/families/{family.FamilyId}", "");
            }
            else if (createUserTask.Result && !addFamilyTask.Result)
            {
                await DeleteIAMUser(family.LoginEmail, family.FamilyId);

                errorResult = $"FamilyId {family.FamilyId} already exists";
                Logger.LogInformation($"Rolled back added IAM User ChurchId {family.ChurchId} and LoginEmail {family.LoginEmail}");
            }
            else if (!createUserTask.Result && addFamilyTask.Result)
            {
                await RemoveFamily(family);

                errorResult = $"LoginEmail {family.LoginEmail} already exists";
                Logger.LogInformation($"Rolled back added family ChurchId {family.ChurchId} and FamilyId {family.FamilyId}");
            }

            Logger.LogInformation($"Creating family failed for ChurchId {family.ChurchId} and FamilyId {family.FamilyId}. {errorResult}");
            return BadRequest(errorResult);
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

        private async Task<bool> AddFamily(Family family)
        {
            var expr = new Expression
            {
                ExpressionStatement = "attribute_not_exists(FamilyId)"
            };
            var document = DynamodbWrapper.DDBContext.ToDocument(family);
            try
            {
                await DynamodbWrapper.FamiliesTable.PutItemAsync(document, new PutItemOperationConfig() { ConditionalExpression = expr });
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                return false;
            }
        }

        private Task RemoveFamily(Family family)
        {
            var familyDoc = DynamodbWrapper.DDBContext.ToDocument(family);
            return DynamodbWrapper.FamiliesTable.DeleteItemAsync(familyDoc);
        }
    }
}