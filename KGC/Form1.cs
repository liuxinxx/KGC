using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CCWin;
using Com.Itrus.Crypto;
namespace KGC
{
    public partial class mainForm : SkinMain
    {
        #region 主窗体
        public mainForm()
        {

            InitializeComponent();
            //跨线程数据的访问
            TextBox.CheckForIllegalCrossThreadCalls = false;

            //隐形效果调用
            

            this.StartPosition = FormStartPosition.CenterScreen;
        }
        #endregion

        #region 一些个定义

        //从sctime窗体获取时间
        public string _time = "";
        public string time
        {
            get
            {
                return this._time;
            }
            set
            {
                this._time = value;
            }
        }

        //负责监听端口
        Socket sokWelcone = null;
        //负责和客户端Socket通讯
        Socket sokConnection = null;
        Thread threadWatchPort = null;//链接线程
        Thread threadWatchMsg = null;//监听线程
        //Socket集合  socket为键 socket为值用来保存套接字
        Dictionary<string, Socket> dict = new Dictionary<string, Socket>();
        //线程集合    socket为键 线程为值用来保存线程
        Dictionary<string, Thread> dictThread = new Dictionary<string, Thread>();
        //用户集合    socket为键  用户为值
        Dictionary<Socket, string> sn = new Dictionary<Socket, string>();
        //          string 为用户名
        Dictionary<string, Socket> user = new Dictionary<string, Socket>();

        //存放lab的序列
        Dictionary<int, Button> lab = new Dictionary<int, Button>();
        //存放短线
        Dictionary<int, Microsoft.VisualBasic.PowerPacks.LineShape> lin = new Dictionary<int, Microsoft.VisualBasic.PowerPacks.LineShape>();
        public static int userunmber = 0;//顺序签名中签名序列的下标
        public static string info = "";//签名过程中要传递的消息
        public static string[] qmsx = new string[100];//保存本次顺序签名的签名序列
        public static string fastid = "";//保存聚合签名最顶层用户id
        public int max = 0;//保存聚合签名当前的序列号（就是聚合签名的第几次）
       
        SM2 sm2 = SM2.Instance;//sm2对象
        string pripk = "";//私钥文件名
        string ppk = "";//公钥文件名
        file file = new file();//文件读取
        desKey des = new desKey();//des加解密算法
        string serverpk = "";//服务器的公钥
        string serverZ = "";//ecc加密中的Z值
        string serverRS = "";//ecc加密中的RS
        
        #endregion

