using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;

namespace parishdirectoryapi.Services
{
    public interface IImageService
    {
        string CreateDownloadableLink(string key);
        string CreateUploadLink(string key);

    }
}
