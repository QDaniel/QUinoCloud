using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace QUinoCloud.Utils
{
    public static class Crypto
    {

        #region Static HashAlgos
        public static HashAlgorithm AlgoMD5 => MD5.Create();
        public static HashAlgorithm AlgoSHA1 => SHA1.Create();
        public static HashAlgorithm AlgoSHA256 => SHA256.Create();

        #endregion

        /// <summary>
        /// Liefert den MD5 Hash 
        /// </summary>
        /// <param name="input">Eingabestring</param>
        /// <returns>MD5 Hash der Eingabestrings</returns>
        public static string GetMD5(string input)
        {
            return GetMD5(input, Encoding.UTF8);
        }
        public static string GetMD5(string input, Encoding encoding)
        {
            //Umwandlung des Eingastring in den MD5 Hash
            using (var algo = AlgoMD5)
            {
                return BuildHash(algo, input, encoding);
            }
        }

        public static byte[] GetMD5Bytes(string input, Encoding encoding)
        {
            //Umwandlung des Eingastring in den MD5 Hash
            using (var algo = AlgoMD5)
            {
                return BuildHashBytes(algo, input, encoding);
            }
        }


        public static string GetSHA256(string input)
        {
            return GetSHA256(input, Encoding.UTF8);
        }
        public static string GetSHA256(string input, Encoding encoding)
        {
            //Umwandlung des Eingastring in den MD5 Hash
            using (var algo = AlgoSHA256)
            {
                return BuildHash(algo, input, encoding);
            }
        }
        public static byte[] GetSHA256Bytes(string input, Encoding encoding)
        {
            //Umwandlung des Eingastring in den MD5 Hash
            using (var algo = AlgoSHA256)
            {
                return BuildHashBytes(algo, input, encoding);
            }
        }


        /// <summary>
        /// Liefert den SHA1 Hash 
        /// </summary>
        /// <param name="input">Eingabestring</param>
        /// <returns>SHA1 Hash der Eingabestrings</returns>
        public static string GetSHA1(string input)
        {
            //Umwandlung des Eingastring in den SHA1 Hash
            using (var hash = AlgoSHA1)
            {
                return BuildHash(hash, input, Encoding.UTF8);
            }
        }

        /// <summary>
        /// Liefert den SHA1 Hash 
        /// </summary>
        /// <param name="input">Eingabestring</param>
        /// <returns>SHA1 Hash der Eingabestrings</returns>
        public static byte[] GetSHA1Bytes(string input)
        {
            //Umwandlung des Eingastring in den SHA1 Hash
            using (var hash = AlgoSHA1)
            {
                return BuildHashBytes(hash, input, Encoding.UTF8);
            }
        }


        public static string BuildFileHash(HashAlgorithm algo, string filepath)
        {
            if (!File.Exists(filepath)) throw new FileNotFoundException("File not found", filepath);
            //            if (Files.IsInUse(filepath, FileShare.Read)) throw new FileLoadException("File already in use", filepath);

            using var fs = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Crypto.BuildHash(algo, fs);
        }


        /// <summary>
        /// Liefert den einen Hash anhand Provider und Daten
        /// </summary>
        /// <param name="algo">Hash Algorithmus</param>
        /// <param name="data">Data as string</param>
        /// <param name="encoding">String encoding</param>
        /// <returns>Hash</returns>
        public static string BuildHash(HashAlgorithm algo, string data, Encoding encoding)
        {
            byte[] bData = encoding.GetBytes(data);
            return ConvertHash(BuildHashBytes(algo, bData));
        }
        /// <summary>
        /// Liefert den einen Hash anhand Provider und Daten
        /// </summary>
        /// <param name="algo">Hash Algorithmus</param>
        /// <param name="data">Data as bytes</param>
        /// <returns>Hash</returns>
        public static string BuildHash(HashAlgorithm algo, byte[] data)
        {
            return ConvertHash(BuildHashBytes(algo, data));
        }
        /// <summary>
        /// Liefert den einen Hash anhand Provider und Daten
        /// </summary>
        /// <param name="algo">Hash Algorithmus</param>
        /// <param name="data">Data as bytes</param>
        /// <returns>Hash</returns>
        public static string BuildHash(HashAlgorithm algo, Stream data)
        {
            return ConvertHash(BuildHashBytes(algo, data));
        }

        /// <summary>
        /// Liefert den einen Hash anhand Provider und Daten
        /// </summary>
        /// <param name="algo">Hash Algorithmus</param>
        /// <param name="data">Data as string</param>
        /// <param name="encoding">String encoding</param>
        /// <returns>Hash</returns>
        public static byte[] BuildHashBytes(HashAlgorithm algo, string data, Encoding encoding)
        {
            byte[] bData = encoding.GetBytes(data);
            return algo.ComputeHash(bData);
        }


        /// <summary>
        /// Liefert den einen Hash anhand Provider und Daten
        /// </summary>
        /// <param name="algo">Hash Algorithmus</param>
        /// <param name="data">Data as stream</param>
        /// <returns>Hash</returns>
        public static byte[] BuildHashBytes(HashAlgorithm algo, byte[] data)
        {
            return algo.ComputeHash(data);
        }

        /// <summary>
        /// Liefert den einen Hash anhand Provider und Daten
        /// </summary>
        /// <param name="algo">Hash Algorithmus</param>
        /// <param name="data">Data as stream</param>
        /// <returns>Hash</returns>
        public static byte[] BuildHashBytes(HashAlgorithm algo, Stream data)
        {
            return algo.ComputeHash(data);
        }

        public static string ConvertHash(byte[] hash)
        {
            System.Text.StringBuilder s = new System.Text.StringBuilder();
            //Hash in Hex String konvertieren
            foreach (byte b in hash)
            {
                s.Append(b.ToString("x2").ToLower());
            }
            return s.ToString();
        }


        private static char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
        public static string GetUniqueKey(int maxSize)
        {
            return GetUniqueKey(maxSize, chars);
        }

        public static string GetUniqueKey(int maxSize, char[] chars)
        {
            byte[] data;
            using (var crypto = RandomNumberGenerator.Create())
            {
                data = new byte[maxSize];
                crypto.GetBytes(data);
            }
            var result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }
        public static async Task EncryptAsync(this Stream source, Stream destination, SymmetricAlgorithm cryptAlgo, byte[] key)
        {
            cryptAlgo.Key = key;
            byte[] iv = cryptAlgo.IV;
            destination.Write(iv, 0, iv.Length);

            using var cryptoStream = new CryptoStream(
                destination,
                cryptAlgo.CreateEncryptor(),
                CryptoStreamMode.Write);
            await source.CopyToAsync(cryptoStream).ConfigureAwait(false);
            /*
            var buffer = new byte[8192];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
            {
                destination.Write(buffer, 0, read);
                hashAlgo.TransformBlock(buffer, 0, read, buffer, 0);
            }
            hashAlgo.TransformFinalBlock(buffer, 0, 0);

            var hash = hashAlgo.Hash;
            destination.Write(hash, 0, hash.Length);
            */
        }
        public static async Task DecryptAsync(this Stream source, Stream destination, SymmetricAlgorithm cryptAlgo, byte[] key)
        {
            byte[] iv = new byte[cryptAlgo.IV.Length];
            var numBytesToRead = cryptAlgo.IV.Length;
            int numBytesRead = 0;
            while (numBytesToRead > 0)
            {
                int n = await source.ReadAsync(iv, numBytesRead, numBytesToRead).ConfigureAwait(false);
                if (n == 0) break;

                numBytesRead += n;
                numBytesToRead -= n;
            }
            cryptAlgo.Key = key;
            cryptAlgo.IV = iv;
            using var cryptoStream = new CryptoStream(
                source,
                cryptAlgo.CreateDecryptor(),
                CryptoStreamMode.Read);
            await cryptoStream.CopyToAsync(destination).ConfigureAwait(false);
        }
    }

}
