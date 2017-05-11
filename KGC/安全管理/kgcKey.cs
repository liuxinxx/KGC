using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CCWin;
namespace KGC
{
    public partial class kgcKey : SkinMain
    {
        desKey des = new desKey();
        public kgcKey()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
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

        private void kgcKey_Load(object sender, EventArgs e)
        {
            file file = new file();
            textBox1.Text =des.Decrypt(file.reader("pk.txt"),"abcdefgh");
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        
    }
}
