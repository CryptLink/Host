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
using System.Security.Authentication;
using Microsoft.AspNetCore.Connections;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace CryptLink.Host
{
    public class Program
    {
        public static Cert HostCert = new CertBuilder() { KeyStrength = 2048, SubjectName = "CN=CryptLinkDefault" }.BuildCert();
        //public static int Port = 5001;

        private static string ConfigFile = "appsettings.json";

        public static void Main(string[] args)
        {
            if (args.Count() == 1) {
                ConfigFile = args[0];
            }

            if (!File.Exists(ConfigFile)) {
                throw new FileLoadException($"The config file: '{ConfigFile}' does not exist, can't start the application");
            }

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) {


            //var W = WebHost.CreateDefaultBuilder(args)
            //.UseStartup<Startup>()
            //.UseKestrel(options => {
            //    options.Listen(System.Net.IPAddress.Any, Port, loptions => {
            //        loptions.UseHttps(HostCert.GetX509Certificate());
            //    });
            //});


            //Load the json so we can check if any certs have been specified
            var configJson = JObject.Parse(File.ReadAllText(ConfigFile));
            var certificates = configJson.SelectToken("$.Kestrel..Certificates");
            var certificate = configJson.SelectToken("$.Kestrel..Certificate");

            var config = new ConfigurationBuilder()
                .AddJsonFile(ConfigFile, optional: false, reloadOnChange: true)
                .AddEnvironmentVariables(prefix: "CL_")
                .AddCommandLine(args)
                .Build();

            //look for a cert

            var certs = new ConcurrentDictionary<string, X509Certificate2>();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel(c => {
                    c.ConfigureHttpsDefaults(opt => {
                        opt.SslProtocols = SslProtocols.Tls12;
                        opt.ServerCertificateSelector = (connectionContext, name) => {
                            X509Certificate2 cert = null;

                            if (name != null && certs.TryGetValue(name, out cert)) {
                                return cert;
                            }

                            return HostCert.GetX509Certificate();
                        };
                    });
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
