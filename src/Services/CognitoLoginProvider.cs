using parishdirectoryapi.Controllers.Models;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using parishdirectoryapi.Configurations;

 namespace parishdirectoryapi.Services
{
    public class CognitoLoginProvider : ILoginProvider
    {
        private readonly CognitoSettings _settngs;

        private readonly AmazonCognitoIdentityProviderClient _client =
            new AmazonCognitoIdentityProviderClient();
 
        public CognitoLoginProvider(IOptions<CognitoSettings> cognitoOptions)
        {
            _settngs = cognitoOptions.Value;
        }

        Task<bool> ILoginProvider.CreateLogin( User user)
        {
            return CreateAsync(user);
        }

        Task<bool> ILoginProvider.DeleteLogin(string email)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> CreateAsync(User user)
        {
            var signUpRequest = new SignUpRequest
            {
                ClientId = _settngs.ClientId,
                Password = GetTempPassword(),
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
                    Name = "familyId",
                    Value = user.FamlyId
                },
                new AttributeType
                {
                    Name = "churchId",
                    Value = user.ChurchId
                },
                new AttributeType
                {
                    Name = "role",
                    Value = user.Role
                }
            };
            signUpRequest.UserAttributes.AddRange(attributes);

            var result = await _client.SignUpAsync(signUpRequest);
            return true; //temp
        }

        private static string GetTempPassword()
        {
            return "DD44E12 $$sh";
        }
    }
}
