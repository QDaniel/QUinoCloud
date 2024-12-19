namespace QUinoCloud.Utils
{
    public class Files
    {
        public static string SanitizeFilename(string filename)
        {
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            filename = filename.Replace("___", "_").Replace("__", "_");
            if (filename.Length > 200)
            {
                var ext = Path.GetExtension(filename);
                filename = filename.Substring(0, 200 - ext.Length) + ext;
            }
            return filename;
        }

    }
}
