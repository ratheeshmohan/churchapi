using System.Linq;
using Microsoft.AspNetCore.Mvc;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;

namespace parishdirectoryapi.Controllers
{
    public class BaseController : Controller
    {
        protected IDataRepository DataRepository;

        public BaseController(IDataRepository dataRepository)
        {
            DataRepository = dataRepository;
        }

        protected UserContext GetUserContext()
        {
            var loginId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.UserName)?.Value;
            var familyId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.FamilyClaimName)?.Value;
            var churchId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.ChurchIdClaimName)?.Value;

            return new UserContext {FamilyId = familyId, ChurchId = churchId, LoginId = loginId};
        }
    }
}
