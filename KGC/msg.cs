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
    public partial class msg : SkinMain
    {
        
        public msg()
        {
            InitializeComponent();
        }

        private void msg_Load(object sender, EventArgs e)
        {
            
        }
        public  void dd(string str)
        {
            label1.Text = str;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
