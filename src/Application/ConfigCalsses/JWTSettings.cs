
namespace Application.ConfigCalsses
{
    public class JWTSettings
    {
        public string Key { get; set; } = default!;
        public string Issuer { get; set; } = default!;
        public string Audience { get; set; } = default!;
        public int Time { get; set; }
        public int TimeForCookies { get; set; }=default!;
    }
}
