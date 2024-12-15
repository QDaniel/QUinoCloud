namespace QUinoCloud.Utils.Streams
{
    public abstract class WrapperStream(Stream stream) : Stream
    {
        protected Stream? _stream = stream;

        public override bool CanRead
        {
            get { return _stream?.CanRead ?? false; }
        }

        public override bool CanSeek
        {
            get { return _stream?.CanSeek ?? false; }
        }

        public override bool CanWrite
        {
            get { return _stream?.CanWrite ?? false; }
        }

        public override long Length
        {
            get { return _stream?.Length ?? throw new ObjectDisposedException(nameof(_stream)); }
        }

        public override long Position
        {
            get { return _stream?.Position ?? throw new ObjectDisposedException(nameof(_stream)); }
            set { if (_stream != null) _stream.Position = value; }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream?.Read(buffer, offset, count) ?? 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream?.Seek(offset, origin) ?? Position;
        }

        public override void SetLength(long value)
        {
            _stream?.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream?.Write(buffer, offset, count);
        }

        public override void Flush()
        {
            _stream?.Flush();
        }

        public override void Close()
        {
            _stream?.Close();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _stream?.Dispose();
            _stream = null;
        }
    }
}
