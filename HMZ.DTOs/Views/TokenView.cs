namespace HMZ.DTOs.Views
{
    public class TokenView
    {
        public String? AccessToken { get; set; }
        public String? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}