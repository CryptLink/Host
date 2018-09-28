using CryptLink.SigningFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace CryptLink.Host.Deploy {
    class Program {

        static int uploadChunkSize = 4096;

        static void Main(string[] args) {
            string deployFolder = "Content";
            string deployUrl = "https://localhost:5001/store-string";
            Dictionary<string, Hash> PostedFiles = new Dictionary<string, Hash>();

            if (args.Length == 1) {
                deployFolder = args[0];
            }

            if (args.Length == 2) {
                deployUrl = args[1];
            }

            Console.WriteLine("Using: " + deployUrl);
            
            List<string> htmls = Directory.GetFiles(deployFolder).Where(f => f.ToLower().EndsWith(".html") || f.ToLower().EndsWith(".htm")).ToList();
            //List<string> otherFiles = Directory.GetFiles(deployFolder).Where(f => htmls.Contains(f) == false).ToList();

            foreach (var file in htmls) {
                var h = PostString(file, deployUrl);

                Console.WriteLine(Path.GetFileName(file));

                if (h != null) {
                    PostedFiles.Add(file, h);
                    Console.WriteLine($"https://localhost:5001/get/{Utility.EncodeBytes(h.Bytes, true, false)}/html");
                } else {
                    Console.WriteLine();
                }
            }
        }

        static Hash SendFile(string FilePath, string ServerUrl) {
            byte[] fileData;
            var fileReader = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
            var webRequest = (HttpWebRequest)WebRequest.Create(ServerUrl);
            webRequest.Method = "POST";
            webRequest.ContentLength = fileReader.Length;
            webRequest.Timeout = 600000;
            webRequest.Credentials = CredentialCache.DefaultCredentials;
            webRequest.AllowWriteStreamBuffering = false;
            var requestStream = webRequest.GetRequestStream();

            long fileSize = fileReader.Length;
            long remainingBytes = fileSize;
            int numberOfBytesRead = 0, done = 0;

            while (numberOfBytesRead < fileSize) {
                //SetByteArray(out fileData, remainingBytes);
                fileData = remainingBytes < uploadChunkSize ? new byte[remainingBytes] : new byte[uploadChunkSize];

                done = fileReader.Read(fileData, 0, fileData.Length);
                requestStream.Write(fileData, 0, fileData.Length);
                
                numberOfBytesRead += done;
                remainingBytes -= done;
            }
            fileReader.Close();

            var response = (HttpWebResponse)webRequest.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            byte[] hashResult = Utility.DecodeBytes(responseString, false);

            return Hash.FromComputedBytes(hashResult, HashProvider.SHA256, null, null);
        }

        public static Hash PostString(string FilePath, string DeployUrl) {
            if (File.Exists(FilePath)) {
                var postData = System.Web.HttpUtility.UrlEncode(File.ReadAllText(FilePath));
                var request = (HttpWebRequest)WebRequest.Create(DeployUrl);

                var data = Encoding.ASCII.GetBytes("value=" + postData);

                request.Method = "POST";
                //request.ContentType = "application/json";
                request.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

                request.ContentLength = data.Length;

                using (var stream = request.GetRequestStream()) {
                    stream.Write(data, 0, data.Length);
                }

                var response = (HttpWebResponse)request.GetResponse();
                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                byte[] hashResult = Utility.DecodeBytes(responseString, false);

                return Hash.FromComputedBytes(hashResult, HashProvider.SHA256, null, null);
            } else {
                return null;
            }
        }
    }
}
