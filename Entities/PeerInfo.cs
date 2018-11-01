using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptLink.SigningFramework;
using Newtonsoft.Json;

namespace CryptLink.Host.Entities
{
    public class PeerInfo : Hashable {

        [JsonIgnore]
        public Cert Cert { get; set; }
        public string Address { get; set; }

        public int ReplicationWeight { get; set; } = 1000;
        public long MaxStoreLength { get; set; } = 1024 * 128;
        public long MaxUploadLength { get; set; } = 1024 * 1024;

        [JsonIgnore]
        public ConcurrentQueue<IHashable> SendQueue { get; set; } = new ConcurrentQueue<IHashable>();

        [JsonIgnore]
        public ConcurrentDictionary<Hash, DateTime> KnownObjects { get; set; } = new ConcurrentDictionary<Hash, DateTime>();

        [JsonIgnore]
        public ConcurrentBag<string> Errors { get; set; } = new ConcurrentBag<string>();

        public DateTime? LastRegistered { get; set; }

        public byte[] PublicKey { get; set; }

        public override byte[] GetHashableData() {
            return PublicKey;
        }
    }
}
