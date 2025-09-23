using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogHybrid.Infrastructure.Configuration
{
    public class CloudflareR2Options
    {
        public const string SectionName = "CloudflareR2";

        public string AccountId { get; set; } = string.Empty;
        public string AccessKeyId { get; set; } = string.Empty;
        public string SecretAccessKey { get; set; } = string.Empty;
        public string BucketName { get; set; } = string.Empty;
        public string PublicDomain { get; set; } = string.Empty;
        public string Region { get; set; } = "auto";
    }
}
