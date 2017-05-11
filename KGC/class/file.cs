using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace KGC
{
    class file
    {
        /// <summary>
        /// 将数据写入指定文件
        /// </summary>
        /// <param name="fsname">文件名字</param>
        /// <param name="mag">要写入的消息</param>
        public void writer(string fsname, string mag)
        //在当前目录建立文件
        {
            FileStream fs = File.Create(fsname);
            //初始化StreamWriter类实例
            StreamWriter sw = new StreamWriter((System.IO.Stream)fs);
            //写入数据
            sw.WriteLine(mag);
            sw.Close();
        }
        /// <summary>
        /// 读取指定文件的内容
        /// </summary>
        /// <param name="fsname">指定的文件路径</param>
        /// <returns>文件内容</returns>
        public string reader(string fsname)        
        {
            //以相对路径的方法构造新的StreamReader 对象
            StreamReader sr = File.OpenText(fsname);
            //用ReadToEnd方法将文件中的所有数据读入到字符串read中
            string read = sr.ReadToEnd();
            //关闭文件很重要
            sr.Close();
            return read;
        }
    }
}
