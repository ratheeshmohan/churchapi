using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Models;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;
// ReSharper disable PossibleMultipleEnumeration

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    public class DirectoryController : BaseController
    {
        private readonly ILogger<DirectoryController> _logger;

        public DirectoryController(IDataRepository dataRepository,
         ILogger<DirectoryController> logger) : base(dataRepository)
        {
            _logger = logger;
        }


        [HttpGet]
        [Authorize(Policy = AuthPolicy.AllUserPolicy)]
        public async Task<IActionResult> Get()
        {
            var context = GetUserContext();
            var families = await DataRepository.GetFamilies(context.ChurchId);
            var allMemberIds = families.Where(f => f.Members != null).SelectMany(f => f.Members.Select(m => m.MemberId));
            var allMembers = await DataRepository.GetMembers(context.ChurchId, allMemberIds);
            var mappedMembers = allMembers.Select(member =>
             {
                 var m = new MemberViewModel
                 {
                     MemberId = member.MemberId,
                     FirstName = member.FirstName,
                     MiddleName = member.MiddleName,
                     LastName = member.LastName,
                     NickName = member.NickName,
                     Gender = member.Gender,
                     FacebookUrl = member.FacebookUrl,
                     LinkedInUrl = member.LinkedInUrl
                 };

                 if (member.DisplayDateOfBirth.HasValue && member.DisplayDateOfBirth.Value &&
                        !string.IsNullOrEmpty(member.DateOfBirth) && DateTime.TryParse(member.DateOfBirth, out var date))
                 {
                     m.DateOfBirth = date.ToString("d MMMM", CultureInfo.InvariantCulture);
                 }
                 else
                 {
                     m.DateOfBirth = "";
                 }

                 if (member.DisplayDateOfWedding.HasValue && member.DisplayDateOfWedding.Value &&
                    !string.IsNullOrEmpty(member.DateOfWedding) && DateTime.TryParse(member.DateOfWedding, out var wdate))
                 {
                     m.DateOfWedding = wdate.ToString("d MMMM", CultureInfo.InvariantCulture);
                 }
                 else
                 {
                     m.DateOfWedding = "";
                 }

                 if (!member.DisplayPhone.HasValue || !member.DisplayPhone.Value)
                 {
                     m.Phone = "";
                 }
                 if (!member.DisplayEmail.HasValue || !member.DisplayEmail.Value)
                 {
                     m.EmailId = "";
                 }
                 return m;
             });

            var membersMap = mappedMembers.ToDictionary(m => m.MemberId);
            var directory = families.Select(f =>
            {
                var item = new DirectoryItem
                {
                    FamilyId = f.FamilyId,
                    Address = f.Address,
                    HomeParish = f.HomeParish,
                    PhotoUrl = f.PhotoUrl,
                };
                if (f.Members != null)
                {
                    item.Members = f.Members.Select(m => new DirectoryMember()
                    {
                        Role = m.Role,
                        Member = membersMap[m.MemberId]
                    });
                }
                return item;
            });
            return Ok(directory);
        }
    }
}