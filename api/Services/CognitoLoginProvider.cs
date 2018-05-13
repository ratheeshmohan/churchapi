using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using parishdirectoryapi.Configurations;
using parishdirectoryapi.Security;

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

        async Task<bool> ILoginProvider.CreateLogin(User user)
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
                    Name =AuthPolicy.EmailClaimName,
                    Value = user.Email
                },
                new AttributeType
                {
                    Name = AuthPolicy.FamilyClaimName,
                    Value = user.FamlyId
                },
                new AttributeType
                {
                    Name =  AuthPolicy.ChurchIdClaimName,
                    Value = user.ChurchId
                },
                new AttributeType
                {
                    Name = AuthPolicy.UserRoleClaimName,
                    Value = user.Role.ToString()
                },
                new AttributeType
                {
                    Name ="email_verified",
                    Value = "true"
                }
            };
            createUserRequest.UserAttributes.AddRange(attributes);

            try
            {
                await _client.AdminCreateUserAsync(createUserRequest);
            }
            catch (UsernameExistsException)
            {
                _logger.LogInformation($"Failed to create user with loginId {user.Email}");
                return false;
            }
            return true;
        }

        async Task<bool> ILoginProvider.IsRegistered(string email)
        {
            var request = new AdminGetUserRequest
            {
                Username = email,
                UserPoolId = _settngs.UserPoolId
            };
            try
            {
                await _client.AdminGetUserAsync(request);
                return true;
            }
            catch (UserNotFoundException)
            {
                _logger.LogInformation($"Failed to delete loginId {email} due to Exception");
                return false;
            }
        }

        async Task<bool> ILoginProvider.DeleteLogin(string email)
        {
            var request = new AdminDeleteUserRequest
            {
                Username = email,
                UserPoolId = _settngs.UserPoolId
            };
            try
            {
                await _client.AdminDeleteUserAsync(request);
                return true;
            }
            catch (UserNotFoundException)
            {
                _logger.LogInformation($"Failed to delete loginId {email} due to Exception");
                return false;
            }
        }

    }
}
