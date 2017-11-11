using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;

namespace parishdirectoryapi.Services
{
    internal class CognitoLoginProvider : ILoginProvider
    {

        Task<bool> ILoginProvider.CreateLogin(string email, LoginMetadata metaData)
        {
            return Task.FromResult(true);
        }

        Task<bool> ILoginProvider.DeleteLogin(string email)
        {
            return Task.FromResult(true);
        }
    }
}
