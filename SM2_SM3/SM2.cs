 /*
 *  ecpoint�������������
 */
using System;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Math.EC;

using Org.BouncyCastle.Math;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Text;

namespace Com.Itrus.Crypto
{
    public class SM2
    {
        #region ʹ�ñ�׼����
        public static SM2 Instance//���ش�
        {
            get
            {
                return new SM2(false);
            }

        }
        #endregion

        #region ʹ�ò��Բ���
        //public static SM2 InstanceTest //���ض�
        //{
        //    get
        //    {
        //        return new SM2(true);
        //    }

        //}
        #endregion
        public bool sm2Test = false;//��ʼ����Ϊ��

        public string[] ecc_param;// = sm2_test_param;
        public readonly BigInteger ecc_p;
        public readonly BigInteger ecc_a;
        public readonly BigInteger ecc_b;
        public readonly BigInteger ecc_n;
        public readonly BigInteger ecc_gx;
        public readonly BigInteger ecc_gy;

        public readonly ECCurve ecc_curve;//��Բ���ߵĲ����ֶ�
        public readonly ECPoint ecc_point_g;//g��������ֶ�

        public readonly ECDomainParameters ecc_bc_spec;
        public readonly ECKeyPairGenerator ecc_key_pair_generator;
        public ECPoint userKey;
        public BigInteger userD;
        #region ecc����
        private SM2(bool sm2Test)
        {
            this.sm2Test = sm2Test;

            //if (sm2Test)//���Ϊ��
            //    ecc_param = sm2_test_param;//ʹ�ù����������ָ��Ĳ��Բ���
            //else
                ecc_param = sm2_param;//����ʹ�ù��ܱ�׼256λ���߲���
            ECFieldElement ecc_gx_fieldelement;
            ECFieldElement ecc_gy_fieldelement;
            ecc_p = new BigInteger(ecc_param[0], 16);
            ecc_a = new BigInteger(ecc_param[1], 16);
            ecc_b = new BigInteger(ecc_param[2], 16);
            ecc_n = new BigInteger(ecc_param[3], 16);
            ecc_gx = new BigInteger(ecc_param[4], 16);
            ecc_gy = new BigInteger(ecc_param[5], 16);
            ecc_gx_fieldelement = new FpFieldElement(ecc_p, ecc_gx);//ѡ����Բ�����ϻ���G��x����
            ecc_gy_fieldelement = new FpFieldElement(ecc_p, ecc_gy); //ѡ����Բ�����ϻ���G������
            ecc_curve = new FpCurve(ecc_p, ecc_a, ecc_b);//������Բ����
            ecc_point_g = new FpPoint(ecc_curve, ecc_gx_fieldelement, ecc_gy_fieldelement);//���ɻ���G
            ecc_bc_spec = new ECDomainParameters(ecc_curve, ecc_point_g, ecc_n);//��Բ���ߣ�g�����꣬��n.
            ECKeyGenerationParameters ecc_ecgenparam;
            ecc_ecgenparam = new ECKeyGenerationParameters(ecc_bc_spec, new SecureRandom());
            ecc_key_pair_generator = new ECKeyPairGenerator();
            ecc_key_pair_generator.Init(ecc_ecgenparam);
        }
        #endregion

