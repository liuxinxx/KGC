using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient;
using CCWin;
namespace KGC
{
    public partial class userPK : SkinMain
    {
        public userPK()
        {
            InitializeComponent();
        }
        #region 将数据库数据填充到表中，此处有待改进
        private void userpkdate()
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
                MessageBox.Show("服务器“userpk窗体”异常 1:" + e.Message);
            }
            //使用SqlCommand提交查询命令
            string selStr = "select ID as 用户ID,userpk as 用户名 from userpk";
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
        private void button1_Click(object sender, EventArgs e)
        {

            this.Close();
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

        private void button2_Click(object sender, EventArgs e)
        {
           
        }

        private void userPK_Load(object sender, EventArgs e)
        {
            userpkdate();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            userpkdate();
        }

        
        
    }
}
