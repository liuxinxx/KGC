using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace KGC
{
    class getSqlStr
    {
        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        /// <returns>连接字符串</returns>
        public string sqlstr()
        {

            //当前运行程序的文件夹路径
            String currentdir = Directory.GetCurrentDirectory();

            ////连接到数据库文件  绝对路径为F:\C#\KGC\KGC\bin\Debug\data\power.MDF
            //string constr = @"Data Source=.\SQLEXPRESS;AttachDbFilename=" + currentdir + @"\data\power.MDF;Integrated Security=True;Connect Timeout=30;User Instance=True";
            //return constr;

            //链接到本机数据库    
            string str = "Data Source=LIUXIN\\sqlexpress;Initial Catalog=power;Integrated Security=True";
            return str;

        }
    }
}