        #region ����Zֵ�ķ���
        /*M2ǩ��ͬ��Ҳ����Ҫ��ժҪԭ�����ݣ�����ʹ��SM3�����Ӵ��㷨�����32byteժҪ��SM3��ҪժҪǩ����ID��Ĭ��1234567812345678����
         * ���߲���a,b,Gx,Gy����Կ����(x,y)�����Zֵ��Ȼ�����Ӵ�ԭ�ĵó�ժҪ���ݡ�����ط�Ҫע�����߲���������㶼��32byte��
         * ��ת��ΪBigInteger��������ת���ֽ���ʱҪȥ���ղ�λ��������ܻ����ժҪ���㲻��ȷ�����⣺*/
        /// <summary>
        /// ����Zֵ
        /// </summary>
        /// <param name="userId">ǩ����ID</param>
        /// <param name="userKey">���ߵĸ�������</param>
        /// <returns></returns>
        public virtual byte[] Sm2GetZ(byte[] userId, ECPoint userKey)
        {
            SM3Digest sm3 = new SM3Digest();
            byte[] p;
            // userId length
            int len = userId.Length * 8;//��userId�ĳ���
            sm3.Update((byte)(len >> 8 & 0x00ff));
            sm3.Update((byte)(len & 0x00ff));

            // userId
            sm3.BlockUpdate(userId, 0, userId.Length);

            // a,b
            p = ecc_a.ToByteArray();
            sm3.BlockUpdate(p, 0, p.Length);
            p = ecc_b.ToByteArray();
            sm3.BlockUpdate(p, 0, p.Length);
            // gx,gy
            p = ecc_gx.ToByteArray();
            sm3.BlockUpdate(p, 0, p.Length);
            p = ecc_gy.ToByteArray();
            sm3.BlockUpdate(p, 0, p.Length);

            // x,y
            p = userKey.X.ToBigInteger().ToByteArray();
            sm3.BlockUpdate(p, 0, p.Length);
            p = userKey.Y.ToBigInteger().ToByteArray();
            sm3.BlockUpdate(p, 0, p.Length);

            // Z
            byte[] md = new byte[sm3.GetDigestSize()];
            sm3.DoFinal(md, 0);

            return md;
        }
        #endregion

        #region ����ǩ��������s,r;
        /*
         * SM2�㷨�ǻ���ECC�㷨�ģ�ǩ��ͬ������2����������64byte������ԭ��RSA�㷨�Ѻ��ձ�֧�֣�
         * Ҫʵ��RSA��ǩ����ǩ���б�׼���ʵ�֣���SM2�ǹ����㷨�ڹ����ϻ�û�б�׼ͨ�ã��㷨Oid��ʶ��X509��׼����û����ġ�
         * ��.Net��Java�п��Ի���ʹ��BouncyCastle���ܿ�ʵ�֣���Դ��Ҳ�ȽϺ�ѧϰ��չ��SM2�㷨��ǩ����ʹ������ǩ��
         * �����Բ���Ҫʹ��Ӳ���豸��ͬ��ʹ��ԭʼ���ݡ�ǩ����֤��(��Կ)��ʵ�ֶ�ǩ������֤����֤����������δ���۸ġ�
         * ��֤����ͬ������ժҪԭ�����ݣ���Կ��֤��������һ��66byte��BitString��ȥ��ǰ����λ��64byteΪ��Կ����(x,y)��
         * �м�ָ��ȡ����Hex��ʽת��BigInteger�������㣬��ǩ�������£�
         */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="md">��Ϣ</param>
        /// <param name="userD">��Կ</param>
        /// <param name="userKey">��Կ</param>
        /// <param name="sm2Ret">sm2Ret����</param>
        public virtual void Sm2Sign(byte[] md, BigInteger userD, ECPoint userKey, SM2Result sm2Ret)
        {
            // e
            BigInteger e = new BigInteger(1, md);//�ֽ�ת��������
            // k
            BigInteger k = null;//��ʼ�������kΪ��
            ECPoint kp = null;//����kp��Ϊ��
            BigInteger r = null;//�������rΪ�գ�������õ�rֵ
            BigInteger s = null;//�������rΪ�գ�������õ�sֵ

            do
            {
                do
                {
                    if (!sm2Test)//���������k
                    {
                        AsymmetricCipherKeyPair keypair = ecc_key_pair_generator.GenerateKeyPair();
                        ECPrivateKeyParameters ecpriv = (ECPrivateKeyParameters)keypair.Private;//����˽Կ
                        ECPublicKeyParameters ecpub = (ECPublicKeyParameters)keypair.Public;//������Կ
                        k = ecpriv.D;//����������k
                        kp = ecpub.Q;//kp=����Ԫ
                    }
                    else//��������������ֶ����
                    {
                        string kS = "6CB28D99385C175C94F94E934817663FC176D925DD72B727260DBAAE1FB2F96F6CB28D99385C175C94F94E9348176240B";
                        k = new BigInteger(kS, 16);
                        kp = ecc_point_g.Multiply(k);
                    }

                    // r
                    r = e.Add(kp.X.ToBigInteger());//r=e+kp������X
                    r = r.Mod(ecc_n);//��r����ģn���㣬��ֹԽ��
                }
                while (r.Equals(BigInteger.Zero) || r.Add(k).Equals(ecc_n));//r==����0��r==nʱ����ѭ��

                // (1 + dA)~-1
                BigInteger da_1 = userD.Add(BigInteger.One);//da_1=��Կ+1;
                da_1 = da_1.ModInverse(ecc_n);//��da_1��������
                // s
                s = r.Multiply(userD);//s=r*��Կ
                s = k.Subtract(s).Mod(ecc_n);//s=((k-s)%n);
                s = da_1.Multiply(s).Mod(ecc_n);//s=((da_1*s)%n)
            }
            while (s.Equals(BigInteger.Zero));//s==0��ʱ������ѭ��

            sm2Ret.r = r;
            sm2Ret.s = s;
        }
        #endregion

