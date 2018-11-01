using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptLink.Host.Controllers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CryptLink.Host.Entities;
using CryptLink.SigningFramework;
using System.Security.Cryptography.X509Certificates;

namespace CryptLink.Host
{
    public class Program
    {
        public static Cert HostCert = new CertBuilder() { KeyStrength = 2048, SubjectName = "CN=CryptLinkDefault" }.BuildCert();
        public static int Port = 5001;

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {
            if (args.Count() == 1) {
                int.TryParse(args[0], out Port);
            }

            var W = WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseKestrel(options => {
                options.Listen(System.Net.IPAddress.Any, Port, loptions => {
                    loptions.UseHttps(HostCert.GetX509Certificate());
                });
            });

            return W;
        }
        

    }
}
