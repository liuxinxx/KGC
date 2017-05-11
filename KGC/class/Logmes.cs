using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace KGC
{
    
	class Logmes
	{
        public static int x = 0;
        public static int y = 0;
        public static int maxID = 0;//保存当前签名的签名序列号（就是第几次签名）
        public static string first = "";//发起签名的用户，即第一个签名的用户
        public static DateTime begintime = System.DateTime.Now;//有效时间        
        public static DateTime endtime = System.DateTime.Now;//有效时间
        public static string time = "";//有效时间
        public static string ingtime = "";//有效时间
        public static string sequence = "";//顺序签名的序列
        public static string message = "";//消息
        public static string result = "";//签名结果成功还是失败
        public static string note = "签名成功";//注释
        /// <summary>
        /// 最后将签名结果插入到数据库
        /// </summary>
        public static void inster()
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
                //MessageBox.Show(maxID + "'\r\n,'"+sequence+"'\r\n,'" + first + "'\r\n," + begintime + "\r\n," + endtime +"\r\n,'" +message + "'\r\n,'" + sequence+"'\r\n,'"+time+"'\r\n,'" + note+"'\r\n,'"+ingtime+"'");
                string ist = "insert into history  values(" + maxID + ",'" + sequence + "','" + first + "','" + begintime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + endtime.ToString("yyyy-MM-dd HH:mm:ss") + "','" + message + "','" + sequence + "','" + time + "','" + note + "','" + ingtime + "')";
                SqlCommand insert = new SqlCommand(ist, mysql);

                insert.ExecuteNonQuery();//执行sql语句并返回影响的行数
            }
            catch (Exception t)
            {

                MessageBox.Show(t.ToString());
            }
 
        }
        /// <summary>
        /// 已重载.计算两个日期的时间间隔,返回的是时间间隔的日期差的绝对值.
        /// </summary>
        /// <param name="DateTime1">第一个日期和时间</param>
        /// <param name="DateTime2">第二个日期和时间</param>
        /// <returns></returns>
        public static string DateDiff(DateTime DateTime1, DateTime DateTime2)
        {
            string dateDiff = null;
            try
            {
                TimeSpan ts1 = new TimeSpan(DateTime1.Ticks);
                TimeSpan ts2 = new TimeSpan(DateTime2.Ticks);
                TimeSpan ts = ts1.Subtract(ts2).Duration();
                dateDiff = ts.Days.ToString() + "天"
                        + ts.Hours.ToString() + "小时"
                        + ts.Minutes.ToString() + "分钟"
                        + ts.Seconds.ToString() + "秒";
            }
            catch
            {

            }
            return dateDiff;
        }
       
	}
}
