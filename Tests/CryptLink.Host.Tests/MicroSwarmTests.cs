using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using NUnit.Framework;
using CryptLink.Host.Extentions;

namespace CryptLink.Host.Tests
{
    [TestFixture]
    public class MicroSwarmTests
    {

        [SetUp]
        public void Setup() {
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        [Test]
        public void SingleHostGetInfo()
        {
            var process = StartHost(5001);
            WaitForPort(5001);
            var peer = GetResponse<Entities.PeerInfo>("https://localhost:5001/info");

            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            peer.Store(Guid.NewGuid().ToString());

            

            Assert.NotNull(peer);
            process.Kill();
        }

        [Test]
        public void ManyHostsGetInfo() {
            var processes = new List<Process>();
            var peers = new List<Entities.PeerInfo>();
            int startingPort = 5000;

            try {
                for (int i = 1; i <= 20; i++) {
                    processes.Add(StartHost(startingPort + i));

                    WaitForPort(startingPort + i);

                    var peerInfo = GetResponse<Entities.PeerInfo>($"https://localhost:{startingPort + i}/info");
                    Assert.NotNull(peerInfo);

                    peers.Add(peerInfo);
                }

                foreach (var peer in peers) {
                    peer.Store(Guid.NewGuid().ToString());
                }
            } catch (Exception ex) {
                Assert.Fail(ex.Message);
            }

            foreach (var process in processes) {
                process.Kill();
            }
        }

        public Process StartHost(int port) {
            var exePath = Path.GetRelativePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                "../../../../../bin/Debug/netcoreapp2.1/CryptLink.Host.dll");

            var process = new Process() {
                StartInfo = new ProcessStartInfo() {
                    FileName = "dotnet",
                    Arguments = $"exec {exePath} {port}",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };

            process.Start();
            return process;
        }

        private void WaitForPort(int Port) {
            while (!IsPortOpen("localhost", Port, new TimeSpan(0, 0, 5))) {
                System.Threading.Thread.Sleep(500);
            };
        }

        bool IsPortOpen(string host, int port, TimeSpan timeout) {
            try {
                using (var client = new TcpClient()) {
                    var result = client.BeginConnect(host, port, null, null);
                    var success = result.AsyncWaitHandle.WaitOne(timeout);
                    if (!success) {
                        return false;
                    }

                    client.EndConnect(result);
                }

            } catch {
                return false;
            }
            return true;
        }

        public T GetResponse<T>(string Url) {
            try {
                WebRequest request = WebRequest.Create(Url);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();

                var r = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseFromServer);

                reader.Close();
                dataStream.Close();
                response.Close();

                return r;
            } catch (Exception ex) {
                return default(T);
            }
            
        }
    }
}
