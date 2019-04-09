using System;
using System.IO;

namespace SoftwarePioniere
{
    public static class StreamExtensions
    {

        public static byte[] CreateByteArray(this Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                var byts = ms.ToArray();
                return byts;
            }
        }

        public static Stream CreateStream(this byte[] byts)
        {
            if (byts == null)
            {
                throw new ArgumentNullException(nameof(byts));
            }

            return new MemoryStream(byts);
        }
    }
}
