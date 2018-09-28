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

namespace CryptLink.Host.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class StorageController : ControllerBase
    {
        private static HashProvider provider = HashProvider.SHA256;
        private static IHashItemStore memStore = Factory.Create(typeof(MemoryStore), provider);

        /// <summary>
        /// Gets a standard type
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        [HttpGet("get/{hash}/{type}")]
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
                    case "zip":
                        type = "application/pdf";
                        asBinary = true;
                        break;
                    case "jpeg":
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
                    case "binary":
                        type = "application/octet-stream";
                        asBinary = true;
                        break;
                    default:
                        type = null;
                        break;
                }

                if (asBinary) {
                    var item = memStore.GetItem<HashableBytes>(parsedHash);
                    if (item == null) {
                        throw new FileNotFoundException();
                    }
                    return Content(Utility.EncodeBytes(item.Value), type, Encoding.ASCII);
                } else {
                    var item = memStore.GetItem<HashableString>(parsedHash);
                    if (item == null) {
                        throw new FileNotFoundException();
                    }
                    return Content(item.Value, type, Encoding.ASCII);
                }

            }
        }

        [HttpGet("test1")]
        [Route("test1")]
        public void Test() {
            Response.StatusCode = 200;
            Response.ContentType = "text/plain";

            using (Response.Body) {
                using (var sw = new StreamWriter(Response.Body)) {
                    sw.Write("Hi there!");
                }
            }
        }

        [HttpGet("test2")]
        [Route("test2")]
        public FileStreamResult GetTest() {
            var stream = new MemoryStream(Encoding.ASCII.GetBytes("Hello World"));

            //return new FileStreamResult(stream, new MediaTypeHeaderValue("text/plain")) {
            return new FileStreamResult(stream, "text/plain") {
                FileDownloadName = "test.txt"
            };
        }

        public static MemoryStream GetTextStream(string Message) {
            return new MemoryStream(Encoding.ASCII.GetBytes(Message));
        }

        [HttpPost("store-string")]

        public object StoreString([FromForm] string value) {

            if (value == null) {
                throw new ArgumentException("Value was null");
            }

            var hs = new HashableString((string)value, provider);
            hs.ComputeHash(provider, null);
            memStore.StoreItem(hs);

            return Utility.EncodeBytes(hs.ComputedHash.Bytes, true, false);
        }

        //public HttpResponseMessage Post(string version, string environment, string filetype) {
        //    var path = @"C:\Temp\test.exe";
        //    HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
        //    var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        //    result.Content = new StreamContent(stream);
        //    result.Content.Headers.ContentType =
        //        new MediaTypeHeaderValue("application/octet-stream");
        //    return result;
        //}

        //[HttpPost("store")]
        //public async Task<IActionResult> Post(List<IFormFile> files) {
        //    long size = files.Sum(f => f.Length);

        //    // full path to file in temp location
        //    var filePath = Path.GetTempFileName();

        //    foreach (var formFile in files) {
        //        if (formFile.Length > 0) {
        //            using (var stream = new FileStream(filePath, FileMode.Create)) {
        //                await formFile.CopyToAsync(stream);
        //            }
        //        }
        //    }

        //    // process uploaded files
        //    // Don't rely on or trust the FileName property without validation.

        //    return Ok(new { count = files.Count, size, filePath });
        //}

        [HttpPost("store-binary")]
        public string StoreBinary() {

            //Span<byte> buffer = new Span<byte>();
            //List<byte> allBytes = new List<byte>();
            //int position = 0;
            //int readBytes = 0;
            //bool doneReading = false;
            //int length = Request.Body.Read(buffer);

            //while (doneReading == false){
            //  readBytes = Request.Body.Read(buffer);

            //if (readBytes == 0) {
            //    doneReading = true;
            //} else {
            //    allBytes.AddRange(buffer.Take(readBytes));
            //}

            //position += readBytes;
            //}

            var ms = new MemoryStream();
            Request.Body.CopyTo(ms);

            var hs = new HashableBytes(ms.ToArray(), provider);
            ms.Dispose();

            hs.ComputeHash(provider, null);
            memStore.StoreItem(hs);

            Response.StatusCode = 200;
            Response.ContentType = "text/plain";
            Response.Body = GetTextStream(hs.ComputedHash.ToString());
            Console.WriteLine(hs.ComputedHash.ToString());
            return Utility.EncodeBytes(hs.ComputedHash.Bytes, true, false);

            //return "ok";
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