        #region 服务器初始化
        /// <summary>
        /// 服务器初始化
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        public void StartListening(string ip, string port)
        {
            try
            {

                //ip地址
                IPAddress address = IPAddress.Parse(ip);
                //创建IP节点(包含ip和端口)
                IPEndPoint endpoint = new IPEndPoint(address, int.Parse(port));
                //创建监听套接字（寻址协议，流方式，TCP协议）
                sokWelcone = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //绑定套接字
                sokWelcone.Bind(endpoint);

               

                //参数是指最大连接数为100
                sokWelcone.Listen(100);
                threadWatchPort = new Thread(WatchPort);
                //设置为后台线程
                threadWatchPort.IsBackground = true;
                //启动线程
                threadWatchPort.Start();

                //生成服务器端的公私钥
                pripk = "pri.txt";//服务器的私钥
                ppk = "pk.txt";//服务器的公钥
                //在启动服务器时检测公私钥是否有，没有的话生成公私钥

                try
                {
                    serverpk = des.Decrypt(file.reader(ppk), "abcdefgh");
                    serverRS = file.reader("serverrs.txt");
                }
                catch (Exception)
                {


                    //生成公私钥，存入指定的文件夹下
                    ecc ecc = new ecc();//实例化ecc对象
                    ecc.Creatkey(sm2, pripk, ppk);//创建密钥
                    //签名验证
                    //string ssss = file.reader(_userName + "rs.txt");
                    //MessageBox.Show(ssss);
                    //string rs = des.Decrypt(ssss,"abcdefgh");

                }

                this.Text = "KGC服务器--启动成功";
                //弹窗提示服务器启动成功
                killms ms = new killms();
                ms.ShowDialog();
                // MessageBox.Show("服务器启动！");
                this.label5.Text = "服务器启动成功！";
                this.label5.ForeColor = Color.Green;
                Logmes.maxID = getMAXID();//获取签名序列

                button4.Enabled = false;
                radioButton3.Enabled = false;
                radioButton4.Enabled = false;
            }
            catch (SocketException e)
            {
                MessageBox.Show("可能是你的IP设置异常,导致服务器无法启动！\r\n异常具体为：\r\n\t" + e.Message, "服务器启动链接异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (Exception ex)
            {
                MessageBox.Show("服务器“Form 1窗体”异常 2：" + ex.Message);

            }
        }
        #endregion

        #region 监听用户连接请求
        /// <summary>
        /// 监听用户连接请求
        /// </summary>
        /// <param name="sokConnectionparn"></param>
        public void WatchPort(object sokConnectionparn)
        {
            while (true)//持续不断的监听连接请求
            {
                try
                {
                    //产生Socket实例 ，开始监听客户端连接需求，Accept方法会阻断当前线程                                     
                    sokConnection = sokWelcone.Accept();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("服务器“Form 1窗体”异常 3：" + ex.Message);
                }
                try
                {
                    //加入到Socket队列
                    dict.Add(sokConnection.RemoteEndPoint.ToString(), sokConnection);
                    //实例化线程
                    threadWatchMsg = new Thread(RecMsg);
                    //设置为后台线程，随着主线程退出而退出
                    threadWatchMsg.IsBackground = true;
                    //启动线程
                    threadWatchMsg.Start(sokConnection);
                    //加入到线程队列
                    dictThread.Add(sokConnection.RemoteEndPoint.ToString(), threadWatchMsg);
                }
                catch (Exception ss)
                {

                    MessageBox.Show("服务器“Form 1窗体”异常 4：" + ss.Message);
                }
            }

        }
        #endregion

        #region 读取客户端的消息,并对客户端发来的不同消息做相应的处理
        /// <summary>
        /// 读取客户端的消息,并对客户端发来的不同消息做相应的处理
        /// </summary>
        /// <param name="sokConnectionparn"></param>
        private void RecMsg(object sokConnectionparn)
        {

            Socket sokClient = sokConnectionparn as Socket;
            //sokClient为当前客户端的连接套字节
            while (true)
            {
                int length;
                string username = "";
                byte[] byteMsg = new byte[1024 * 1024 * 4];

                try
                {
                    length = sokClient.Receive(byteMsg, SocketFlags.None);
                    string strMsg = Encoding.UTF8.GetString(byteMsg, 0, length);
                    // MessageBox.Show("用户—  " + sokClient.RemoteEndPoint.ToString() + "  -发来消息\r\n" + strMsg + "\r\n");
                    if (length > 0)//标识接收到的是数据
                    {
                        //解析用户发送过来的数据      
                        string[] a = strMsg.Split('#');//利用“#”作为消息的分割

                        //数组长度
                        int x = a.Length;
                        //标记数据类型，当falg>1是为用户登录信息
                        //falg为数组中消息的个数
                        int falg = 0;
                        for (int j = 0; j < x; j++)
                        {
                            if (a[j] != "")
                            {
                                falg++;
                            }
                        }

                        getMd5 md5 = new getMd5();//用作：对用户的传递过来的机器信息加密

                        #region falg = 1处理、当a[]数组只有一个值时代表返回的是用户名，发生在用户与服务器建立连接时发生
                        if (falg == 1)
                        {
                            username = strMsg;
                        }
                        #endregion

                        #region falg = 4 对用户登录进行处理
                        //对用户登录进行处理
                        //a[0]位用户名,a[1]为CPU序列号.a[2]位硬盘序列号,a[3]为用户注册时间
                        if (falg == 4)
                        {
                            //对数据进行MD5加密
                            a[1] = md5.MD5(a[1]);//CPU序列号
                            a[2] = md5.MD5(a[2]);//硬盘序列号
                            //将用户本次连接的套接字作为键值 用户 名为值，方便后边向指定用户发消息
                            sn.Add(sokClient, a[0]);
                            //将在线用户加入到列表中
                            user.Add(a[0], sokClient);

                            //向用户实况表中填充登录消息a[0]为用户名，sokClient.RemoteEndPoint.ToString()为该用户此次链接的套接字
                            ListViewItem lv = new ListViewItem(a[0]);
                            lv.SubItems.Add(sokClient.RemoteEndPoint.ToString());
                            listView1.Items.Add(lv);
                            //建立数据库链接
                            SqlConnection mysql = new SqlConnection();
                            try
                            {
                                //链接字符串类
                                getSqlStr sqlStr = new getSqlStr();
                                //获取链接字符串
                                string str = sqlStr.sqlstr();
                                //
                                mysql.ConnectionString = str;
                                //打开连接
                                mysql.Open();
                            }
                            catch (Exception a1)
                            {
                                MessageBox.Show("服务器“Form 1窗体”异常  5：" + a1.Message, "出错", MessageBoxButtons.OK, MessageBoxIcon.Error);

                            }

                            try
                            {   //*****
                                //验证登录信息是否正确
                                //*****

                                //查询命令，查询用户是否存在
                                string select = "select [username] from [userdata] where [username]= '" + a[0] + "'";
                                SqlCommand cmd = new SqlCommand(select, mysql);
                                SqlDataReader dr = cmd.ExecuteReader();
                                if (!dr.HasRows)
                                {

                                    dr.Close();//关闭数据集连接 
                                    //查询命令，查询密码是否正确
                                    fs("用户名尚未注册！", sokClient);
                                    //从在线用户的列表中删除
                                    user.Remove(a[0]);

                                }
                                else
                                {
                                    dr.Close();//关闭数据集连接  
                                    string sel = "select * from [userdata] where [username]= '" + a[0] + "'and [userhd] is null";
                                    SqlCommand cmd1 = new SqlCommand(sel, mysql);
                                    SqlDataReader dr1 = cmd1.ExecuteReader();
                                    if (dr1.HasRows)
                                    {
                                        dr1.Close();//关闭数据集连接
                                        fs("系统检测到用户首次登陆，正在注册关键信息请稍等！", sokClient);
                                        string up = "UPDATE userdata SET userhd = '" + a[2] + "',usercpu='" + a[1] + "',zctime = '" + a[3] + "' where username = '" + a[0] + "'";
                                        SqlCommand updata = new SqlCommand(up, mysql);
                                        updata.ExecuteNonQuery();//执行sql语句并返回影响的行数


                                    }
                                    else
                                    {
                                        dr1.Close();//关闭数据集连接
                                        //向指定的客户端发送登录成功
                                        string sele = "select [username] from [userdata] where [username]= '" + a[0] + "'and userhd = '" + a[2] + "'and usercpu='" + a[1] + "'";
                                        SqlCommand cd = new SqlCommand(sele, mysql);
                                        SqlDataReader d = cd.ExecuteReader();
                                        if (!d.HasRows)
                                        {
                                            //从在线用户的列表中删除
                                            user.Remove(a[0]);
                                            fs("服务器检测到，当前配置与注册不符", sokClient);
                                            d.Close();//关闭数据集连接
                                        }
                                        else
                                        {

                                            d.Close();//关闭数据集连接

                                            //MessageBox.Show("server#" + serverpk);

                                            fs("正在登录！#" + serverpk, sokClient);
                                        }

                                    }

                                }
                                mysql.Close();//关闭数据库连接

                            }
                            //错误处理
                            catch (Exception ex)
                            {
                                MessageBox.Show("服务器“Form 1窗体”异常  6：" + ex.Message);
                            }
                        }
                        #endregion

                        #region 签名消息说明（同意和拒绝）
                        //签名消息处理，客户端返回两种格式
                        //第一种：
                        //       *#*#签名消息#用户名#同意签名的时间
                        //       有效消息为* * 用户名
                        //       第一个*和第二个*同时返回时代表该用户同意了签名
                        //第二种：
                        //       *#&#用户名
                        //       有效消息为* & 用户名
                        //       第一个*和第二个&同时返回时代表该用户拒绝了签名 
                        #endregion

                        #region falg = 3处理签名失败
                        if (falg == 3)
                        {
                            if (a[0] == "*" && a[1] == "&")//&& qmsx[userunmber] == a[2]
                            {
                                Logmes.endtime = System.DateTime.Now;//签名结束时间
                                Logmes.result = "失败";//签名结果！
                                Logmes.note = "由于用户：" + qmsx[userunmber] + "拒绝签名,签名失败！";//签名失败记录失败原因
                                Logmes.ingtime = Logmes.DateDiff(Logmes.begintime, Logmes.endtime);//计算时差
                                Logmes.inster();//执行插入
                                
                                MessageBox.Show("由于" + qmsx[userunmber] + "拒绝签名\n\n本次签名失败！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                userunmber++;
                            }
                        }
                        #endregion

                        #region falg = 5 对签名处理
                        if (falg == 5)
                        {
                            // MessageBox.Show("我是服务器，客户端发来消息：" + a[0] + "    " + a[1] + "   " + a[2] + "  " + a[3] + "   " + a[4]);
                            if (radioButton3.Checked == true)//顺序签名
                            {
                                //MessageBox.Show("我是服务器，客户端发来消息：" + a[0] + "    " + a[1] + "   " + a[2] +"  "+a[3] + "   " + a[4]);
                                /*a[0] = "*" ;                                 
                                 *a[1] = "*" ;
                                 *a[2] = 传递的消息 ;
                                 *a[3] = 
                                 *
                                 */
                                if (a[0] == "*" && a[1] == "*" && qmsx[userunmber] == a[3])//qmsx为存储签名顺序的结构体数组，nserunmber为用户计数器
                                {
                                    info = a[2];//传递的消息
                                    userunmber++;
                                    //MessageBox.Show("我是服务器，下一次该它签名了：" + qmsx[userunmber]);
                                    //发送消息的格式，qmsx()为一个签名顺序的队列,接下来是签名的消息+时间,“0”代表它不是第一个签名者，timeBox.Text为本次签名的有效时长
                                    fs(qmsx[userunmber] + "|" + info + "|" + "时间" + timeBox.Text.Trim() + "|0", user[qmsx[userunmber]]);//告诉下一个签名用户同时将本次签名有效时长告诉用户
                                    //MessageBox.Show("我是服务器！我接收到" + a[2] + "\n\n发来的消息：" + a[0] + a[1] + a[2]);
                                }
                            }
                            if (radioButton4.Checked == true)//聚合签名
                            {
                                if (a[0] == "*" && a[1] == "*")//qmsx为存储签名顺序的结构体数组，nserunmber为用户计数器
                                {
                                    info += a[2];//传递的消息
                                    //将当前节点的入度置为 -1
                                    upsj(lab[int.Parse(a[3])].Text.Trim(), -1, "@@", max);

                                    //获取当前节点的根节点
                                    string strlab = "select Id , pID from jhqm where ID = '" + lab[int.Parse(a[3])].Text.Trim() + "' and  number = '" + max + "'";
                                    //labb为当前节点的根节点
                                    string labb = retid(strlab);

                                    //获取当前节点跟节点的入度
                                    string selsql = "select Id , flag from jhqm where ID = '" + labb + "' and  number = '" + max + "'";
                                    //当前节点根节点的入度
                                    int aa = retflag(selsql,1);

                                    //将当前节点根节点的入度-1 更新数据库
                                    //MessageBox.Show("子节点：" + lab[int.Parse(a[3])].Text.Trim() + "根节点是：" + labb + "根节点"+ labb +"此时的入度为:"+fff);
                                    int fff = aa - 1;

                                    upsj(lab[int.Parse(labb)].Text.ToString(), fff, "@@", max);

                                    //MessageBox.Show("我是服务器，下一次该它签名了：" + qmsx[userunmber]);
                                    //发送消息的格式，qmsx()为一个签名顺序的队列,接下来是签名的消息+时间,“0”代表它不是第一个签名者，timeBox.Text为本次签名的有效时长
                                    //fs(lab[int.Parse(a[3])].Text.ToString()+ "|" + info + "|" + "时间" + timeBox.Text.Trim() + "|0", user[qmsx[userunmber]]);//告诉下一个签名用户同时将本次签名有效时长告诉用户
                                    //MessageBox.Show("我是服务器！我接收到" + a[2] + "\n\n发来的消息：" + a[0] + a[1] + a[2]);

                                    jh();

                                }

                            }

                        }
                        #endregion

                        #region falg = 6 处理、从客户端上传来的用户公钥

                        if (falg == 6)
                        {
                            // MessageBox.Show(a[2] + "\n" + a[1]);

                            //建立数据库链接
                            SqlConnection mysql = new SqlConnection();
                            try
                            {
                                //链接字符串类
                                getSqlStr sqlStr = new getSqlStr();
                                //获取链接字符串
                                string str = sqlStr.sqlstr();
                                //
                                mysql.ConnectionString = str;
                                //打开连接
                                mysql.Open();
                                //先查询此用户是否是第一次上传公钥
                                string select = "select [ID] from [userpk] where [ID]= '" + a[2] + "'";
                                SqlCommand cmd = new SqlCommand(select, mysql);
                                SqlDataReader dr = cmd.ExecuteReader();

                                //MessageBox.Show("服务器：" + a[0] + "\r\n" + a[0]);

                                //第一次上传
                                if (!dr.HasRows)
                                {
                                    dr.Close();
                                    string up = "insert into userpk values('" + a[2] + "','" + a[1] + "')";
                                    SqlCommand updata = new SqlCommand(up, mysql);
                                    updata.ExecuteNonQuery();//执行sql语句并返回影响的行数

                                    dr.Close();
                                }
                                //更新公钥
                                else
                                {
                                    dr.Close();
                                    //string up = "insert into userpk values('"+a[2]+"','"+a[1]+"')";
                                    string up = "UPDATE userpk SET ID = '" + a[2] + "',userpk='" + a[1] + "'" + "where ID = '" + a[2] + "'";
                                    SqlCommand updata = new SqlCommand(up, mysql);
                                    updata.ExecuteNonQuery();//执行sql语句并返回影响的行数

                                }
                                mysql.Close();

                            }
                            catch (Exception ss)
                            {
                                MessageBox.Show("插入用户公钥对出错！" + ss.ToString());
                            }


                        }
                        #endregion

                        /*//a[2],a[3],a[4]合起来为加密后的消息
                         * a[0] = "*";
                         * a[1] = "*";
                         * a[2] = c1 ;
                         * a[3] = c2 ;
                         * a[4] = c3 ;
                         * a[5] = Z ;
                         * a[6] = R ;
                         * a[7] = S ;
                         * a[8] = name ;
                         * a[9] = time ;
                         */

                        if (falg == 10)
                        {

                            // MessageBox.Show(a[0] + "\r\n" + a[1] + "\r\n" + a[2] + "\r\n" + a[3] + "\r\n" + a[4] + "\r\n" + a[5] + "\r\n" + a[6] + "\r\n" + a[7] + "\r\n" + a[8] + "\r\n" + a[9]);
                            ecc ecc = new ecc();
                            file file = new KGC.file();

                            //从数据库获取用户的公钥
                            string selpk = "select userpk  from userpk where ID = '" + a[8] + "'";
                            string userpk = getppk(selpk);
                            //  MessageBox.Show(userpk);

                            if (ecc.Signature_Check(sm2, userpk, a[5], a[6], a[7]) == true)
                            {
                                //msg mmgg = new msg();
                                //MessageBox.Show("KGC(R,S)验证成功！\r\nYES");
                               // mmgg.dd("KGC(R,S)验证成功！\r\nYES");
                                //mmgg.Show();

                                string jmxx = ecc.deciphering(sm2, pripk, a[2], a[3], a[4]);//解密消息
                                Logmes.message = jmxx;//获取解密后的消息
                                if (jmxx != "0")
                                {
                                    userunmber++;
                                    //MessageBox.Show("服务器解密后为" + jmxx);
                                    //serverRS = file.reader("serverrs.txt");//读取服务器的RS

                                    //MessageBox.Show("下个用户是：" + qmsx[userunmber]);
                                    //查询下个用户的公钥
                                    string nextpk = "select userpk  from userpk where ID = '" + qmsx[userunmber] + "'";
                                    string nextppk = getppk(nextpk);

                                    serverZ = ecc.Test_sm2_sign(sm2, pripk, ppk, "server");//返回服务器的Z，并将服务器生成的RS存入serverrs.txt文件里

                                    try
                                    {
                                        file.writer("nextpk.txt", des.Encrypt(nextppk, "abcdefgh"));
                                    }
                                    catch (Exception ss)
                                    {
                                        MessageBox.Show("不存在nextpk.txt\r\n" + ss.ToString());
                                        throw;
                                    }
                                    string[] mm = ecc.Test_sm2_cipher(sm2, jmxx, "nextpk.txt");//利用下个用户的公钥加密生成密后消息即 要发给下个用户的c1 , c2  ,c3;
                                    // MessageBox.Show(serverZ);
                                    info = serverRS + "#" + serverZ + "#" + mm[0] + "#" + mm[1] + "#" + mm[2];
                                    //MessageBox.Show("我是服务器，下一次该它签名了：" + qmsx[userunmber]);
                                    ////发送消息的格式，qmsx()为一个签名顺序的队列,接下来是签名的消息+时间,“0”代表它不是第一个签名者，timeBox.Text为本次签名的有效时长
                                    // MessageBox.Show(qmsx[userunmber] + "#" + info + "#" + "时间" + timeBox.Text.Trim() + "#0");
                                    /*发送的消息
                                     * 1.qmsx[userunmber]下个用户的id
                                     * 2.info为发送的消息包含（R#S（RS中间用“#”隔开），Z，c1 ,c2 ,c3）
                                     * 3.签名时效
                                     * 4.是否是签名发起者的标志（1代表为签名发起者，0代表不是）
                                     */

                                    fs(qmsx[userunmber] + "#" + info + "#" + "时间" + timeBox.Text.Trim() + "#0", user[qmsx[userunmber]]);//告诉下一个签名用户同时将本次签名有效时长告诉用户

                                }
                                else
                                {
                                    MessageBox.Show("客户端" + qmsx[userunmber] + "数据校验失败！");
                                }

                            }
                            else
                            {
                                MessageBox.Show("服务器签名验证不通过NO");
                            }


                        }


                    }
                }
                catch (SocketException)//套字节异常，实现用户离线后从用户在线实况中删除用户信息
                {

                    // 从 通信套接字 集合中删除被中断连接的通信套接字； 
                    dict.Remove(sokClient.RemoteEndPoint.ToString());
                    // 从通信线程集合中删除被中断连接的通信线程对象；
                    dictThread.Remove(sokClient.RemoteEndPoint.ToString());
                    // 从列表中移除被中断的连接IP 
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        if (listView1.Items[i].SubItems[0].Text == sn[sokClient])
                        {
                            listView1.Items.RemoveAt(i);
                            i--;
                        }
                    }

                    break;
                }
                catch (Exception e)//程序异常，实现签名结束时跳出签名结束的提示
                {
                    //签名出现异常
                    MessageBox.Show("服务器“Form 1窗体”异常 7：\n\n签名结束\n\n" + e.Message);
                    Logmes.result = "成功";//签名结果！
                    Logmes.endtime = System.DateTime.Now;//签名结束时间
                    Logmes.ingtime = Logmes.DateDiff(Logmes.begintime, Logmes.endtime);//计算时差
                    Logmes.inster();//执行插入
                    // 从 通信套接字 集合中删除被中断连接的通信套接字； 
                    dict.Remove(sokClient.RemoteEndPoint.ToString());
                    // 从通信线程集合中删除被中断连接的通信线程对象；
                    dictThread.Remove(sokClient.RemoteEndPoint.ToString());
                    break;
                }
            }
        }
        #endregion

        #region 查询获取公钥
        /// <summary>
        /// 查询获取公钥
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>返回公钥</returns>
        public string getppk(string sql)
        {

            //建立数据库链接
            SqlConnection mysql = new SqlConnection();

            //链接字符串类
            getSqlStr sqlStr = new getSqlStr();
            //获取链接字符串
            string str = sqlStr.sqlstr();
            mysql.ConnectionString = str;
            //打开连接
            mysql.Open();

            try
            {
                SqlCommand sel = new SqlCommand(sql, mysql);
                //获取数据适配器
                SqlDataReader fm = sel.ExecuteReader();
                if (fm != null)
                {
                    if (fm.Read())
                    {
                        return fm.GetString(0);
                    }
                    else
                        return "0";
                }
                else
                    return "0";
            }
            catch (Exception sd)
            {
                MessageBox.Show("获取公钥失败\r\n" + sd.ToString());
                throw;
            }
        }
        #endregion


        #region 不要动

        #region 查询出所有入度为 0 的用户，即该启动签名的用户
        /// <summary>
        /// 查询出所有入度为 0 的用户，即该启动签名的用户
        /// </summary>
        public void jh()
        {
            #region 查询出所有入度为 0 的用户，即该启动签名的用户

            string selmysql = "select id from jhqm where flag = " + 0 + "and number = " + max + "";
            SqlDataReader fm = returnfal(selmysql);

            while (fm.Read())
            {

                try
                {

                    // MessageBox.Show("开始第二轮签名" + fm[0].ToString());
                    fs(fm[0].ToString() + "|" + info + "|" + "时间" + timeBox.Text.Trim() + "|1", user[fm[0].ToString()]);//告诉下一个签名用户同时将本次签名有效时长告诉用户
                }
                catch (Exception qm)
                {
                    MessageBox.Show("第二轮签名出错\r\n" + qm.ToString());
                }
                string strss = fm[0].ToString();
                if (strss == fastid)
                {
                    //停止签名，签名结束

                    // upsj(fastid,0, "@@", max);
                    MessageBox.Show("签名结束！");

                }
            }

            fm.Close(); //关闭数据集，链接

            #endregion

        }
        #endregion

        #region 向客户端发送消息，发送消息的函数，和其函数的重载fs()和fs()
        /// <summary>
        /// 向客户端直接发送要发送的消息
        /// </summary>
        /// <param name="msg">要发送的消息</param>
        private void fs(string msg)
        {
            try
            {
                string strMsg = msg.Trim();

                //装换成字节
                byte[] byteMsg = Encoding.UTF8.GetBytes(strMsg);
                //群发（可以实现在签名没到用户之前，告知用户当前签名序列中是否有你）
                foreach (Socket s in dict.Values)
                {
                    s.Send(byteMsg, byteMsg.Length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("服务器“Form 1窗体”异常  8：" + ex.Message);

            }

        }
        /// <summary>
        /// //向指定的客户端发消息
        /// </summary>
        /// <param name="msg">要发送的消息</param>
        /// <param name="sok">指定的客户端</param>
        private void fs(string msg, Socket sok)
        {

            #region 向指定的sok中发送要发送的消息
            try
            {
                string strMsg = msg.Trim();

                //装换成字节
                byte[] byteMsg = Encoding.UTF8.GetBytes(strMsg);
                //群发
                foreach (Socket s in dict.Values)
                {
                    if (s == sok)
                        s.Send(byteMsg, byteMsg.Length, SocketFlags.None);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("服务器“Form 1窗体”异常  9：" + ex.Message + "\n\n此次发送的是：" + msg);


            }

            #endregion
        }
        #endregion

        #region 将数据库数据填充到表中，此处有待改进
        /// <summary>
        /// 将数据库数据填充到Form1窗体的表中，此处有待改进
        /// </summary>
        private void userdate()
        {
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCellsExceptHeaders;
            getSqlStr ss = new getSqlStr();
            string sql = ss.sqlstr();
            //连接数据库
            SqlConnection myCon = new SqlConnection();
            try
            {
                myCon.ConnectionString = sql;
                myCon.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show("服务器“Form 1窗体”异常 10:" + e.Message);
            }
            //使用SqlCommand提交查询命令
            string selStr = "select ID as 用户ID,username as 用户名 from userdata where usercpu  is  not  null";
            SqlCommand sel = new SqlCommand(selStr, myCon);
            //获取数据适配器
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = sel;
            //填充dataset
            DataSet ds = new DataSet();
            DataTable dt = new DataTable();
            da.Fill(dt);
            //将dataset中的数据绑定到
            this.dataGridView1.DataSource = dt.DefaultView;

        }
        #endregion

        #region 动态调整窗体大小，以及字体大小
        private float X;
        private float Y;
        private void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ":" + con.Height + ":" + con.Left + ":" + con.Top + ":" + con.Font.Size;
                if (con.Controls.Count > 0)
                    setTag(con);
            }
        }
        private void setControls(float newx, float newy, Control cons)
        {
            foreach (Control con in cons.Controls)
            {

                string[] mytag = con.Tag.ToString().Split(new char[] { ':' });
                float a = Convert.ToSingle(mytag[0]) * newx;
                con.Width = (int)a;
                a = Convert.ToSingle(mytag[1]) * newy;
                con.Height = (int)(a);
                a = Convert.ToSingle(mytag[2]) * newx;
                con.Left = (int)(a);
                a = Convert.ToSingle(mytag[3]) * newy;
                con.Top = (int)(a);
                Single currentSize = Convert.ToSingle(mytag[4]) * Math.Min(newx, newy);
                con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                if (con.Controls.Count > 0)
                {
                    setControls(newx, newy, con);
                }
            }

        }
        private void FormLogin_Resize(object sender, EventArgs e)
        {
            float newx = (this.Width) / X;
            float newy = this.Height / Y;
            setControls(newx, newy, this);
        }
        #endregion

        #region 主窗体Load事件
        private void Form1_Load(object sender, EventArgs e)
        {
            //加载lab和lin队列
            addxy();

            Logmes.x = this.Location.X;
            Logmes.y = this.Location.Y + this.Height;

            #region 动态改变窗体大小
            this.Resize += new EventHandler(FormLogin_Resize);
            X = this.Width;
            Y = this.Height;
            setTag(this);
            #endregion

            //第一次加载时令“管理”页面标识为当前选中，即改变其背景色
            管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16769218);
            #region 菜单栏初始化为隐藏
            用户管理.Visible = false;
            安全管理.Visible = false;
            调度管理.Visible = false;
            用户实况.Visible = false;
            管理.Visible = true;


            #endregion
            userdate();
        }
        #endregion

        #region 菜单栏
        private void 用户管理ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            用户管理.Visible = true;
            安全管理.Visible = false;
            调度管理.Visible = false;

            用户实况.Visible = false;
            管理.Visible = false;

            用户管理ToolStripMenuItem.BackColor = System.Drawing.ColorTranslator.FromWin32(16769218);
            管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            用户实况toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            调度管理toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            安全管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);

        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            用户管理.Visible = false;
            安全管理.Visible = true;
            调度管理.Visible = false;
            用户实况.Visible = false;
            管理.Visible = false;

            用户管理ToolStripMenuItem.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            用户实况toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            调度管理toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            安全管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16769218);
        }

