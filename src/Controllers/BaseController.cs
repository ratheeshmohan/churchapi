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
            var email = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.EmailClaimName)?.Value;
            var familyId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.FamilyClaimName)?.Value;
            var loginId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.EmailClaimName)?.Value;

            return new UserContext {FamilyId = email, ChurchId = familyId, LoginId = loginId};
        }
    }
}
