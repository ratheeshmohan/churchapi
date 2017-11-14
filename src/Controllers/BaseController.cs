using Microsoft.AspNetCore.Mvc;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;

namespace parishdirectoryapi.Controllers
{
    [Route("api/churches/{churchId}")]
    public class BaseController : Controller
    {
        protected IDataRepository DataRepository;

        public BaseController(IDataRepository dataRepository)
        {
            DataRepository = dataRepository;
        }

        protected UserContext GetUserContext()
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = "fam0001", ChurchId = "smioc", LoginId = "ratheeshmohan@gmail.com" };
        }
    }
}
