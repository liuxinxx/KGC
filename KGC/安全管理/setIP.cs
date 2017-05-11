using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using CCWin;
namespace KGC
{
    public partial class setIP : SkinMain
    {
        public setIP()
        {
            InitializeComponent();
            TextBox.CheckForIllegalCrossThreadCalls = false;
            
        }
        private void button1_Click(object sender, EventArgs e)
        {
            #region 检测IP是否合法并将合法IP端口信息写入ipMsg.txt文件中
            IPAddress myIP;
            string ip = textBox1.Text.Trim() + "." + textBox2.Text.Trim() + "." + textBox3.Text.Trim() + "." + textBox4.Text.Trim();
            string port = textBox5.Text.Trim();
            if (IPAddress.TryParse(ip, out myIP))
            {
                MessageBox.Show("当前IP为：" + ip + "\n\n" + "端口号为：" + port, "重置IP成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                string ipStr = textBox1.Text.Trim() + "." + textBox2.Text.Trim() + "." + textBox3.Text.Trim() + "." + textBox4.Text.Trim() + ":" + textBox5.Text.Trim();
                //自己写的文件读写类
                file wd = new file();
                //writer()函数第一个参数为文件名字、第二个为要写如文件的名字
              // wd.writer("..\\..\\..\\..\\ipMsg.txt", ipStr);
               wd.writer("ipMsg.txt", ipStr);
            }
            else
            {
                MessageBox.Show("IP非法！请重新设置");

            }
            #endregion
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void setIP_Load(object sender, EventArgs e)
        {
            #region IP和端口信息的读取和档文件ipMsg.txt不存在时的初始化
            //读取ipMsg.txt中的IP配置
            try
            {

                string a1, a2, a3, a4, port;
                a1 = a2 = a3 = a4 = port = "";
                int b1, b2, b3, b4, i;
                b1 = b2 = b3 = b4 = i = 0;
                //自己写的文件读写类
                file re = new file();
                //reader()为返回从指定文件都出来的数据，其参数为文件路径
                //string read = re.reader("..\\..\\..\\..\\ipMsg.txt");
                //
                string read = re.reader("ipMsg.txt");
                int length = read.Length;

                while (true)
                {
                    if (b1 == 0 && b2 == 0 && b3 == 0)
                    {
                        if (read[i].ToString() == ".")
                        {
                            b1 = 1;
                            i++;

                        }
                        else
                            a1 += read[i];

                    }
                    if (b1 == 1 && b2 == 0 && b3 == 0)
                    {
                        if (read[i].ToString() == ".")
                        {
                            b2 = 1;
                            i++;
                        }
                        else
                            a2 += read[i];

                    }
                    if (b1 == 1 && b2 == 1 && b3 == 0)
                    {
                        if (read[i].ToString() == ".")
                        {
                            b3 = 1;
                            i++;
                        }
                        else
                            a3 += read[i];

                    }
                    if (b1 == 1 && b2 == 1 && b3 == 1 && b4 == 0)
                    {

                        if (read[i].ToString() == ":")
                        {
                            b4 = 1;
                            i++;
                        }
                        else
                            a4 += read[i];

                    }
                    if (b1 == 1 && b2 == 1 && b3 == 1 && b4 == 1)
                    {

                        if (i == length)
                            break;
                        else
                            port += read[i];
                    }
                    i++;

                }
                textBox1.Text = a1;
                textBox2.Text = a2;
                textBox3.Text = a3;
                textBox4.Text = a4;
                textBox5.Text = port;

            }
            //当文件不存在的时候新建初并始化ipMsg.txt
            catch
            {
                string ipStr = textBox1.Text.Trim() + "." + textBox2.Text.Trim() + "." + textBox3.Text.Trim() + "." + textBox4.Text.Trim() + ":" + textBox5.Text.Trim();
                //自己写的文件读写类
                file wd = new file();
                //writer()函数第一个参数为文件名字、第二个为要写如文件的名字
                wd.writer("ipMsg.txt", ipStr);
                //wd.writer("..\\..\\..\\..\\ipMsg.txt", ipStr);

            }
            #endregion
        }

        private void setIP_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region 鼠标拖动窗体
        Point mouse_offset;
        private void panel2_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_offset = new Point(-e.X, -e.Y);
        }

        private void panel2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouse_offset.X, mouse_offset.Y);
                Location = mousePos;
            }
        } 
        #endregion

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
