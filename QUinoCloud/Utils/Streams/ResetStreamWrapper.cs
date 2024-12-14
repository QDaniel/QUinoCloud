namespace System.IO
{
    public class ResetStreamWrapper : NoCloseStreamWrapper
    {
        public ResetStreamWrapper(Stream stream) : base(stream)
        {
            if (CanSeek) _stream?.Seek(0, SeekOrigin.Begin);
        }

        public override void Close()
        {
            if (CanSeek) _stream?.Seek(0, SeekOrigin.Begin);
        }
    }
}
