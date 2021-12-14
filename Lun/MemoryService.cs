using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Linq;

namespace Lun
{
    public static class MemoryService
    {
        /// <summary>
        ///  Comprimi os bytes
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static byte[] Compress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData não pode ser nulo!");

            MemoryStream output = new MemoryStream();
            using (GZipStream dstream = new GZipStream(output, CompressionLevel.Optimal))
            {
                dstream.Write(inputData, 0, inputData.Length);
            }
            return output.ToArray();
        }

        /// <summary>
        /// Descomprimi
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public static byte[] Decompress(byte[] inputData)
        {
            if (inputData == null)
                throw new ArgumentNullException("inputData não pode ser nulo!");

            MemoryStream input = new MemoryStream(inputData);
            MemoryStream output = new MemoryStream();
            using (GZipStream dstream = new GZipStream(input, CompressionMode.Decompress))
            {
                dstream.CopyTo(output);
            }
            return output.ToArray();
        }

    }
}
