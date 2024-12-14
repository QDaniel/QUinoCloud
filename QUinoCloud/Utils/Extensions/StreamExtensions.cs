using QUinoCloud.Utils.Streams;
using System.Security.Cryptography;
using System.Text;

namespace QUinoCloud.Utils.Extensions
{
    public static class StreamExtensions
    {

        public static byte[] ReadToBytes(this Stream stream) => ReadToBytes(stream, false);

        public static byte[] ReadToBytes(this Stream stream, bool closeStream)
        {
            if (stream == null) return [];
            try
            {

                if (stream is MemoryStream memstrm) return memstrm.ToArray();
                if (stream is TempStream tstrm) return tstrm.ToArray();

                long originalPosition = 0;

                if (stream.CanSeek)
                {
                    originalPosition = stream.Position;
                    stream.Seek(0, SeekOrigin.Begin);
                }
                try
                {
                    using var mems = new MemoryStream();
                    stream.CopyTo(mems);
                    return mems.ToArray();
                }
                finally
                {
                    if (!closeStream && stream.CanSeek)
                    {
                        stream.Seek(originalPosition, SeekOrigin.Begin);
                    }
                }
            }
            finally
            {
                if (closeStream) stream.Dispose();
            }
        }

        public static string? ReadToString(this Stream stream, Encoding enc)
        {
            if (stream == null) return null;
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Seek(0, SeekOrigin.Begin);
            }

            try
            {
                using var reader = new StreamReader(stream, enc, true, 2048, true);
                return reader.ReadToEnd();
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Seek(originalPosition, SeekOrigin.Begin);
                }
            }
        }

        public async static Task<string?> ReadToStringAsync(this Stream stream, Encoding enc)
        {
            if (stream == null) return null;
            long originalPosition = 0;
            if (stream.CanSeek)
            {
                originalPosition = stream.Position;
                stream.Seek(0, SeekOrigin.Begin);
            }

            try
            {
                using var reader = new StreamReader(stream, enc, true, 2048, true);
                return await reader.ReadToEndAsync();
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Seek(originalPosition, SeekOrigin.Begin);
                }
            }
        }
        public static void SaveTo(this Stream stream, string destPath)
        {
            using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write);
            long originalPosition = 0;
            try
            {
                if (stream.CanSeek)
                {
                    originalPosition = stream.Position;
                    stream.Seek(0, SeekOrigin.Begin);
                }
                stream.CopyTo(fileStream);
            }
            finally
            {
                if (stream.CanSeek)
                {
                    stream.Seek(originalPosition, SeekOrigin.Begin);
                }
            }
        }

        /// <summary>
        /// Set the Stream to Position 0
        /// </summary>
        /// <param name="strm"></param>
        /// <returns></returns>
        public static bool Reset(this Stream strm)
        {
            if (strm != null && strm.CanSeek && strm.Position != 0)
            {
                strm.Seek(0, SeekOrigin.Begin);
                return true;
            }
            return false;
        }

        public static void Write(this Stream strm, string data, Encoding enc)
        {
            var bData = enc.GetBytes(data);
            strm.Write(bData, 0, bData.Length);
        }
        public static void CopyTo(this Stream input, Stream output, int bufferSize, ulong maxBytes, IProgress<long>? progress)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            long readpos=0;
            while (maxBytes > 0 &&
                   (read = input.Read(buffer, 0, (int)Math.Min((ulong)buffer.Length, maxBytes))) > 0)
            {
                output.Write(buffer, 0, read);
                maxBytes -= (ulong)read;
                readpos += read;
                progress?.Report(readpos);
            }
            output.Reset();
        }
        public static void CopyTo(this Stream input, Stream output, int bufferSize, ulong maxBytes)
        {
            CopyTo(input, output, bufferSize, maxBytes, null);
        }


        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize = 81920, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (!source.CanRead)
                throw new ArgumentException("Has to be readable", nameof(source));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (!destination.CanWrite)
                throw new ArgumentException("Has to be writable", nameof(destination));
            if (bufferSize < 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
            destination.Reset();
        }

        public static void ConvertToBase64(this Stream input, Stream output)
        {
            using var cs = new CryptoStream(output, new ToBase64Transform(), CryptoStreamMode.Write);
            input.CopyTo(output);
            output.Reset();
        }

        public static Stream ToTempStream(this Stream input)
        {
            if (input is TempStream tsq) return tsq;
            using (input)
            {
                Stream ts = new TempStream();
                input.CopyTo(ts);
                ts.Reset();
                return ts;
            }
        }

        public static Task<Stream> ToTempStreamAsync(this Task<Stream> taskS)
        {
            return taskS.ContinueWith(o =>
            {
                if (o.Result is TempStream tsq) return tsq;
                using var str = o.Result;
                Stream ts = new TempStream();
                str.CopyTo(ts);
                ts.Reset();
                return ts;
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}