        private void toolStripMenuItem3_Click_1(object sender, EventArgs e)
        {

        }
        private void toolStripMenuItem3_Click_2(object sender, EventArgs e)
        {
            用户管理.Visible = false;
            安全管理.Visible = false;
            调度管理.Visible = false;
            用户实况.Visible = false;
            管理.Visible = true;

            用户管理ToolStripMenuItem.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16769218);
            用户实况toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            调度管理toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            安全管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            用户管理.Visible = false;
            安全管理.Visible = false;
            调度管理.Visible = true;
            用户实况.Visible = false;
            管理.Visible = false;

            用户管理ToolStripMenuItem.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            用户实况toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            调度管理toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16769218);
            安全管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);

        }


        private void toolStripMenuItem2_Click_1(object sender, EventArgs e)
        {
            用户管理.Visible = false;
            安全管理.Visible = false;
            调度管理.Visible = false;
            用户实况.Visible = true;
            管理.Visible = false;

            用户管理ToolStripMenuItem.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            用户实况toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16769218);
            调度管理toolStripMenuItem2.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
            安全管理toolStripMenuItem3.BackColor = System.Drawing.ColorTranslator.FromWin32(16547880);
        }
        #endregion

        #region 点击“用户信息管理”弹出窗体
        private void 用户信息管理_Click(object sender, EventArgs e)
        {
            //弹出用户信息管理页面
            userInfo us = new userInfo();
            us.ShowDialog();

        }
        #endregion

        #region timer1_Tick事件，将服务器当前时间同步到客户端
        private void timer1_Tick(object sender, EventArgs e)
        {
            //获取服务器当前时间，
            label4.Text = DateTime.Now.ToString();
            
        }
        #endregion

        #region 点击“用户公钥管理_Click”弹出窗体
        private void 用户公钥管理_Click(object sender, EventArgs e)
        {
            //弹出用户公钥管理界面
            userPK pk = new userPK();
            pk.ShowDialog();
        }
        #endregion

        #region 点击“系统密钥管理_Click”弹出窗体
        private void 系统密钥管理_Click(object sender, EventArgs e)
        {
            //弹出系统秘钥管理界面
            kgcKey kk = new kgcKey();
            kk.ShowDialog();
        }
        #endregion

        #region 点击“调度历史查询_Click”弹出窗体
        private void 调度历史查询_Click(object sender, EventArgs e)
        {
            //调度历史界面
            schhistory sh = new schhistory();
            sh.ShowDialog();
        }
        #endregion

        #region 服务器启动按钮
        private void button4_Click_1(object sender, EventArgs e)
        {
            file file = new file();
            string f = file.reader("ipMsg.txt");//读取保存在本地的ＩＰ
            string[] sstr = f.Split(':');//利用":"分割IP和端口

            //启动服务器
            StartListening(sstr[0], sstr[1]);
            //令"启动服务器"按钮变为不可选取
           
        }
        #endregion

        #region 去掉”签名管理“下“已注册用户”表中多余的空格去掉
        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {

            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }
        #endregion

        #region //标识是否是第一次加载
        static int n = 1;
        #endregion

        #region 添加but和lin的list
        /// <summary>
        /// 添加but和lin的list
        /// </summary>
        public void addxy()
        {
            #region but和lin两个list

            lab.Add(1, but1);
            lab.Add(2, but2);
            lab.Add(3, but3);
            lab.Add(4, but4);
            lab.Add(5, but5);
            lab.Add(6, but6);
            lab.Add(7, but7);

            //获取点击的行的第一个列值
            lin.Add(1, this.lineShape1);
            lin.Add(2, this.lineShape2);
            lin.Add(3, this.lineShape3);
            lin.Add(4, this.lineShape4);
            lin.Add(5, this.lineShape5);
            lin.Add(6, this.lineShape6);
            lin.Add(7, this.lineShape6);
            #endregion

        }
        #endregion

        #region ”重新生成“按钮
        /// <summary>
        /// ”重新生成“按钮
        /// </summary>
        public void clear()
        {
            if (radioButton3.Checked == true)
            {
                textBox2.Text = "";
                userdate();
            }
            int hh = 1;
            if (radioButton4.Checked == true)
            {

                n = 1;
                while (true)
                {
                    lab[hh].Visible = false;
                    lin[hh].Visible = false;

                    //    lin.Remove(hh);
                    //    lab.Remove(hh);
                    hh++;
                    if (hh == 8)
                    {
                        n = 1;
                        break;

                    }
                }
                lab.Clear();
                lin.Clear();
                userdate();
            }

        }
        #endregion

        #region 设置签名顺序，获取当前选中的用户
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            //顺序签名
            if (radioButton3.Checked == true)
            {
                try
                {
                    string a = textBox2.Text;
                    //获取点击的行的第一个列值
                    string ss = this.dataGridView1.SelectedCells[1].Value.ToString().Trim();
                    textBox2.Text = ss + "#" + a;
                    if (radioButton2.Checked == false)
                    {
                        //删除选中行
                        foreach (DataGridViewRow r in dataGridView1.SelectedRows)
                        {
                            if (!r.IsNewRow)
                            {
                                dataGridView1.Rows.Remove(r);
                            }
                        }
                    }
                }
                catch (Exception)//当表中没有数据时的异常捕捉
                {

                }
            }
            //聚合签名
            if (radioButton4.Checked == true)
            {
                try
                {
                    if (n == 1)//第一次加载时将button和"短线"添加到list里边去
                    {
                        try { addxy(); }
                        catch { };

                    }
                    //获取点击的行的第一个列值
                    string ss = this.dataGridView1.SelectedCells[1].Value.ToString().Trim();

                    if (n > 7)
                    {
                        MessageBox.Show("最多为7个签名用户", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    //MessageBox.Show(n.ToString());
                    else
                    {
                        lab[n].Text = ss;

                        lab[n].Visible = true;
                        int a = n - 1;
                        if (a != 0)
                            lin[a].Visible = true;
                        n++;
                        foreach (DataGridViewRow r in dataGridView1.SelectedRows)
                        {
                            if (!r.IsNewRow)
                            {
                                dataGridView1.Rows.Remove(r);
                            }
                        }
                    }

                }
                catch (Exception)
                {


                }
                //删除选中行


            }
        }
        #endregion

        #region 聚合签名时查询签名标记
        /// <summary>
        /// 返回节点入度
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>标记</returns>
        public int retflag(string sql ,int fa)
        {

            //建立数据库链接
            SqlConnection mysql = new SqlConnection();

            //链接字符串类
            getSqlStr sqlStr = new getSqlStr();
            //获取链接字符串
            string str = sqlStr.sqlstr();
            mysql.ConnectionString = str;
            //打开连接
            mysql.Open();

            try
            {
                SqlCommand sel = new SqlCommand(sql, mysql);
                //获取数据适配器
                SqlDataReader fm = sel.ExecuteReader();
                if (fm != null)
                {
                    if (fm.Read())
                    {
                        return fm.GetInt32(fa);
                    }
                    else
                        return 0;
                }
                else
                    return 0;
            }
            catch (Exception)
            {
                //MessageBox.Show("获取节点入度失败" + sd.ToString());
                //throw;
                return 0;
            }
        }
        #endregion

        #region 聚合签名时查询id
        /// <summary>
        /// 返回id
        /// </summary>
        /// <param name="sql">查询语句</param>
        /// <returns>标记</returns>
        public string retid(string sql)
        {

            //建立数据库链接
            SqlConnection mysql = new SqlConnection();

            //链接字符串类
            getSqlStr sqlStr = new getSqlStr();
            //获取链接字符串
            string str = sqlStr.sqlstr();
            mysql.ConnectionString = str;
            //打开连接
            mysql.Open();

            try
            {
                SqlCommand sel = new SqlCommand(sql, mysql);
                //获取数据适配器
                SqlDataReader fm = sel.ExecuteReader();
                if (fm != null)
                {
                    if (fm.Read())
                    {
                        return fm.GetString(1);
                    }
                    else
                        return "0";
                }
                else
                    return "0";
            }
            catch (Exception sd)
            {
                MessageBox.Show("获取id失败" + sd.ToString());
                throw;
            }
        }
        #endregion

        #region 更新flag标记
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flag"></param>
        public void upsj(string id, int flag, string pid, int max)
        {
            //建立数据库链接
            SqlConnection mysql = new SqlConnection();

            //链接字符串类
            getSqlStr sqlStr = new getSqlStr();
            //获取链接字符串
            string str = sqlStr.sqlstr();

            mysql.ConnectionString = str;
            //打开连接
            mysql.Open();
            string up = "";
            if (pid == "@@")
            {

                up = "UPDATE jhqm SET ID = '" + id + "',flag=" + flag + "where ID = '" + id + "'and number = " + max + "";
            }
            else

                up = "UPDATE jhqm SET ID = '" + id + "',pID='" + pid + "'where ID = '" + id + "'and number =" + max + "";

            SqlCommand updata = new SqlCommand(up, mysql);

            updata.ExecuteNonQuery();//执行sql语句并返回影响的行数
        }
        #endregion

        #region 将聚合签名的相关消息插入到数据库中
        /// <summary>
        /// 将聚合签名的相关消息插入到数据库中
        /// </summary>
        /// <param name="id">上一个用户的id</param>
        /// <param name="flag">上一个用户的入度</param>
        /// <param name="pid">当前用户的id</param>
        /// <param name="max">签名序号</param>
        public void inster(string id, int flag, string pid, int max)
        {
            //建立数据库链接
            SqlConnection mysql = new SqlConnection();

            //链接字符串类
            getSqlStr sqlStr = new getSqlStr();
            //获取链接字符串
            string str = sqlStr.sqlstr();
            //
            mysql.ConnectionString = str;
            //打开连接
            mysql.Open();

            try
            {
                string ist = "insert into jhqm ([ID],[flag],[pID],[number]) values('" + id + "'," + flag + ",'" + pid + "'," + max + ")";
                SqlCommand insert = new SqlCommand(ist, mysql);

                insert.ExecuteNonQuery();//执行sql语句并返回影响的行数
            }
            catch (Exception t)
            {

                MessageBox.Show(t.ToString());
            }
        }
        #endregion

        #region 重新生成签名序列
        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                clear();
            }
            catch
            {
                MessageBox.Show("数据为空");

            }
        }
        #endregion

        #region 获取查询到入度为0 的节点，返回数据集
        /// <summary>
        /// 获取要查询的
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns></returns>
        public SqlDataReader returnfal(string sql)
        {
            //建立数据库链接
            SqlConnection mysql = new SqlConnection();

            //链接字符串类
            getSqlStr sqlStr = new getSqlStr();
            //获取链接字符串
            string str = sqlStr.sqlstr();

            mysql.ConnectionString = str;
            //打开连接
            mysql.Open();

            SqlCommand sel = new SqlCommand(sql, mysql);
            //获取数据适配器
            SqlDataReader fm = sel.ExecuteReader();

            return fm;

        }
        #endregion

        public int getMAXID()
        {
            
                string maxf = "select  max(ID)  from history ";
               
                return retflag(maxf,0) + 1;

        }
        #region 存储当前签名序列，并设置签名序列、告知签名用户（包含顺序签名和聚合签名）
        private void button5_Click(object sender, EventArgs e)
        {
            
           
            
            
            #region 顺序签名
            if (radioButton3.Checked == true)
            {
                if (textBox2.Text.Trim() == "")
                {
                    MessageBox.Show("顺序签名不能为空！！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (MessageBox.Show("您确定当前签名顺序吗？\n\n " + textBox2.Text + "\n\n点击“否”即可从新选择!", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        button5.Enabled = false;
                        button6.Enabled = false;
                        try
                        {
                            string gg = "";
                            string s = textBox2.Text.Trim();
                            int length = s.Length;
                            //初始化签名序列字符串数组中的值
                            #region 为qmsx字符数组赋值
                            int j = 0;
                            while (true)
                            {

                                if (j == 100)
                                    break;
                                qmsx[j] = "";
                                // MessageBox.Show(qmsx[j]);
                                j++;
                            }
                            #endregion

                            #region 将全名顺序存入qmsx字符数组中，并告知第一个客户端，它为第一个
                            int h = 0;
                            int i = 0;
                            int tt = 0;
                            string fast = "";
                            while (true)
                            {
                                gg += s[i];
                                i++;
                                if (s[i] == '#')
                                {
                                    i++;
                                    if (tt == 0)//找到第一个签名的人
                                    {
                                        fast = gg;
                                        tt = 1;
                                    }
                                    if (gg != "")
                                    {
                                        qmsx[h] = gg;
                                        h++;
                                        gg = "";
                                    }
                                }
                                if (i == length)
                                {
                                    button5.Enabled = button6.Enabled = true;
                                    break;
                                }
                            }

                            try
                            {
                                Logmes.sequence = textBox2.Text.Trim();//将签名顺序存起来
                                Logmes.first = fast;//签名发起者
                                
                                Logmes.time = timeBox.Text.Trim();//获取签名有效时长
                                Logmes.begintime = System.DateTime.Now;//获取开始时间
                            }
                            catch (Exception es)
                            {
                                MessageBox.Show(es.ToString());
                                throw;
                            }

                            //通知所有参与签名用户，该签名序列中有他
                            for (int hh = 0; hh < h; hh++)
                            {
                                fs(qmsx[hh], user[qmsx[hh]]);

                            }
                            //线程休眠0.1秒，防止数据粘连

                            Thread.Sleep(100);

                            //通知第一个签名的用户 fast为第一个用户的名字，“|”为分隔符，“0”为标志位，接下来为本次签名的有效时间
                            //fs(fast + "|0|"+ "时间" + timeBox.Text.Trim(), user[fast]);
                            fs(fast + "#" + "请输入要签名的消息" + "#" + "时间" + timeBox.Text.Trim() + "#1", user[fast]);
                            #endregion

                        }
                        catch (Exception ss)
                        {

                            MessageBox.Show("服务器“Form 1窗体”异常 11：" + ss.Message);
                        }

                    }
                    else//点击重新生成按钮是对数据的初始化
                    {
                        textBox2.Text = "";
                        //从新载入数据
                        userdate();
                    }
                }
            }
            #endregion

            #region 聚合签名
            int qmbj = 0;//查出本次签名序列的标记，为0 是代表第一次查询本次签名序列，

            if (radioButton4.Checked == true)
            {
                #region 将本次聚合签名的数据结构存入数据库

                if (but1.Visible == false)
                {
                    MessageBox.Show("聚合签名不能为空！！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {

                    if (MessageBox.Show("您确定当前签名顺序吗？点击“否”即可从新选择!", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        fastid = lab[1].Text.Trim();//保存聚合签名的最顶层用户id
                        if (qmbj == 0)
                        {
                            string maxf = "select id,max(number) number from jhqm group by ID";
                            max = retflag(maxf,1) + 1;
                            qmbj = 1;
                        }
                        // MessageBox.Show(max.ToString());
                        //将入度写入数据库  
                       
                        for (int kk = 1; kk < n; kk++)
                        {
                            //初始化数据库
                            inster(lab[kk].Text.Trim(), 0, "", max);

                        }
                        for (int j = 1; j < n; j++)
                        {
                            //k为当前节点的根节点
                            int k = (int)((j - 1) / 2.0 + 0.5);
                            if (k == 0)
                                k = 1;

                            //获取当前节点根节点的入度
                            string selsql = "select Id , flag from jhqm where ID = '" + lab[k].Text.Trim() + "' and  number = '" + max + "'";
                            int a = retflag(selsql,1);
                            //将当前节点根节点的入度+1 更新数据库
                            int fff = a + 1;
                            upsj(lab[k].Text.ToString(), fff, "@@", max);
                            //指出当前节点对应的根节点
                            upsj(lab[j].Text.ToString(), 1, lab[k].Text.ToString(), max);
                        }
                        //直接指定最顶用户的入度为2
                        upsj(fastid, 2, "@@", max);

                        //通知所有用户签名任务
                        for (int kk = 1; kk < n; kk++)
                        {
                            //初始化数据库
                            fs(lab[kk].Text.Trim(), user[lab[kk].Text.Trim()]);

                        }

                        #region 查询出所有入度为 0 的用户，遂即启动签名！告知最低层用户，开始签名

                        string selmysql = "select id from jhqm where flag = " + 0 + "and number = " + max + "";
                        SqlDataReader fm = returnfal(selmysql);

                        while (fm.Read())
                        {
                            try
                            {

                                //告知最低层签名用户                        
                                fs(fm[0].ToString(), user[fm[0].ToString()]);
                                //线程休眠休眠0.1秒，避免数据粘连
                                Thread.Sleep(100);

                                //告知他们是最低层用户，给予签名消息编辑权限；
                                fs(fm[0].ToString() + "|" + "请输入要签名的消息" + "|" + "时间" + timeBox.Text.Trim() + "|1", user[fm[0].ToString()]);//告诉下一个签名用户同时将本次签名有效时长告诉用户
                                //MessageBox.Show(fm[0].ToString());

                                //告诉客户端后，将最低层用户置的入度置为一
                                upsj(fm[0].ToString(), -1, "@@", max);

                            }
                            catch (Exception errmsg)
                            {
                                MessageBox.Show("聚合签名在告知最低层用户时发生异常！\r\n" + errmsg.ToString());
                            }

                        }
                        fm.Close(); //关闭数据集，链接

                        #endregion
                    }
                    else
                    {
                        //重新生成
                        clear();
                    }
                }
                #endregion

            }

            #endregion
        }
        #endregion

        #region 点击“系统配置_Click”弹出窗体
        private void 系统配置_Click(object sender, EventArgs e)
        {
            sysconf sc = new sysconf();
            sc.ShowDialog();

        }
        #endregion

        #region 点击获取“当前在线用户”的值
        private void listView1_Click(object sender, EventArgs e)
        {
            //MessageBox.Show(listView1.Items.IndexOf(listView1.FocusedItem).ToString());
        }
        #endregion

        #region 主窗体关闭按钮
        private void button2_Click_1(object sender, EventArgs e)
        {
            Application.Exit();
            this.Close();
        }
        #endregion

        #region 点击拖动窗体代码
        Point mouse_offset;
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_offset = new Point(-e.X, -e.Y);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            //设置坐标
            Logmes.x = this.Location.X;
            Logmes.y = this.Location.Y + this.Height;
            

            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouse_offset.X, mouse_offset.Y);
                Location = mousePos;
            }
        }
        #endregion

        #region 最小化按钮
        private void button7_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //提示“最小化”字样
        private void button7_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.button7, "最小化");

        }
        #endregion

        #region 鼠标悬停在关闭按钮上时，提示“关闭”
        private void button2_MouseEnter(object sender, EventArgs e)
        {
            ToolTip p = new ToolTip();
            p.ShowAlways = true;
            p.SetToolTip(this.button2, "关闭");
        }

        #endregion

        #region 点击“顺序签名”和“聚合签名”单选按钮
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            groupBox7.Visible = false;

            groupBox1.Visible = true;
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            groupBox7.Visible = true;

            groupBox1.Visible = false;
        }
        #endregion

        #region 点击“顺序签名”和“聚合签名”单选框式重新加载数据
        private void radioButton3_Click(object sender, EventArgs e)
        {
            clear();
        }

        private void radioButton4_Click(object sender, EventArgs e)
        {
            try
            {
                //当再次选择时避免再次clear（），因为在点击“顺序签名”按钮时已经清空过。此处再次清空会因为数据为空而报错
                clear();
            }
            catch
            {
            }
            userdate();

        }
        #endregion
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            setIP sip = new setIP();
            sip.ShowDialog();
        }

        private void 用户管理_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            xf xf = new xf();

            xf.Show();
        }

        private void mainForm_LocationChanged(object sender, EventArgs e)
        {
            
        }
    }
}