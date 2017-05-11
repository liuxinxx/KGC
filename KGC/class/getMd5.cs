using System;
using System.Security.Cryptography;
using System.Text;
namespace KGC
{
    class getMd5
    {
        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="encryptString">要加密的消息</param>
        /// <returns>加密过的信息</returns>
        public string MD5(string encryptString)
        {

            byte[] result = Encoding.Default.GetBytes(encryptString);

            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] output = md5.ComputeHash(result);

            string encryptResult = BitConverter.ToString(output).Replace("-", "");

            return encryptResult;

        }
    }
}
