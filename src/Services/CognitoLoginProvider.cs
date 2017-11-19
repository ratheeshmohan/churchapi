using System;
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

            try
            {
                await _client.AdminCreateUserAsync(createUserRequest);
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Failed to create user with loginId {user.Email}. Exception: {e} ");
                return false;
            }
            return true;
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
            catch(Exception e)
            {
                _logger.LogInformation($"Failed to delete loginId {email} due to Exception : {e}");
                return false;
            }
        }

    }
}
