namespace WebAPI.Test.DataModels
{
    internal class LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public string[] Roles { get; set; } 
    }
}