        #region ��֤
        /// <summary>
        /// 
        /// </summary>
        /// <param name="md">��Ϣ</param>
        /// <param name="userKey">��Կ</param>
        /// <param name="r">������ǩ���õ��Ĵ���r</param>
        /// <param name="s">������ǩ���õ��Ĵ���s</param>
        /// <param name="sm2Ret"></param>
        public virtual void Sm2Verify(byte[] md, ECPoint userKey, BigInteger r, BigInteger s, SM2Result sm2Ret)//�ͻ�����֤
        {
            sm2Ret.R = null;

            // e_
            BigInteger e = new BigInteger(1, md);//�ֽ�ת��������e
            // t
            BigInteger t = r.Add(s).Mod(ecc_n);//����t=(r+s)%n;

            if (t.Equals(BigInteger.Zero))//���t==0��������һ��
                return;

            // x1y1
            ECPoint x1y1 = ecc_point_g.Multiply(sm2Ret.s);//x1y1=g*s
            x1y1 = x1y1.Add(userKey.Multiply(t));//x1y1=x1y1+��Կ*(t),����t=(r+s)%n

            // R
            sm2Ret.R = e.Add(x1y1.X.ToBigInteger()).Mod(ecc_n);//r=(x1y1���X�Ĵ�����ʽ+e)%n
        }
        #endregion

        public class SM2Result
        {
            public SM2Result()
            {
            }
            // ǩ������ǩ
            public BigInteger r;
            public BigInteger s;
            public BigInteger R;
        }
        #region �����������ָ��Ĳ��Բ���
        //public static readonly string[] sm2_test_param = {
        //    "8542D69E4C044F18E8B92435BF6FF7DE457283915C45517D722EDB8B08F1DFC3",// p,0
        //    "787968B4FA32C3FD2417842E73BBFEFF2F3C848B6831D7E0EC65228B3937E498",// a,1
        //    "63E4C6D3B23B0C849CF84241484BFE48F61D59A5B16BA06E6E12D1DA27C5249A",// b,2
        //    "8542D69E4C044F18E8B92435BF6FF7DD297720630485628D5AE74EE7C32E79B7",// n,3
        //    "421DEBD61B62EAB6746434EBC3CC315E32220B3BADD50BDC4C4E6C147FEDD43D",// gx,4
        //    "0680512BCBB42C07D47349D2153B70C4E5D7FDFCBFA36EA1A85841B9E46E09A2" // gy,5
        //};
        #endregion

        #region ���ܱ�׼256λ���߲���
        public static readonly string[] sm2_param = {
			"FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFF",// p,0
			"FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000FFFFFFFFFFFFFFFC",// a,1
			"28E9FA9E9D9F5E344D5A9E4BCF6509A7F39789F515AB8F92DDBCBD414D940E93",// b,2
			"FFFFFFFEFFFFFFFFFFFFFFFFFFFFFFFF7203DF6B21C6052B53BBF40939D54123",// n,3
			"32C4AE2C1F1981195F9904466A39C9948FE30BBFF2660BE1715A4589334C74C7",// gx,4
			"BC3736A2F4F6779C59BDCEE36B692153D0A9877CC62A474002DF32E52139F0A0" // gy,5
	    };
        #endregion

