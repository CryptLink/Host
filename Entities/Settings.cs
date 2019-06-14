using Microsoft.AspNetCore.Server.Kestrel.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptLink.Host.Entities {
    public class Settings {
        public int MaxConcurrentConnections { get; set; } = 100;
        public int MaxRequestBodySize = 10 * 1024;

        public bool GenerateCert { get; set; } = true;
        public bool StoreCert { get; set; } = true;
        public bool RequireCertPassword { get; set; } = true;
        public int CertSize { get; set; } = 4096;

        public MinDataRate MinRequestBodyDataRate { get; set; } = 
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));

        public MinDataRate MinResponseDataRate { get; set; } = 
            new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
    }
}
