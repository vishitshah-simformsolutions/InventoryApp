using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;

namespace Demo.MedTech.Utility.Helper
{
    /// <summary>
    /// Used for compression decompression of string
    /// </summary>
    public class CompressHelper : ICompressHelper
    {
        /// <summary>Used for gzip compression of string</summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public async Task<byte[]> Compress(string plainText)
        {
            await using MemoryStream sourceMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(plainText));
            await using MemoryStream destinationMemoryStream = new MemoryStream();
            await using (GZipStream destination = new GZipStream((Stream)destinationMemoryStream, CompressionMode.Compress))
            {
                await sourceMemoryStream.CopyToAsync((Stream)destination);
                await destination.FlushAsync();
            }

            return destinationMemoryStream.ToArray();
        }

        /// <summary>Used for gzip decompression of bytes</summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public async Task<string> Decompress(byte[] base64EncodedData)
        {
            await using MemoryStream memoryStream = new MemoryStream(base64EncodedData);
            await using MemoryStream destination = new MemoryStream();
            await using (GZipStream gzipStream = new GZipStream((Stream)memoryStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync((Stream)destination);
                await gzipStream.FlushAsync();
            }

            return Encoding.UTF8.GetString(destination.ToArray());
        }
    }
}
