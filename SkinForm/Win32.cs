using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Collections;

/// <summary>
/// Wind32API����
/// </summary>
public class Win32
{
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;
    public const int WS_EX_LAYERED = 0x00080000;

    [StructLayout(LayoutKind.Sequential)]
    public struct Size
    {
        public Int32 cx;
        public Int32 cy;

        public Size(Int32 x, Int32 y)
        {
            cx = x;
            cy = y;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct BLENDFUNCTION
    {
        public byte BlendOp;
        public byte BlendFlags;
        public byte SourceConstantAlpha;
        public byte AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Point
    {
        public Int32 x;
        public Int32 y;

        public Point(Int32 x, Int32 y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public const byte AC_SRC_OVER = 0;
    public const Int32 ULW_ALPHA = 2;
    public const byte AC_SRC_ALPHA = 1;

    /// <summary>
    /// ��������ʾ
    /// </summary>
    public const Int32 AW_HOR_POSITIVE = 0x00000001;
    /// <summary>
    /// ���ҵ�����ʾ
    /// </summary>
    public const Int32 AW_HOR_NEGATIVE = 0x00000002;
    /// <summary>
    /// ���ϵ�����ʾ
    /// </summary>
    public const Int32 AW_VER_POSITIVE = 0x00000004;
    /// <summary>
    /// ���µ�����ʾ
    /// </summary>
    public const Int32 AW_VER_NEGATIVE = 0x00000008;
    /// <summary>
    /// ��ʹ����AW_HIDE��־����ʹ���������ص������������ڣ�����ʹ����������չ����չ������
    /// </summary>
    public const Int32 AW_CENTER = 0x00000010;
    /// <summary>
    /// ���ش��ڣ�ȱʡ����ʾ����
    /// </summary>
    public const Int32 AW_HIDE = 0x00010000;
    /// <summary>
    /// ����ڡ���ʹ����AW_HIDE��־����ʹ�������־
    /// </summary>
    public const Int32 AW_ACTIVATE = 0x00020000;
    /// <summary>
    /// ʹ�û������͡�ȱʡ��Ϊ�����������͡���ʹ��AW_CENTER��־ʱ�������־�ͱ�����
    /// </summary>
    public const Int32 AW_SLIDE = 0x00040000;
    /// <summary>
    /// ͸���ȴӸߵ���
    /// </summary>
    public const Int32 AW_BLEND = 0x00080000;

    /// <summary>
    /// ִ�ж���
    /// </summary>
    /// <param name="whnd">�ؼ����</param>
    /// <param name="dwtime">����ʱ��</param>
    /// <param name="dwflag">�����������</param>
    /// <returns>boolֵ�������Ƿ�ɹ�</returns>
    [DllImport("user32")]
    public static extern bool AnimateWindow(IntPtr whnd, int dwtime, int dwflag);

    /// <summary>
    /// <para>�ú�����ָ������Ϣ���͵�һ���������ڡ�</para>
    /// <para>�˺���Ϊָ���Ĵ��ڵ��ô��ڳ���ֱ�����ڳ���������Ϣ�ٷ��ء�</para>
    /// <para>������PostMessage��ͬ����һ����Ϣ���͵�һ���̵߳���Ϣ���к��������ء�</para>
    /// return ����ֵ : ָ����Ϣ����Ľ���������������͵���Ϣ��
    /// </summary>
    /// <param name="hWnd">Ҫ������Ϣ���Ǹ����ڵľ��</param>
    /// <param name="Msg">��Ϣ�ı�ʶ��</param>
    /// <param name="wParam">����ȡ������Ϣ</param>
    /// <param name="lParam">����ȡ������Ϣ</param>
    [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SendMessageA")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32")]
    public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo); 

    [DllImport("gdi32.dll")]
    public static extern int CreateRoundRectRgn(int x1, int y1, int x2, int y2, int x3, int y3);

    [DllImport("user32.dll")]
    public static extern int SetWindowRgn(IntPtr hwnd, int hRgn, Boolean bRedraw);

    [DllImport("user32", EntryPoint = "GetWindowLong")]
    public static extern int GetWindowLong(
        IntPtr hwnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(
        IntPtr hwnd, int nIndex, int dwNewLong);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr GetDC(IntPtr hWnd);

    [DllImport("gdi32.dll", ExactSpelling = true)]
    public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObj);

    [DllImport("user32.dll", ExactSpelling = true)]
    public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int DeleteDC(IntPtr hDC);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int DeleteObject(IntPtr hObj);

    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern int UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pptSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

    [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
    public static extern IntPtr ExtCreateRegion(IntPtr lpXform, uint nCount, IntPtr rgnData);
}