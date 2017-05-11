using System;
using System.Text;
using System.Windows.Forms;
using Com.Itrus.Crypto;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;
using System.IO;
namespace KGC
{
    class ecc
    {
        #region 生成公私钥，并存入文件夹
        /// <summary>
        /// 生成公私钥，并存入文件夹
        /// </summary>
        /// <param name = "pripk">私钥文件地址</param>
        /// <param name="ppk">公钥文件地址</param>
        /// <param name="sm2">传入的SM2实例</param>

        public void Creatkey(SM2 sm2, string pripk, string ppk)
        {

            AsymmetricCipherKeyPair keypair = null;
            for (int i = 0; i < 1; i++)
                keypair = sm2.ecc_key_pair_generator.GenerateKeyPair();

            ECPrivateKeyParameters ecpriv = (ECPrivateKeyParameters)keypair.Private;
            ECPublicKeyParameters ecpub = (ECPublicKeyParameters)keypair.Public;

            BigInteger userD = ecpriv.D;
            ECPoint Key = ecpub.Q;

            byte[] userKeybyte = Key.GetEncoded();
            System.String userKey = new UTF8Encoding().GetString(Hex.Encode(userKeybyte));

            string avg = Console.ReadLine();

            WriterKey(userD, pripk);//私钥
            WriterKey(userKey, ppk);//公钥        

        }
        #endregion

        #region 写入公私钥
        /// <summary>
        /// 经过des加密后、写入秘钥（用户名pri.txt）
        /// </summary>
        /// <param name="userD">秘钥</param>
        /// <param name="file">要写入的文件名</param>
        public void WriterKey(BigInteger userD, string file)
        {
            try
            {
                string path = file;
                FileStream Ali;
                file fi = new file();
                desKey des = new desKey();

                Ali = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件
                StreamWriter sr = new StreamWriter(Ali);
                string ss = des.Encrypt(userD.ToString(16), "abcdefgh");//des加密，"abcdefgh"为加密随机数

                sr.WriteLine(ss);//写入文件         


                sr.Close();
            }
            catch (Exception ee)
            {

                MessageBox.Show("ecc类异常 1\r\n" + ee.ToString());
            }
        }

        /// <summary>
        /// 经过des加密后、写入秘钥（用户名pri.txt）
        /// </summary>
        /// <param name="userKey">公钥</param>
        /// <param name="file">要写入的文件名</param>
        public void WriterKey(string userKey, string file)
        {
            try
            {
                string path = file;
                FileStream Ali;
                desKey des = new desKey();
                Ali = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件
                StreamWriter sr = new StreamWriter(Ali);
                sr.WriteLine(des.Encrypt(userKey, "abcdefgh"));
                // MessageBox.Show("\r\n公钥长度：" + userKey.Length);
                sr.Close();
            }
            catch (Exception ee)
            {

                MessageBox.Show("ecc类异常 2\r\n" + ee.ToString());
            }
        }
        #endregion

        #region 读私钥操作
        /// <summary>
        /// 读取私钥(进过des加密)
        /// </summary>
        /// <param name="prikey">传出大数类型的私钥</param>
        /// <param name="file">文件名</param>
        public void Readprikey(out BigInteger prikey, string file)
        {
            string[] str = File.ReadAllLines(file, Encoding.Default);
            desKey des = new desKey();
            prikey = new BigInteger(des.Decrypt(str[0], "abcdefgh"), 16);

        }
        #endregion

        #region 读公钥操作
        /// <summary>
        /// 读取公钥(进过des加密)
        /// </summary>
        /// <param name="key">传出字节类型的公钥</param>
        /// <param name="file">要读的文件名</param>
        public void ReadpublicKey(out byte[] key, string file)
        {
            string[] str = File.ReadAllLines(file, Encoding.Default);
            desKey des = new desKey();
            key = strToToHexByte(des.Decrypt(str[0], "abcdefgh"));
        }
        #endregion

        #region 16进制转字节
        /// <summary>
        /// 16进制转字节
        /// </summary>
        /// <param name="hexString">16进制的字符串</param>
        /// <returns></returns>
        private static byte[] strToToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        #endregion

