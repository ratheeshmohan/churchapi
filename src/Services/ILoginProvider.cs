using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;

namespace parishdirectoryapi.Services
{
    public interface ILoginProvider
    {
        Task<bool> CreateLogin(User user);
        Task<bool> DeleteLogin(string loginId);
    }
}
