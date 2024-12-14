using System.Text;

namespace QUinoCloud.Utils
{
    public class Password
    {
        const string AlphaNumChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string NonAlphaNumChars = "!?&@";
        public static string CreatePassword(int length)
        {
            var rnd = new Random();
            var res = new StringBuilder();
            while (1 < length--)
            {
                res.Append(AlphaNumChars[rnd.Next(AlphaNumChars.Length)]);
            }
            res.Append(NonAlphaNumChars[rnd.Next(NonAlphaNumChars.Length)]);
            return res.ToString();
        }
    }
}
