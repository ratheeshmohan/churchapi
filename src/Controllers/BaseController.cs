using System;
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

        protected internal UserContext GetUserContext()
        {
            if (User == null)
                return null;

            var loginId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.UserName)?.Value;
            var familyId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.FamilyClaimName)?.Value;
            var churchId = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.ChurchIdClaimName)?.Value;
            var role = User.Claims.FirstOrDefault(x => x.Type == AuthPolicy.UserRoleClaimName)?.Value;

            Models.UserRole userRole;
            Enum.TryParse(role, out userRole);

            return new UserContext
            {
                FamilyId = familyId,
                ChurchId = churchId,
                LoginId = loginId,
                Role = userRole
            };
        }
    }
}