        #region �����㷨��
        public class Cipher
        {
            private int ct = 1;

            private ECPoint p2;
            private SM3Digest sm3keybase;
            private SM3Digest sm3c3;

            private byte[] key = new byte[32];
            private byte keyOff = 0;

            public Cipher()
            {
            }

            private void Reset()
            {
                sm3keybase = new SM3Digest();//ʵ����һ��SM3Digest�Ķ���sm3keybase
                sm3c3 = new SM3Digest();//ʵ����һ��SM3Digest�Ķ���sm3c3

                byte[] p;

                p = p2.X.ToBigInteger().ToByteArray();//��������ת��Ϊ���ش���
                sm3keybase.BlockUpdate(p, 0, p.Length);//���������Ӵ�BlockUpdate����
                sm3c3.BlockUpdate(p, 0, p.Length);//���������Ӵ�BlockUpdate����

                p = p2.Y.ToBigInteger().ToByteArray();//��������ת��Ϊ���ش�
                sm3keybase.BlockUpdate(p, 0, p.Length);//���������Ӵ�BlockUpdate����

                ct = 1;
                NextKey();//����NextKey����
            }

            private void NextKey()
            {
                SM3Digest sm3keycur = new SM3Digest(sm3keybase);
                sm3keycur.Update((byte)(ct >> 24 & 0x00ff));//���������Ӵ�Update����
                sm3keycur.Update((byte)(ct >> 16 & 0x00ff));//���������Ӵ�Update����
                sm3keycur.Update((byte)(ct >> 8 & 0x00ff));//���������Ӵ�Update����
                sm3keycur.Update((byte)(ct & 0x00ff));
                sm3keycur.DoFinal(key, 0);//���������Ӵ�DoFinal����
                keyOff = 0;
                ct++;
            }

            public virtual ECPoint Init_enc(SM2 sm2, ECPoint userKey)
            {
                BigInteger k = null;
                ECPoint c1 = null;
                if (!sm2.sm2Test)//�ж�ʹ�����ַ���
                {
                    AsymmetricCipherKeyPair key = sm2.ecc_key_pair_generator.GenerateKeyPair();
                    ECPrivateKeyParameters ecpriv = (ECPrivateKeyParameters)key.Private;//����˽Կ
                    ECPublicKeyParameters ecpub = (ECPublicKeyParameters)key.Public;//���ɹ�Կ
                    k = ecpriv.D;//k
                    c1 = ecpub.Q;//������Բ��c1
                }
                else//ʹ�ò��Բ���
                {
                    k = new BigInteger("4C62EEFD6ECFC2B95B92FD6C3D9575148AFA17425546D49018E5388D49DD7B4F", 16);//ָ��k
                    c1 = sm2.ecc_point_g.Multiply(k);//��ȡ��Կ
                }

                p2 = userKey.Multiply(k);
                Reset();//���������Ӵ�Reset����

                return c1;//�ѹ�Կ���ظ���������ʽ��.
            }

            public virtual void Encrypt(byte[] data)
            {
                sm3c3.BlockUpdate(data, 0, data.Length);
                for (int i = 0; i < data.Length; i++)
                {
                    if (keyOff == key.Length)
                        NextKey();

                    data[i] ^= key[keyOff++];
                }
            }

            public virtual void Init_dec(BigInteger userD, ECPoint c1)
            {
                p2 = c1.Multiply(userD);
                Reset();//����Reset����
            }

            public virtual void Decrypt(byte[] data)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (keyOff == key.Length)
                        NextKey();

                    data[i] ^= key[keyOff++];
                }
                sm3c3.BlockUpdate(data, 0, data.Length);
            }

            public virtual void Dofinal(byte[] c3)//�����Ӵ��еķ���
            {
                byte[] p = p2.Y.ToBigInteger().ToByteArray();
                sm3c3.BlockUpdate(p, 0, p.Length);
                sm3c3.DoFinal(c3, 0);
                Reset();
            }
        }
        #endregion
        
    }
}