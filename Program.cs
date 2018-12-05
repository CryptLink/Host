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
using Microsoft.AspNetCore.Server.Kestrel.Core;

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

            //var W = WebHost.CreateDefaultBuilder(args)
            //.UseStartup<Startup>()
            //.UseKestrel(options => {
            //    options.Listen(System.Net.IPAddress.Any, Port, loptions => {
            //        loptions.UseHttps(HostCert.GetX509Certificate());
            //    });
            //});

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables(prefix: "CL_")
                .AddCommandLine(args)
                .Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel(options =>
                {
                    // https://github.com/aspnet/Docs/blob/master/aspnetcore/fundamentals/servers/kestrel.md

                    options.Limits.MaxConcurrentConnections = 100;
                    options.Limits.MaxRequestBodySize = 10 * 1024;
                    //options.Limits.Http2.MaxStreamsPerConnection = 100;
                    options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                    options.Limits.MinResponseDataRate =
                        new MinDataRate(bytesPerSecond: 100, gracePeriod: TimeSpan.FromSeconds(10));
                })
                .UseStartup<Startup>();

            //var host2 = new WebHostBuilder().
            //    options.Configure(config.Configuration.GetSection("Kestrel"))
            //.Endpoint("HTTPS", opt => {
            //    opt.HttpsOptions.SslProtocols = SslProtocols.Tls12;
            //});

            return host;
        }
        

    }
}
