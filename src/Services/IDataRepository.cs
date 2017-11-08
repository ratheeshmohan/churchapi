using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace parishdirectoryapi.Services
{
    public interface IDataRepository
    {
        Task<bool> AddChurch(Church church);
        Task<Church> GetChurch(string churchId);
        Task<bool> UpdateChurch(Church church);

        Task<bool> CreateFamily(string churchId, string familyId);
        Task<bool> DeleteFamily(string churchId, string familyId);
        Task<Family> GetFamily(string churchId, string familyId);
        Task<bool> UpdateFamily(Family family);

        Task<bool> AddMembers(IEnumerable<Member> members);
        Task<bool> RemoveMember(string churchId, IEnumerable<string> memberIds);
        Task<IEnumerable<Member>> GetMembers(string churchId, IEnumerable<string> memberIds);
        Task<bool> UpdateMember(Member member);

        Task<IEnumerable<Family>> GetFamilies(string churchId);
    }
}