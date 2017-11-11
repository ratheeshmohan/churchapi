using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;

namespace parishdirectoryapi.Services
{
    public interface ILoginProvider
    {
        Task<bool> CreateLogin(string email, LoginMetadata metaData);
        Task<bool> DeleteLogin(string email);
    }
}
