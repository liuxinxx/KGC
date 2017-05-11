using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace KGC
{
    public partial class xf : Form
    {
        public xf()
        {
            InitializeComponent();
        }


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
        private const int AW_BLEND = 0x80000;
        #endregion

        private void xf_Load(object sender, EventArgs e)
        {

            //300毫秒为动画时间
            ////这三行代码可以实现从显示器右下角从下滑入效果
            //int x = Screen.PrimaryScreen.WorkingArea.Right - this.Width;
            //int y = Screen.PrimaryScreen.WorkingArea.Bottom - this.Height;
            int x = Logmes.x ;
            int y = Logmes.y - this.Height;
            this.Location = new Point(x, y);//设置窗体在屏幕右下角显示
            AnimateWindow(this.Handle, 300, AW_SLIDE | AW_ACTIVE | AW_VER_NEGATIVE);
        }

        private void xf_FormClosing(object sender, FormClosingEventArgs e)
        {
            

            //关窗口时的淡入淡出效果，200为淡出的动画时间
            AnimateWindow(this.Handle, 200, AW_BLEND | AW_HIDE);

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
        }//使用淡入淡出效果 


    }
}
