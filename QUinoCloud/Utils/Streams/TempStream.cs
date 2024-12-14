using QUinoCloud.Utils.Extensions;
using System.Text;

namespace QUinoCloud.Utils.Streams
{
    public class TempStream : Stream
    {
        //public static int DefaultMaxMemoryLength = 40960;
        public static int DefaultMaxMemoryLength = 1024000;

        private Stream parent;

        public TempStream() : this(DefaultMaxMemoryLength)
        {
        }
        public TempStream(int maxMemoryLength)
        {
            parent = new MemoryStream();
            UseMemory = true;
            MaxMemoryLength = maxMemoryLength;
        }

        public int MaxMemoryLength { get; set; }
        public bool UseMemory { get; private set; }

        public override bool CanRead { get { return parent.CanRead; } }

        public override bool CanSeek { get { return parent.CanSeek; } }

        public override bool CanWrite { get { return parent.CanWrite; } }

        public override long Length { get { return parent.Length; } }

        public override long Position
        {
            get
            {
                return parent.Position;
            }

            set
            {
                parent.Position = value;
            }
        }

        public override void Flush()
        {
            parent.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return parent.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return parent.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            parent.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            parent.Write(buffer, offset, count);
            if (UseMemory && Length > MaxMemoryLength)
            {
                var mem = parent;
                var pos = Position;
                try
                {
                    var strm = new TempFileStream();
                    mem.Flush();
                    mem.Seek(0, SeekOrigin.Begin);
                    mem.CopyTo(strm);
                    if (strm.Position != pos) strm.Seek(pos, SeekOrigin.Begin);
                    parent = strm;
                    UseMemory = false;
                    mem.Close();
                    mem.Dispose();
                }
                catch (Exception)
                {
                    mem.Seek(pos, SeekOrigin.Begin);
                    MaxMemoryLength = MaxMemoryLength * 2;
                }
            }
        }

        public override void Close()
        {
            parent.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) parent.Dispose();
            base.Dispose(disposing);
        }

        public byte[] ToArray()
        {
            return parent.ReadToBytes();
        }

        public static Stream CreateFromBytes(byte[] data)
        {
            var tmp = new TempStream();
            using (var strm = new MemoryStream(data))
                strm.CopyTo(tmp);
            tmp.Seek(0, SeekOrigin.Begin);
            return tmp;
        }
        public static Stream CreateFromText(string text, Encoding encoding)
        {
            return CreateFromBytes(encoding.GetBytes(text));
        }

    }

}
