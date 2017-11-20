using System;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using parishdirectoryapi.Configurations;
using parishdirectoryapi.Controllers.Models;
using parishdirectoryapi.Security;
using parishdirectoryapi.Services;

namespace parishdirectoryapi
{
    public class Startup
    {
        public const string AppS3BucketKey = "AppS3Bucket";

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public static IConfigurationRoot Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Bearer", new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build());

                options.AddPolicy(AuthPolicy.ChurchAdministratorPolicy,
                    policy =>
                    {
                        policy.RequireClaim(AuthPolicy.UserRoleClaimName, UserRole.Administrator.ToString());
                    });
            });

            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                    options.SerializerSettings.ContractResolver =
                        new Newtonsoft.Json.Serialization.DefaultContractResolver();
                });
            // Pull in any SDK configuration from Configuration object
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());

            // Add S3 to the ASP.NET Core dependency injection framework.
            services.AddAWSService<Amazon.S3.IAmazonS3>();
            services.AddScoped<IDataRepository, DataRepository>();
            services.AddScoped<ILoginProvider, CognitoLoginProvider>();
            services.Configure<CognitoSettings>(options => Configuration.GetSection("CognitoSettings").Bind(options));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLambdaLogger(Configuration.GetLambdaLoggerOptions());

            EnbaleJwtAuthtentication(app);
            app.UseMvc();
        }


        private void EnbaleJwtAuthtentication(IApplicationBuilder app)
        {
     

            var bearerOptions = new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = CognitoTokenValidationParameters(),
            };

            app.UseJwtBearerAuthentication(bearerOptions);
            
        }

        private TokenValidationParameters CognitoTokenValidationParameters()
        {
            const string issuer = "https://cognito-idp.ap-southeast-2.amazonaws.com/ap-southeast-2_3PfLVfMii";
            const string key =
                "kfxKv-cPpX3YszkIFGxKjWn2r2byRtaRRbVCZikRSreo2DaoigxfGKe5aXaoxK7pCdXEYtWakQ528wrcPZguMGWcnGmYPFCvZOmy-OZDMIQRUtM1dwPdcw7SW_pe1m-eTuc12T8TYBEesJ63jCJgqSkpqpWERXnkNPddRiEOoPFstxJ1OVr3V03OtX-YhJIWCqhrFYAh3GSA4o1frRGGTRBxIPJ0-79tV6IVH7SdmB4ccK4Ds45iddC9gI7l7qz_jFrIQMEys11IiO9z2dEYIVeO9-WCUYgg1aGVC0oNJN90ftAZFl2-ybYTIR4wABopHuFozn9KX_cbKIeNnIVmkQ";
            const string expo = "AQAB";

            // Basic settings - signing key to validate with, audience and issuer.
            return new TokenValidationParameters
            {
                // Basic settings - signing key to validate with, IssuerSigningKey and issuer.
                IssuerSigningKey = SigningKey(key, expo),
                ValidIssuer = issuer,

                // when receiving a token, check that the signing key
                ValidateIssuerSigningKey = true,

                // When receiving a token, check that we've signed it.
                ValidateIssuer = true,

                // When receiving a token, check that it is still valid.
                ValidateLifetime = true,

                // Do not validate Audience on the "access" token since Cognito does not supply it but it is      on the "id"
                ValidateAudience = false,

                // This defines the maximum allowable clock skew - i.e. provides a tolerance on the token expiry time 
                // when validating the lifetime. As we're creating the tokens locally and validating them on the same 
                // machines which should have synchronised time, this can be set to zero. Where external tokens are
                // used, some leeway here could be useful.
                ClockSkew = TimeSpan.FromMinutes(0),
            };

        }

        private static RsaSecurityKey SigningKey(string key, string expo)
        {
            var rsa = RSA.Create();
            rsa.ImportParameters(
                new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(key),
                    Exponent = Base64UrlEncoder.DecodeBytes(expo)
                }
            );
            return new RsaSecurityKey(rsa);
        }

    }
}

