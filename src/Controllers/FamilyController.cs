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
using parishdirectoryapi.Extenstions;
using System.Linq;
using System.Collections.Generic;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/family")]
    [ValidateModel]
    public class FamilyController : Controller
    {
        private IDataRepository _dataRepository;

        public FamilyController(IDataRepository dataRepository)
        {
            _dataRepository = dataRepository;
        }
        /* 
                [HttpPut]
                public async Task<IActionResult> Put(FamilyPofile profile)
                {
                    var family = profile.ToFamily();

                    var userContext = GetUserContext();
                    family.FamilyId = userContext.FamilyId;
                    family.ChurchId = userContext.ChurchId;

                    var expr = new Expression
                    {
                        ExpressionStatement = "attribute_exists(FamilyId)"
                    };

                    var document = DynamodbWrapper.DDBContext.ToDocument(family);
                    try
                    {
                        await DynamodbWrapper.FamiliesTable.PutItemAsync(document,
                            new PutItemOperationConfig() { ConditionalExpression = expr });
                        return Ok();
                    }
                    catch (ConditionalCheckFailedException)
                    {
                        return BadRequest($"Church with id {family.ChurchId} and {family.ChurchId} doesnot exists");
                    }
                }
        */
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userContext = GetUserContext();
            var family = await _dataRepository.GetFamily(userContext.ChurchId, userContext.FamilyId);
            if (family == null)
            {
                return NotFound();
            }

            var familyVM = new FamilyViewModel();
            familyVM.Address = family.Address;
            familyVM.HomeParish = family.HomeParish;
            familyVM.PhotoUrl = family.PhotoUrl;

            var members2RoleMap = new Dictionary<string, FamilyRole>();
            foreach (var m in family.Members)
            {
                members2RoleMap[m.MemberId] = m.Role;
            }

            if (members2RoleMap.Keys.Any())
            {
                var members = await _dataRepository.GetMembers(userContext.ChurchId, members2RoleMap.Keys);
                familyVM.Members = members.Select(m =>
                {
                    var vm = m.ToMemberViewModel();
                    vm.Role = members2RoleMap[vm.MemberId];
                    return vm;
                });
            }
            return Ok(familyVM);
        }

        private UserContext GetUserContext()
        {
            //TEMP: read from user claims
            return new UserContext { FamilyId = "fam0001", ChurchId = "smioc", LoginId = "ratheeshmohan@gmail.com" };
        }
    }
}
