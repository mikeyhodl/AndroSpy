using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class DownloadManager : Form
    {
        Socket sck;
        public string ID;
        public DownloadManager(Socket socket, string ident)
        {
            InitializeComponent();
            sck = socket;
            ID = ident;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            byte[] inecek = Encoding.UTF8.GetBytes("DOWNFILE|" + textBox1.Text + "|" + textBox2.Text + "|");
            Gonderici.Send(sck, inecek, 0, inecek.Length, 59999);
        }
    }
}
