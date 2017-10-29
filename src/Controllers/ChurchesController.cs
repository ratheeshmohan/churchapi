using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Amazon.Lambda.Core;

using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Net;
using Amazon.DynamoDBv2.DocumentModel;
using parishdirectoryapi.Controllers.Models;
using Newtonsoft.Json.Serialization;
using Amazon.DynamoDBv2.Model;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    public class ChurchesController : Controller
    {
        IDynamoDBContext DDBContext { get; set; }
        Table ChurchTable { get; set; }

        ILogger Logger { get; }

        public ChurchesController(ILogger<S3ProxyController> logger)
        {
            Logger = logger;

            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Church)] = new Amazon.Util.TypeMapping(typeof(Church), "Church");
            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            this.DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);

            ChurchTable = Table.LoadTable(new AmazonDynamoDBClient(), "Church");
        }

        [HttpGet("{churchId}")]
        public async Task Get(string churchId)
        {
            Logger.LogInformation($"Getting details of church {churchId}");

            var church = await DDBContext.LoadAsync<Church>(churchId);

            Logger.LogInformation($"Found church with id {churchId} = {church != null}");
            if (church == null)
            {
                this.Response.StatusCode = (int)HttpStatusCode.NotFound;
            }
            else
            {
                await SetBodyContent(church);
                this.Response.StatusCode = (int)HttpStatusCode.OK;
                this.Response.ContentType = "application/json";
            }
        }

        [HttpPost]
        public async Task Post([FromBody]Church church)
        {
            if (!ModelState.IsValid)
            {
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            Logger.LogInformation($"Adding a new church with churchId {church.ChurchId}");

            var churchDoc = DDBContext.ToDocument(church);
            var expr = new Expression();
            expr.ExpressionStatement = "attribute_not_exists(ChurchId)";
            try
            {
                await ChurchTable.PutItemAsync(churchDoc, new PutItemOperationConfig() { ConditionalExpression = expr });
                this.Response.StatusCode = (int)HttpStatusCode.Created;
                this.Response.ContentType = "application/json";
            }
            catch (ConditionalCheckFailedException)
            {
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }


        [HttpPut("{churchId}")]
        public async Task Put(string churchId, [FromBody]Church church)
        {
            if (!ModelState.IsValid)
            {
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return;
            }

            Logger.LogInformation($"Replacing church with churchId {church.ChurchId}");

            var churchDoc = DDBContext.ToDocument(church);
            var expr = new Expression();
            expr.ExpressionStatement = "attribute_exists(ChurchId)";
            try
            {
                await ChurchTable.PutItemAsync(churchDoc, new PutItemOperationConfig() { ConditionalExpression = expr });
                this.Response.StatusCode = (int)HttpStatusCode.OK;
                this.Response.ContentType = "application/json";
            }
            catch (ConditionalCheckFailedException)
            {
                this.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private Task SetBodyContent(object content)
        {
            var writer = new StreamWriter(this.Response.Body);
            writer.AutoFlush = true;
            var serializationSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return writer.WriteAsync(JsonConvert.SerializeObject(content, serializationSettings));
        }
    }
}