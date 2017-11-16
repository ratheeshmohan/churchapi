using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using parishdirectoryapi.Configurations;

namespace parishdirectoryapi.Services
{
    public class CognitoLoginProvider : ILoginProvider
    {
        private readonly CognitoSettings _settngs;
        private readonly ILogger<CognitoLoginProvider> _logger;

        private readonly AmazonCognitoIdentityProviderClient _client = new AmazonCognitoIdentityProviderClient();
        public CognitoLoginProvider(ILogger<CognitoLoginProvider> logger, IOptions<CognitoSettings> cognitoOptions)
        {
            _logger = logger;
            _settngs = cognitoOptions.Value;
        }

        Task<bool> ILoginProvider.CreateLogin(User user)
        {
            return CreateAsync(user);
        }

        Task<bool> ILoginProvider.DeleteLogin(string email)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> CreateAsync(User user)
        {
            var createUserRequest = new AdminCreateUserRequest
            {
                UserPoolId = _settngs.UserPoolId,
                Username = user.Email,
            };

            var attributes = new[]
            {
                new AttributeType
                {
                    Name = "email",
                    Value = user.Email
                },
                new AttributeType
                {
                    Name = "custom:familyId",
                    Value = user.FamlyId
                },
                new AttributeType
                {
                    Name = "custom:churchId",
                    Value = user.ChurchId
                },
                new AttributeType
                {
                    Name = "custom:role",
                    Value = user.Role.ToString()
                }
            };
            createUserRequest.UserAttributes.AddRange(attributes);

            var result = await _client.AdminCreateUserAsync(createUserRequest);

            _logger.LogInformation(result.HttpStatusCode.ToString());
            _logger.LogInformation(result.User.ToString());
            return true; //temp
        }
    }
}
