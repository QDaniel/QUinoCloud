
using QUinoCloud.Utils.Streams;

namespace System.IO
{
    public class NoCloseStreamWrapper(Stream stream) : WrapperStream(stream)
    {
        public override void Close()
        {
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
