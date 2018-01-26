using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using parishdirectoryapi.Models;
using parishdirectoryapi.Controllers.Actions;
using parishdirectoryapi.Services;
using parishdirectoryapi.Security;
using Amazon.Auth.AccessControlPolicy;
using Microsoft.AspNetCore.Authorization;
using parishdirectoryapi.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Amazon.S3;
using Amazon.S3.Model;
using System;

namespace parishdirectoryapi.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    [Route("api/[controller]")]
    [ValidateModel]
    public class FilesController : BaseController
    {
        private readonly ResourceSettings _resourceSettings;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IDataRepository dataRepository,
               IOptions<ResourceSettings> resourceSettings,
                ILogger<FilesController> logger) : base(dataRepository)
        {
            _resourceSettings = resourceSettings.Value;
            _logger = logger;
        }

        [HttpPost("uploadlink")]
        [Authorize(Policy = AuthPolicy.ChurchMemberPolicy)]
        public IActionResult UploadLink()
        {
            using (var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _resourceSettings.ImagesS3Bucket,
                    Key = Guid.NewGuid().ToString(),
                    Expires = DateTime.Now.AddMinutes(5),
                    Verb = HttpVerb.PUT
                };
                var url = "";
                try
                {
                    url = s3Client.GetPreSignedURL(request);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }
                return Ok(url);
            }
        }
    }
}