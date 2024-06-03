using Google.Apis.Auth;
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using RestSharp;

namespace HMZ.Service.Services.TokenServices
{

    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _securityKey;
        private readonly UserManager<User> _userManager;
        private readonly IConfigurationSection _googleAuthSettings;
        private readonly IConfigurationSection _facebookAuthSettings;

        public TokenService(IConfiguration config, UserManager<User> userManager)
        {
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            _userManager = userManager;
            _googleAuthSettings = config.GetSection("Authentication:GoogleOAuth");
            _facebookAuthSettings = config.GetSection("Authentication:FacebookOAuth");
        }
        public async Task<string> CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName,user.UserName),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
            };
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var creds = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return await Task.FromResult(Convert.ToBase64String(randomNumber));
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _securityKey,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha512, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");
            return principal;
        }

        public async Task<UserView> VerifyFacebookToken(ExternalAuth externalAuth)
        {
            // verify access token with facebook API to authenticate
            var client = new RestClient(_facebookAuthSettings["ClientUrl"]);
            var request = new RestRequest($"me?access_token={externalAuth.IdToken}");
            var response = await client.GetAsync(request);
            if (!response.IsSuccessStatusCode)
                return null;
            var contents =  response.Content;
            var result = JsonConvert.DeserializeObject<FacebookUserData>(contents);
            var user = await _userManager.FindByEmailAsync(result.Email);
            if (user == null)
            {
               var  appUser = new User()
                {
                    Email = result.Email,
                    UserName = result.Email.Split("@")[0].ToLower(),
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    Image = result.Data.Picture.Url,
                    DateOfBirth = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    AccountType = EAccountType.Facebook,
                    IsActive = true
                };
                var createdUser = await _userManager.CreateAsync(appUser, Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 8));
                if (!createdUser.Succeeded)
                    return null;
                await _userManager.AddToRoleAsync(appUser, EUserRoles.Member.ToString());
                return  new UserView
                {
                    Id = appUser.Id,
                    Username = appUser.UserName,
                    Email = appUser.Email,
                    FirstName = appUser.FirstName,
                    LastName = appUser.LastName,
                    Image = appUser.Image,
                };
            }
            return new UserView
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Image = user.Image,
            };
        }

        public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuth externalAuth)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _googleAuthSettings["ClientId"] }
            };
            return await GoogleJsonWebSignature.ValidateAsync(externalAuth.IdToken, settings);
        }
    }

    public class FacebookUserData {
        [JsonProperty("email")]
        public string? Email { get; set; }
        [JsonProperty("first_name")]
        public string? FirstName { get; set; }
        [JsonProperty("picture")]
        public PictureData? Data { get; set; }
        [JsonProperty("last_name")]
        public string? LastName { get; set; }
    }
    public class PictureData {
        [JsonProperty("data")]
        public Picture? Picture { get; set; }
    }
    public class Picture {
        [JsonProperty("url")]
        public string? Url { get; set; }
    }
}
