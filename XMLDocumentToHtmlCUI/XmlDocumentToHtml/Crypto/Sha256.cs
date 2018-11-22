using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace XmlDocumentToHtml.Crypto
{
    /// <summary>
    /// Sha256を計算するクラスを提供します。
    /// </summary>
    public static class Sha256
    {
        /// <summary>
        /// バイト配列からSHA256を計算します。
        /// </summary>
        /// <param name="bytes">計算する対象のバイト配列</param>
        /// <returns>変換されたSHA256の文字列</returns>
        public static string GetSha256(byte[] bytes)
        {
            var crypto256 = new SHA256CryptoServiceProvider();
            byte[] hash256Value = crypto256.ComputeHash(bytes);

            return BitConverter.ToString(hash256Value).Replace("-", String.Empty);
        }

        public static string GetSha256(string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            return GetSha256(data);
        }
    }
}