        #region 字节数组转16进制字符串
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("x2");
                }
            }
            return returnStr;
        }
        #endregion

        #region 数字签名算法
        /// <summary>
        /// 数字签名算法
        /// 利用自己的公私钥生成rs并存入文件
        /// </summary>
        /// <param name="sm2">sm2对象</param>
        /// <param name="pripk">自己的私钥文件夹</param>
        /// <param name="ppk">自己的公钥文件路径</param>
        /// <param name="ida">用户名</param>
        public string Test_sm2_sign(SM2 sm2, string pripk, string ppk, string ida)
        {
            BigInteger test_d = null;
            ECPoint test_p = null;
            byte[] key = null;

            //读取私钥
            Readprikey(out test_d, pripk);
            //读取公钥
            ReadpublicKey(out key, ppk);
            test_p = sm2.ecc_curve.DecodePoint(key);

            Com.Itrus.Crypto.SM2.SM2Result sm2Ret = new Com.Itrus.Crypto.SM2.SM2Result();//实例化一个SM2Result的对象sm2Ret
            SM3Digest sm3 = new SM3Digest();

            byte[] z = sm2.Sm2GetZ(Encoding.Default.GetBytes(ida), test_p);//调用Sm2GetZ方法求a的Z的字节数组

            sm3.BlockUpdate(z, 0, z.Length);

            byte[] md = new byte[32];
            sm3.DoFinal(md, 0);
            sm2.Sm2Sign(md, test_d, test_p, sm2Ret);//生成rs           
            Writers(sm2Ret.r, sm2Ret.s, ida + "rs" + ".txt");//写入rs文件

            return byteToHexStr(z);

        }
        #endregion

        #region 写入r,s
        /// <summary>
        /// 将rs写入一个文件下（不经过des加密）
        /// </summary>
        /// <param name="r">R</param>
        /// <param name="s">S</param>
        /// <param name="rsfile">rs要存入的文件夹路径</param>
        public void Writers(BigInteger r, BigInteger s, string rsfile)//写入r,s,签名者公钥，用于验证
        {
            try
            {
                FileStream Ali;
                string path = rsfile;
                desKey des = new desKey();
                Ali = new FileStream(path, FileMode.Create, FileAccess.Write);//创建写入文件
                StreamWriter sr = new StreamWriter(Ali);

                //RS         
                sr.Write(r.ToString(16));//开始写入值 
                sr.Write("#");
                sr.Write(s.ToString(16));
                sr.Close();
            }
            catch (Exception ss)
            {
                MessageBox.Show("写入RS时出错！\r\n" + ss.ToString());

                throw;
            }
        }
        #endregion

        #region 签名验证算法
        /// <summary>
        /// 签名验证算法
        /// 利用签名者的公钥和传过来的r，s来验证签名是否合法
        /// </summary>
        /// <param name="sm2">sm2对象</param>
        /// <param name="ppk">签名者的公钥16进制字符串</param>
        /// <param name="Z">签名算法产生的16进制字符串 Z</param>
        /// <param name="r">签名算法生成的 R</param>
        /// <param name="s">签名算法生成的 S</param>
        /// <returns></returns>
        public bool Signature_Check(SM2 sm2, string ppk, string Z, string r, string s)
        {
            ECPoint test_p = null;
            //test_p = sm2.userKey;
            //MessageBox.Show(ppk);
            byte[] key = strToToHexByte(ppk);

            test_p = sm2.ecc_curve.DecodePoint(key);
            Com.Itrus.Crypto.SM2.SM2Result sm2Ret = new Com.Itrus.Crypto.SM2.SM2Result();//实例化一个SM2Result的对象sm2Ret
            SM3Digest sm3 = new SM3Digest();

            byte[] z = strToToHexByte(Z);
            sm3.BlockUpdate(z, 0, z.Length);
            byte[] md = new byte[32];
            sm3.DoFinal(md, 0);

            sm2Ret.r = new BigInteger(r, 16);
            sm2Ret.s = new BigInteger(s, 16);
            sm2.Sm2Verify(md, test_p, sm2Ret.r, sm2Ret.s, sm2Ret);//调用Sm2Verify方法，得到R

            if (sm2Ret.r.Equals(sm2Ret.R))//如果r==R
            {
                return true;  //System.Console.Out.WriteLine("\n签名结果验证通过！r == R\n");
            }
            else//r!=R
            {
                return false;//System.Console.Out.WriteLine("\n签名结果验证失败！r != R\n");
            }

        }
        #endregion

        #region 公钥加密算法
        /// <summary>
        /// 公钥加密算法，利用下一个用户的公钥进行加密
        /// </summary>
        /// <param name="sm2">sm2对象</param>
        /// <param name="msg">要加密的消息</param>
        /// <param name="ppk">目的签名者的公钥文件夹路径</param>
        /// <param name="id">加密后消息存入的文件夹（最好不存起来）</param>
        public string[] Test_sm2_cipher(SM2 sm2, string msg, string ppk)
        {
            byte[] data = Encoding.Default.GetBytes(msg);//将信息转化为比特

            // 加密过程
            ECPoint userKey = null;
            byte[] key = null;

            ReadpublicKey(out key, ppk);//读取解密者的公钥
            userKey = sm2.ecc_curve.DecodePoint(key);//把字节形的转化为Ecpoint
            System.String sdata = new UTF8Encoding().GetString(Hex.Encode(data));

            SM2.Cipher cipher = new SM2.Cipher();
            ECPoint c1 = cipher.Init_enc(sm2, userKey);//调用Init_enc方法

            byte[] bc1 = c1.GetEncoded();//将c1的数据类型转换成比特串
            System.String sbc1 = new UTF8Encoding().GetString(Hex.Encode(bc1));

            cipher.Encrypt(data);

            System.String sc2 = new UTF8Encoding().GetString(Hex.Encode(data));
            byte[] c3 = new byte[32];
            cipher.Dofinal(c3);
            System.String sc3 = new UTF8Encoding().GetString(Hex.Encode(c3));

            string[] cc = { sbc1, sc2, sc3 };
            return cc;
           
        }
        #endregion

        #region 私钥解密算法
        /// <summary>
        /// 私钥解密算法
        /// </summary>
        /// <param name="sm2">sm2对象</param>
        /// <param name="pripk">自己的私钥</param>
        /// <param name="id">id</param>
        /// <returns></returns>
        public string deciphering(SM2 sm2, string pripk, string mc1, string sc2, string mc3)
        {


            byte[] bc1 = new byte[32];
            BigInteger userD = null;
            // String sc2 = null;
            byte[] c3 = new byte[32];
            byte[] data;
            bc1 = strToToHexByte(mc1);
            c3 = strToToHexByte(mc3);
            Readprikey(out userD, pripk);
            // 解密过程
            SM2.Cipher cipher = new SM2.Cipher();
            cipher = new SM2.Cipher();

            ECPoint c1 = sm2.ecc_curve.DecodePoint(bc1);
            data = strToToHexByte(sc2);

            cipher.Init_dec(userD, c1);//调用Init_dec,从c中取出比特串c1,将c1的数据类型转化为椭圆曲线上的点，如果不满足椭圆曲线上的点则报错。
            cipher.Decrypt(data);//调用Decrypt方法
            //System.String sdata1 = new UTF8Encoding().GetString(Hex.Encode(data));

            string sdata = System.Text.Encoding.Default.GetString(data);
            System.String sc3 = new UTF8Encoding().GetString(Hex.Encode(c3));

            byte[] c3_ = new byte[32];
            cipher.Dofinal(c3_);
            System.String sc3_ = new UTF8Encoding().GetString(Hex.Encode(c3_));
            //数据校验检测数据是否被篡改或丢失
            if (sc3_.ToUpper().Equals(sc3.ToUpper()))//sc3_==sc3
            {

                return sdata;
            }
            else
            {
                // System.Console.Out.WriteLine("数据校验失败!\n");//sc3_!=sc3
                return "0";
            }

        }
        #endregion

    }
}
