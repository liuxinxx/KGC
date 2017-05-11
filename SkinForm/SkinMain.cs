using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace CCWin
{
    //控件层
    public partial class SkinMain : Form
    {
        //绘制层
        private SkinForm skin;
        public SkinMain()
        {
            InitializeComponent();
            //减少闪烁
            SetStyles();
        }
        #region 减少闪烁
        private void SetStyles()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.DoubleBuffer, true);
            //强制分配样式重新应用到控件上
            UpdateStyles();
            base.AutoScaleMode = AutoScaleMode.None;
        }
        #endregion

        #region 变量属性
        //不显示FormBorderStyle属性
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new FormBorderStyle FormBorderStyle
        {
            get { return base.FormBorderStyle; }
            set { base.FormBorderStyle = FormBorderStyle.None; }
        }

        private bool _skinmobile = true;
        [Category("Skin")]
        [Description("窗体是否可以移动")]
        [DefaultValue(typeof(bool), "true")]
        public bool SkinMobile
        {
            get { return _skinmobile; }
            set
            {
                if (_skinmobile != value)
                {
                    _skinmobile = value;
                }
            }
        }
        #endregion

        #region 重载事件
        //Show或Hide被调用时
        protected override void OnVisibleChanged(EventArgs e)
        {
            if (Visible)
            {
                //启用窗口淡入淡出
                if (!DesignMode)
                {
                    //淡入特效
                    Win32.AnimateWindow(this.Handle, 150, Win32.AW_BLEND | Win32.AW_ACTIVATE);
                }
                //判断不是在设计器中
                if (!DesignMode && skin == null)
                {
                    skin = new SkinForm(this);
                    skin.Show(this);
                }
                base.OnVisibleChanged(e);
            }
            else
            {
                base.OnVisibleChanged(e);
                Win32.AnimateWindow(this.Handle, 150, Win32.AW_BLEND | Win32.AW_HIDE);
            }
        }

        //窗体关闭时
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            //先关闭阴影窗体
            if (skin != null)
            {
                skin.Close();
            }
            //在Form_FormClosing中添加代码实现窗体的淡出
            Win32.AnimateWindow(this.Handle, 150, Win32.AW_BLEND | Win32.AW_HIDE);
        }

        //控件首次创建时被调用
        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            SetReion();
        }

        //圆角
        private void SetReion()
        {
            using (GraphicsPath path =
                    GraphicsPathHelper.CreatePath(
                    new Rectangle(Point.Empty, base.Size), 6, RoundStyle.All, true))
            {
                Region region = new Region(path);
                path.Widen(Pens.White);
                region.Union(path);
                this.Region = region;
            }
        }

        //改变窗体大小时
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            SetReion();
        }

        //移动窗体
        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (SkinMobile)
            {
                //释放鼠标焦点捕获
                Win32.ReleaseCapture();
                //向当前窗体发送拖动消息
                Win32.SendMessage(this.Handle, 0x0112, 0xF011, 0);
                base.OnMouseUp(e);
            }
            base.OnMouseDown(e);
        }
        #endregion

        #region 允许点击任务栏最小化
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;  // Winuser.h中定义
                CreateParams cp = base.CreateParams;
                cp.Style = cp.Style | WS_MINIMIZEBOX;   // 允许最小化操作
                return cp;
            }
        }
        #endregion
    }
}
