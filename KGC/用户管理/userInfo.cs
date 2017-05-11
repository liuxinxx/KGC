using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;
using CCWin;
namespace KGC
{
    public partial class userInfo : SkinMain
    {
        public userInfo()
        {
            InitializeComponent();
        }
        //用户点击的行


        public static string sql = ss();
        public static string ss()
        {
            getSqlStr s = new getSqlStr();
            return s.sqlstr();
        }
        /// <summary>
        /// 在DataGridView空间中显示记录
        /// </summary>
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
                MessageBox.Show("服务器“userinfo窗体”异常 1:" + e.Message);
            }


            //使用SqlCommand提交查询命令
            string selStr = "select ID as 用户ID,username as 用户名,userhd as 硬盘序列号,usercpu as cpu序列号,zctime as 注册时间 from userdata";
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
        private void userInfo_Load(object sender, EventArgs e)
        {
            Initdate();
        }

        private void updateBut_Click(object sender, EventArgs e)
        {
            //Initdate();
        }
        public static string str = "";
        /// <summary>
        /// 获取当前用户点击的行的第一列数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void deleteBut_Click(object sender, EventArgs e)
        {
            if (str != null)
            {
                if (MessageBox.Show("您确定要删除该用户吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    if (str != "")
                    {
                        SqlConnection con = new SqlConnection();
                        con.ConnectionString = sql;
                        con.Open();
                        SqlCommand cmd = new SqlCommand("delete from userdata where ID ='" + str + "'", con);
                        SqlCommand del = new SqlCommand("delete from userpk where ID ='" + str + "'", con);
                        cmd.Connection = con;
                        cmd.ExecuteNonQuery();
                        del.Connection = con;
                        del.ExecuteNonQuery();
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

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        /// <summary>
        /// 清除按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = null;
            textBox2.Text = null;
        }
        /// <summary>
        /// 退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// 添加用户，按钮的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addBut_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("用户ID为空！");
            }
            if (textBox2.Text == "")
            {
                MessageBox.Show("用户名为空！");
            }

            if (isSameRecord() == true)
            {
                return;
            }
            if (textBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    SqlConnection con = new SqlConnection();
                    con.ConnectionString = sql;
                    con.Open();
                    string ist = "insert into userdata ([ID],[username]) values('" + textBox1.Text.Trim() + "','" + textBox2.Text.Trim() + "')";
                    SqlCommand insert = new SqlCommand(ist, con);

                    int t = insert.ExecuteNonQuery();//执行sql语句并返回影响的行数
                    MessageBox.Show("信息添加成功！");
                    textBox1.Text = "";
                    textBox2.Text = "";
                    con.Close();//关闭数据库连接 
                    Initdate();
                }
                catch (Exception )
                {

                    MessageBox.Show("用户ID不能重复！");
                    textBox1.Text = "";
                }
            }
            else
            {
                return;
            }

        }
        /// <summary>
        /// 判断是否存在相同记录，是返回true
        /// </summary>
        /// <returns></returns>
        private bool isSameRecord()
        {
            SqlConnection con = new SqlConnection();
            con.ConnectionString = sql;
            con.Open();
            string name = "";
            string ID = "";
            ID = this.textBox1.Text.Trim();
            name = this.textBox2.Text.Trim();
            string selstr = "select * from userdata where ID='" + ID + "' or username = '" + name + "'";
            SqlCommand sel = new SqlCommand(selstr, con);
            //获取数据适配器
            SqlDataAdapter da = new SqlDataAdapter();
            da.SelectCommand = sel;
            //填充dataset
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, "info");
            }
            catch (Exception ex)
            {

                MessageBox.Show("服务器“userinfo窗体”异常 2:" + ex.Message);
            }
            con.Close();//关闭数据库连接 
            con.Dispose();//释放资源
            if (ds.Tables["info"].Rows.Count > 0)
            {
                MessageBox.Show("用户名或用户ID已存在！", "警告", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.Value is string)
                e.Value = e.Value.ToString().Trim();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            ExportToExcel d = new ExportToExcel();
            d.OutputAsExcelFile(dataGridView1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        #region 鼠标拖动窗体按钮
        Point mouse_offset;
        private void panel3_MouseDown(object sender, MouseEventArgs e)
        {
            mouse_offset = new Point(-e.X, -e.Y);
        }

        private void panel3_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePos = Control.MousePosition;
                mousePos.Offset(mouse_offset.X, mouse_offset.Y);
                Location = mousePos;
            }
        } 
        #endregion

      

    }
}
