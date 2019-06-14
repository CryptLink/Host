using CryptLink.SigningFramework;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using RestSharp;
using CryptLink.Host.Entities;

namespace CryptLink.Host.Extentions {
    public static class PeerExtentions {

        public static void SetCert(this PeerInfo For, Cert _Cert, HashProvider Provider) {
            For.Cert = _Cert;
            For.PublicKey = For.Cert.PublicKey.EncodedKeyValue.RawData;
            For.Cert.ComputeHash(Provider);
            For.ComputeHash(Provider);
        }

        /// <summary>
        /// Registers another peer with this one
        /// </summary>
        /// <param name="ToPeer">Peer to registering into</param>
        /// <param name="PeerToRegister">Peer you are regestering to this</param>
        public static void RegisterPeer(this PeerInfo ToPeer, PeerInfo PeerToRegister) {
            ToPeer.SendQueue.Enqueue(PeerToRegister);
        }

        public static void Store(this PeerInfo ToPeer, IHashable Item) {
            ToPeer.SendQueue.Enqueue(Item);
            ToPeer.ProcessPeerQueue();
        }

        public static void Store(this PeerInfo ToPeer, string Item) {
            ToPeer.SendQueue.Enqueue(new HashableString(Item));
            ToPeer.ProcessPeerQueue();
        }

        private static bool _processingSendQueue = false;

        public static void ProcessPeerQueue(this PeerInfo ToPeer) {

            if (_processingSendQueue == true) {
                return;
            }

            IHashable item;

            //todo: this needs to check some parts of a cert, but allow self signed certs
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

            while (ToPeer.SendQueue.TryDequeue(out item)) {

                if (item.GetType() == typeof(HashableString)) {
                    ToPeer.RequestStoreString((HashableString)item);
                } else if (item.GetType() == typeof(PeerInfo)) {
                    ToPeer.RequestRegister((PeerInfo)item);
                } else {
                    //unsupported type
                    ToPeer.Errors.Add($"{DateTime.Now} Error posting {item.ComputedHash.ToString()}, unknown type: {item.GetType().ToString()}");
                }
            }

            _processingSendQueue = false;
        }

        private static void RequestRegister(this PeerInfo ToPeer, PeerInfo Item) {
            var client = new RestClient(ToPeer.Address);
            var request = new RestRequest("register", Method.POST);
            request.AddParameter("peer", Item);

            var asyncHandler = client.ExecuteAsync<Hash>(request, r => {

                if (r.ResponseStatus == ResponseStatus.Completed) {
                    ToPeer.KnownObjects.AddOrUpdate(r.Data, DateTime.Now, (k, v) => DateTime.Now);
                } else {
                    ToPeer.Errors.Add($"{DateTime.Now} Error posting {Item.ComputedHash.ToString()}, {r}");
                }
            });
        }

        private static Task<T> RequestItem<T>(this PeerInfo ToPeer, Hash Item) {

            var client = new RestClient(ToPeer.Address);
            var request = new RestRequest($"get/{Item.ToString()}", Method.GET);

                        


            var asyncHandler = client.ExecuteAsync<byte[]>(request, r => {

                if (r.ResponseStatus == ResponseStatus.Completed) {
                    ToPeer.KnownObjects.AddOrUpdate(Hash.FromComputedBytes(r.Data, ), DateTime.Now, (k, v) => DateTime.Now);
                } else {
                    ToPeer.Errors.Add($"{DateTime.Now} Error posting {Item.ComputedHash.ToString()}, {r}");
                }
            });
        }

        private static void RequestStoreString(this PeerInfo ToPeer, HashableString Item) {
            var client = new RestClient(ToPeer.Address);
            var request = new RestRequest("store", Method.POST);
            request.AddParameter("value", Item.Value);

            var asyncHandler = client.ExecuteAsync<Hash>(request, r => {

                if (r.ResponseStatus == ResponseStatus.Completed) {
                    ToPeer.KnownObjects.AddOrUpdate(r.Data, DateTime.Now, (k, v) => DateTime.Now);
                } else {
                    ToPeer.Errors.Add($"{DateTime.Now} Error posting {Item.ComputedHash.ToString()}, {r}");
                }
            });
        }

        public static async Task<Hash> UploadAsync(this PeerInfo ToPeer, Stream Item) {
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
