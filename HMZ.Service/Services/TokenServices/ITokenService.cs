using System.Security.Claims;
using Google.Apis.Auth;
using HMZ.Database.Entities;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;

namespace HMZ.Service.Services.TokenServices
{
    public interface ITokenService
    {
        Task<string> CreateToken(User user);
        Task<string> GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<GoogleJsonWebSignature.Payload> VerifyGoogleToken(ExternalAuth externalAuth);
        Task<UserView> VerifyFacebookToken(ExternalAuth externalAuth);
    }
}
