using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using CCWin;
namespace KGC
{
    public partial class killms : SkinMain
    {

        #region 给无边框加阴影效果
        private const int CS_DropSHADOW = 0x20000;
        private const int GCL_STYLE = (-26);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);
        #endregion

        #region 窗体动画
        /// 窗体动画函数
        /// </summary>
        /// <param name="hwnd">指定产生动画的窗口的句柄</param>
        /// <param name="dwTime">指定动画持续的时间</param>
        /// <param name="dwFlags">指定动画类型，可以是一个或多个标志的组合。</param>
        /// <returns></returns>
        [DllImport("user32")]
        private static extern bool AnimateWindow(IntPtr hwnd, int dwTime, int dwFlags);

        //下面是可用的常量，根据不同的动画效果声明自己需要的

        private const int AW_HOR_POSITIVE = 0x0001;//自左向右显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志
        private const int AW_HOR_NEGATIVE = 0x0002;//自右向左显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志
        private const int AW_VER_POSITIVE = 0x0004;//自顶向下显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志
        private const int AW_VER_NEGATIVE = 0x0008;//自下向上显示窗口，该标志可以在滚动动画和滑动动画中使用。使用AW_CENTER标志时忽略该标志该标志
        private const int AW_CENTER = 0x0010;//若使用了AW_HIDE标志，则使窗口向内重叠；否则向外扩展
        private const int AW_HIDE = 0x10000;//隐藏窗口
        private const int AW_ACTIVE = 0x20000;//激活窗口，在使用了AW_HIDE标志后不要使用这个标志
        private const int AW_SLIDE = 0x40000;//使用滑动类型动画效果，默认为滚动动画类型，当使用AW_CENTER标志时，这个标志就被忽略
        private const int AW_BLEND = 0x80000;//使用淡入淡出效果 
        #endregion

        public killms()
        {
            InitializeComponent();

         
           
        }

        private void killms_Load(object sender, EventArgs e)
        {
            ////这三行代码可以实现从显示器右下角从下滑入效果
            //int x = Screen.PrimaryScreen.WorkingArea.Right - this.Width;
            //int y = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            //this.Location = new Point(x, y);//设置窗体在屏幕右下角显示

            //加载窗体时的从下划入效果,300毫秒为动画时间
            AnimateWindow(this.Handle, 300, AW_SLIDE | AW_ACTIVE | AW_VER_NEGATIVE);

        }
        Point mouse_offset;
        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

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
        int i = 2;
        private void timer1_Tick(object sender, EventArgs e)
        {           
            if (i == -1)
            {
                timer1.Stop();
                this.Close();
            }
            button1.Text = "确定(" + i-- + ")";
        }

        private void killms_FormClosing(object sender, FormClosingEventArgs e)
        {
            //关窗口时的淡入淡出效果，200为淡出的动画时间
            AnimateWindow(this.Handle, 200, AW_BLEND | AW_HIDE);           
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

    }
}
