using System.Threading.Tasks;

namespace Demo.MedTech.Utility.Helper
{
    /// <summary>
    /// Used for compression decompression of string
    /// </summary>
    public interface ICompressHelper
    {
        /// <summary>Used for gzip compression of string</summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        Task<byte[]> Compress(string plainText);

        /// <summary>Used for gzip decompression of bytes</summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        Task<string> Decompress(byte[] base64EncodedData);
    }
}