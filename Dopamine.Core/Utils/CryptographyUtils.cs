
using System.Text;

namespace Dopamine.Core.Utils
{
    public static class CryptographyUtils
    {
        public static string MD5Hash(string str)
        {
            string strHash = null;

            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] arrHash = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                var sb = new StringBuilder();
                for (int i = 0; i <= arrHash.Length - 1; i++)
                {
                    sb.Append(arrHash[i].ToString("x2"));
                }

                strHash = sb.ToString();
            }

            return strHash;
        }
    }
}
