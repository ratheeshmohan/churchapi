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
    [Route("api/members")]
    [ValidateModel]
    public class MembersController : Controller
    {
        ILogger Logger { get; }
        private IDataRepository _dataRepository;

        public MembersController(IDataRepository dataRepository, ILogger<FamilyController> logger)
        {
            _dataRepository = dataRepository;
            Logger = logger;
        }
        private UserContext GetUserContext()
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = null, ChurchId = "smioc", LoginId = "admin@gmail.com" };
        }
    }
}