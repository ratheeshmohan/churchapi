using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Amazon.DynamoDBv2.DocumentModel;
using parishdirectoryapi.Controllers.Models;
using Amazon.DynamoDBv2.Model;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ValidateModel]
    public class ChurchesController : Controller
    {
        ILogger Logger { get; }

        public ChurchesController(ILogger<ChurchesController> logger)
        {
            Logger = logger;
        }

        [HttpGet("{churchId}")]
        public async Task<IActionResult> Get(string churchId)
        {
            Logger.LogInformation($"Getting details of church {churchId}");

            var church = await DynamodbWrapper.DDBContext.LoadAsync<Church>(churchId);

            Logger.LogInformation($"Found church with id {churchId} = {church != null}");

            return church == null ? NotFound() : (IActionResult)Ok(church);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Church church)
        {
            Logger.LogInformation($"Adding a new church with churchId {church.ChurchId}");

            var expr = new Expression
            {
                ExpressionStatement = "attribute_not_exists(ChurchId)"
            };

            var document = DynamodbWrapper.DDBContext.ToDocument(church);
            try
            {
                await DynamodbWrapper.ChurchesTable.PutItemAsync(document, new PutItemOperationConfig() { ConditionalExpression = expr });
                return Created($"/api/churches/{church.ChurchId}", "");
            }
            catch (ConditionalCheckFailedException)
            {
                return BadRequest($"Church with id {church.ChurchId} already exists");
            }
        }


        [HttpPut("{churchId}")]
        public async Task<IActionResult> Put(string churchId, [FromBody]Church church)
        {
            Logger.LogInformation($"Replacing church with churchId {church.ChurchId}");

            var expr = new Expression
            {
                ExpressionStatement = "attribute_exists(ChurchId)"
            };

            var document = DynamodbWrapper.DDBContext.ToDocument(church);
            try
            {
                await DynamodbWrapper.ChurchesTable.PutItemAsync(document,
                    new PutItemOperationConfig() { ConditionalExpression = expr });
                return Ok();
            }
            catch (ConditionalCheckFailedException)
            {
                return BadRequest($"Church with id {church.ChurchId} doesnot exists");
            }
        }
    }
}