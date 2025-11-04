

namespace Domain.AppSettings
{
    public class MinioSettings
    {
        public string MainEndpoint { get; set; }
        public string MainFolder { get; set; }
        public string Bucket { get; set; }
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
    }
}
