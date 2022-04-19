using Product.Utility.Helper;
using Microsoft.Extensions.DependencyInjection;

namespace Product.Utility.Extension
{
    /// <summary>
    /// Extension used for gzip compression decompression of string
    /// </summary>
    public static class RegisterCompressionDecompressionExtension
    {
        /// <summary>
        /// Register used for gzip compression decompression of string
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection RegisterGzipCompressorDeCompressor(this IServiceCollection services)
        {
            services.AddSingleton<ICompressHelper, CompressHelper>();
            return services;
        }
    }
}
