
using parishdirectoryapi.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace parishdirectoryapi.Services
{
    public interface IDataRepository
    {
        Task<bool> AddChurch(Church church);
        Task<Church> GetChurch(string churchId);
        Task<bool> UpdateChurch(Church church);

        //Task<bool> CreateFamily(string churchId, string familyId);
        Task<bool> AddFamily(Family family);
        Task<bool> UpdateFamily(Family family);
        Task<Family> GetFamily(string churchId, string familyId);
        Task<bool> DeleteFamily(string churchId, string familyId);

        Task<bool> AddMembers(IEnumerable<Member> members);
        Task<bool> RemoveMember(string churchId, IEnumerable<string> memberIds);
        Task<IEnumerable<Member>> GetMembers(string churchId, IEnumerable<string> memberIds);
        Task<bool> UpdateMember(Member member);

        Task<IEnumerable<Family>> GetFamilies(string churchId);
    }
}