using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/members")]
    [ValidateModel]
    public class MembersController : BaseController
    {
        ILogger Logger { get; }
        public MembersController(IDataRepository dataRepository, ILogger<FamilyController> logger) : base(dataRepository)
        {
            Logger = logger;
        }
    }
}