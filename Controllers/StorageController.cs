using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using CryptLink.HashedObjectStore;
using CryptLink.SigningFramework;
using System.IO;
using System.Text;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using Microsoft.AspNetCore.Http;
using CryptLink.Host.Entities;
using CryptLink.Host.Extentions;

namespace CryptLink.Host.Controllers {

    [ApiController]
    public class StorageController : ControllerBase {
        private static HashProvider provider = HashProvider.SHA256;
        private static IHashItemStore memStore = Factory.Create(typeof(MemoryStore), provider);
        private static IHashItemStore fileStore = Factory.Create(typeof(FileStore), provider);
        private static ConsistentHash<PeerInfo> peers = new ConsistentHash<PeerInfo>(provider);
        public static PeerInfo HostInfo { get; set; }
        public static int ReplicationWeightMax { get; set; } = 1000;

        private void CheckHostInfo() {
            if (HostInfo == null) {
                HostInfo = new PeerInfo() {
                    Address = $"{Request.Scheme}://{Request.Host}"
                };

                //!todoHostInfo.SetCert(Program.HostCert, provider);
            }
        }


        [HttpGet("/")]
        public object GetDefault() {
            return Ok("Root");
        }

        /// <summary>
        /// Gets a standard type
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpGet("get/{hash}")]
        [RequestSizeLimit(4096)]
        public object Get([FromRoute]string hash) {
            return Get(hash, "binary");
        }


        /// <summary>
        /// Gets a standard type
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpGet("get/{hash}/{type}")]
        [RequestSizeLimit(4096)]
        public object Get([FromRoute]string hash, [FromRoute]string type) {
            if (hash == null) {
                throw new FileNotFoundException();
            }

            var parsedBytes = Utility.DecodeBytes(hash, false);
            if (parsedBytes == null) {
                throw new ArgumentException("Hash B64 can't be parsed");
            }

            if (parsedBytes.Length != provider.GetProviderByteLength()) {
                throw new ArgumentException($"Hash is a invalid length, {provider} should be '{provider.GetProviderByteLength()}'");
            }

            var parsedHash = Hash.FromComputedBytes(parsedBytes, provider, null, null);

            if (parsedHash == null) {
                throw new ArgumentException("Hash bytes are invalid");

            } else {
                bool asBinary = false;

                switch (type) {
                    case "html":
                        type = "text/html";
                        break;
                    case "css":
                        type = "text/css";
                        break;
                    case "js":
                        type = "application/javascript";
                        break;
                    case "json":
                        type = "application/json";
                        break;
                    case "text":
                        type = "text/plain";
                        break;
                    case "jpeg":
                    case "jpg":
                        type = "image/jpeg";
                        asBinary = true;
                        break;
                    case "png":
                        type = "image/png";
                        asBinary = true;
                        break;
                    case "gif":
                        type = "image/gif";
                        asBinary = true;
                        break;
                    case null:
                    case "binary":
                        type = "application/octet-stream";
                        asBinary = true;
                        break;
                    default:
                        type = null;
                        break;
                }

                var item = memStore.GetItem<HashableBytes>(parsedHash);
                if (item == null) {
                    throw new FileNotFoundException();
                } else {
                    Response.StatusCode = 200;
                    Response.ContentType = type;
                }

                if (asBinary) {
                    return item.Value;
                } else {
                    if (item.Value.Length > HostInfo.MaxStoreLength) {
                        var encoding = new ASCIIEncoding();
                        return encoding.GetString(item.Value);
                    } else {
                        throw new InvalidCastException("Item is not text");
                    }
                }
            }
        }

        [HttpPost("store")]
        [RequestSizeLimit(65536)]
        public Hash StoreString([FromForm] string value) {

            if (value == null) {
                throw new ArgumentException("Value was null");
            }

            var encoding = new ASCIIEncoding();

            var hs = new HashableBytes(encoding.GetBytes(value), provider);
            hs.ComputeHash(provider, null);
            memStore.StoreItem(hs);

            return hs.ComputedHash;
        }

        //public bool ForwardObjectToPeer(PeerInfo Peer, IHashable Item) {
        //    return false;
        //}

        [HttpPost("upload")]
        public Hash StoreBinary(IFormFile file) {
            if (file == null) {
                throw new ArgumentException("Upload payload was null");
            }

            var ms = new MemoryStream();
            file.CopyTo(ms);

            var hs = new HashableBytes(ms.ToArray(), provider);
            ms.Dispose();

            hs.ComputeHash(provider, null);
            memStore.StoreItem(hs);

            Console.WriteLine(hs.ComputedHash.ToString());

            Response.StatusCode = 200;
            Response.ContentType = "text/plain";
            return hs.ComputedHash;
        }

        [HttpPost("register")]
        public Hash RegisterPeer([FromForm]PeerInfo peer) {

            if (peer == null || peer.ComputedHash == null) {
                throw new ArgumentNullException("Peer info or peer cert was null");
            }

            peer.ComputeHash(provider);
            peer.ReplicationWeight = Math.Min(peer.ReplicationWeight, ReplicationWeightMax);

            peers.Add(peer, true, peer.ReplicationWeight);
            return peer.ComputedHash;
        }

        [HttpGet("info")]
        public PeerInfo GetHostInfo() {
            CheckHostInfo();
            return HostInfo;
        }

    }
}