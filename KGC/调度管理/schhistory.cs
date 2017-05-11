using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Drawing;
using CCWin;
namespace KGC
{
    public partial class schhistory : SkinMain
    {
        public schhistory()
        {
            InitializeComponent();
        }

        private void schhistory_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            Initdate();

        }
        public static string str = "";
        public static string sql = ss();
        public static string ss()
        {
            getSqlStr s = new getSqlStr();
            return s.sqlstr();
        }
        private void Initdate()
        {
            //连接数据库
            SqlConnection myCon = new SqlConnection();
            try
            {
                myCon.ConnectionString = sql;
                myCon.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show("服务器“schhistory窗体”异常 1:" + e.Message);
            }


            //使用SqlCommand提交查询命令
            string selStr = "select ID as ID,cyuser as 参与用户名,first as 提出签名,begintime as 开始时间,time as 有效时长,endtime as 结束时间 ,ingtime as 总耗时,message as 消息,sequence as 签名顺序 ,note as 备注 from history";
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
        private void cx()
        {
            //连接数据库
            SqlConnection myCon = new SqlConnection();
            try
            {
                myCon.ConnectionString = sql;
                myCon.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show("服务器“schhistory窗体”异常 1:" + e.Message);
            }
            string selStr="";
            string eee = "select ID as ID,cyuser as 参与用户名,first as 提出签名,begintime as 开始时间,time as 有效时长(min),endtime as 结束时间 ,ingtime as 总耗时,message as 消息,sequence as 签名顺序 ,note as 备注 from history";
            //comboBox1.SelectedIndex
            //==1 查询参与用户
            //==2 根据时间查询    comboBox1.Items[comboBox1.SelectedIndex].ToString()
            if (comboBox1.SelectedIndex == 1)
            {
                MessageBox.Show(textBox1.Text.Trim());
                selStr = eee + " where cyuser like '%" +textBox1.Text.Trim()+"%'";
                MessageBox.Show(selStr);
            }
            if (comboBox1.SelectedIndex == 2)
            {
 
            }
            //使用SqlCommand提交查询命令
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
        
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            textBox1.Text = dateTimePicker1.Text;
        }

        private void dataGridView1_Click(object sender, EventArgs e)
        {
            try
            {
                str = this.dataGridView1.SelectedCells[0].Value.ToString();
            }
            catch (Exception )//当表中没有数据时的异常捕捉
            {

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (str != "")
            {
                if (MessageBox.Show("您确定要删除该条记录吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (str != "")
                    {
                        SqlConnection con = new SqlConnection();
                        con.ConnectionString = sql;
                        con.Open();
                        SqlCommand cmd = new SqlCommand("delete from history where ID ='" + str + "'", con);
                        cmd.Connection = con;
                        cmd.ExecuteNonQuery();
                        con.Close();
                        Initdate();
                        MessageBox.Show("删除成功！");
                        str = "";
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择要删除的行！");

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ExportToExcel d = new ExportToExcel();
            d.OutputAsExcelFile(dataGridView1);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (comboBox1.Text == "时间")
            {
                dateTimePicker1.Visible = true;
            }
            else
            {
                dateTimePicker1.Visible = false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        #region 鼠标拖动窗体
        Point mouse_offset;
        private void panel2_MouseDown_1(object sender, MouseEventArgs e)
        {
            mouse_offset = new Point(-e.X, -e.Y);
        }

        private void panel2_MouseMove_1(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouse_offset.X, mouse_offset.Y);
                Location = mousePos;
            }
        } 
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            cx();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

    }
}
