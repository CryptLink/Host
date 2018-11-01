using CryptLink.SigningFramework;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using CryptLink.Host.Entities;

namespace CryptLink.Host.Extentions {
    public static class PeerExtentions {

        public static void SetCert(this Entities.PeerInfo For, Cert _Cert, HashProvider Provider) {
            For.Cert = _Cert;
            For.PublicKey = For.Cert.PublicKey.EncodedKeyValue.RawData;
            For.Cert.ComputeHash(Provider);
            For.ComputeHash(Provider);
        }

        public static void Register(this Entities.PeerInfo ToPeer) {
            ToPeer.SendQueue.Enqueue(ToPeer);
        }

        public static void Store(this Entities.PeerInfo ToPeer, IHashable Item) {
            ToPeer.SendQueue.Enqueue(Item);
            ToPeer.ProcessSendQueue();
        }

        public static void Store(this Entities.PeerInfo ToPeer, string Item) {
            ToPeer.SendQueue.Enqueue(new HashableString(Item));
            ToPeer.ProcessSendQueue();
        }

        private static bool _processingSendQueue = false;

        public static void ProcessSendQueue(this Entities.PeerInfo ToPeer) {

            if (_processingSendQueue == true) {
                return;
            }

            if (ToPeer.LastRegistered == null) {
                ToPeer.Register();
            }

            IHashable item;

            //todo: this needs to check some parts of a cert, but allow self signed certs
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            while (ToPeer.SendQueue.TryDequeue(out item)) {
                var client = new RestClient(ToPeer.Address);

                if (item.GetType() == typeof(HashableString)) {
                    var hItem = (HashableString)item;

                    var request = new RestRequest("store", Method.POST);
                    request.AddParameter("value", hItem.Value);

                    var asyncHandler = client.ExecuteAsync<Hash>(request, r => {
                        
                        if (r.ResponseStatus == ResponseStatus.Completed) {
                            ToPeer.KnownObjects.AddOrUpdate(r.Data, DateTime.Now, (k,v) => DateTime.Now);
                        } else {
                            ToPeer.Errors.Add($"{DateTime.Now} Error posting {hItem.ComputedHash.ToString()}, {r}");
                        }
                    });

                    //asyncHandler.WebRequest.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

                } else if (item.GetType() == typeof(PeerInfo)) {
                    var hItem = (PeerInfo)item;

                    var request = new RestRequest("register", Method.POST);
                    request.AddParameter("peer", hItem);

                    var asyncHandler = client.ExecuteAsync<Hash>(request, r => {

                        if (r.ResponseStatus == ResponseStatus.Completed) {
                            ToPeer.KnownObjects.AddOrUpdate(r.Data, DateTime.Now, (k, v) => DateTime.Now);
                        } else {
                            ToPeer.Errors.Add($"{DateTime.Now} Error posting {hItem.ComputedHash.ToString()}, {r}");
                        }
                    });
                } else {
                    //unsupported type
                    ToPeer.Errors.Add($"{DateTime.Now} Error posting {item.ComputedHash.ToString()}, unknown type: {item.GetType().ToString()}");
                }
            }

            _processingSendQueue = false;
        }

        public static async Task<Hash> UploadAsync(this Entities.PeerInfo ToPeer, Stream Item) {
            //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            //var client = new RestClient(ToPeer.Address);
            //var request = new RestRequest("store", Method.POST);
            //request.AddParameter("name", "value"); // adds to POST or URL querystring based on Method
            //request.AddFile("file", path);
            //IRestResponse response = client.Execute(request);
            //var content = response.Content; // raw content as string
            //return await client.ExecuteAsync(request);

            throw new NotImplementedException();
        }
    }
}
