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
        private readonly IImageService _imageService;
        private readonly ILogger<FilesController> _logger;

        public FilesController(IDataRepository dataRepository,
              IImageService imageService,
                ILogger<FilesController> logger) : base(dataRepository)
        {
            _imageService = imageService;
            _logger = logger;
        }

        [HttpPost("uploadlink")]
        [Authorize(Policy = AuthPolicy.ChurchMemberPolicy)]
        public IActionResult UploadLink()
        {
            var key = Guid.NewGuid().ToString();
            var url = _imageService.CreateUploadLink(key);
            return Ok(new { Key = key, Link = url });
        }
    }
}