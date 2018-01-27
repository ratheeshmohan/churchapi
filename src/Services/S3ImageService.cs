
using System;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using parishdirectoryapi.Configurations;

namespace parishdirectoryapi.Services
{
    public class S3ImageService : IImageService
    {
        private readonly ResourceSettings _resourceSettings;
        private readonly ILogger<S3ImageService> _logger;

        public S3ImageService(IOptions<ResourceSettings> resourceSettings, ILogger<S3ImageService> logger)
        {
            _resourceSettings = resourceSettings.Value;
            _logger = logger;
        }

        public string CreateUploadLink(string key)
        {
            using (var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2))
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _resourceSettings.ImagesS3Bucket,
                    Key = key,
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
                return url;
            }
        }

        public string CreateDownloadableLink(string objectKey)
        {
            if (string.IsNullOrEmpty(objectKey))
            {
                return objectKey;
            }

            using (var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.APSoutheast2))
            {

                var request1 = new GetPreSignedUrlRequest
                {
                    BucketName = _resourceSettings.ImagesS3Bucket,
                    Key = objectKey,
                    Expires = DateTime.Now.AddMinutes(5),
                    Verb = HttpVerb.GET
                };

                var url = "";
                try
                {
                    url = s3Client.GetPreSignedURL(request1);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                }

                return url;
            }

        }
    }
}