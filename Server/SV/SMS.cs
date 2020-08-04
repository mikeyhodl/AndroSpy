using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class SMS : Form
    {
        Socket s;
        public SMS(Socket socket, string num)
        {
            InitializeComponent();
            s = socket;
            textBox1.Text = num;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text != "" && textBox2.Text != "")
            {
                try
                {
                    byte[] polat_alemdar = Encoding.UTF8.GetBytes("SMSGONDER|" + textBox1.Text + "=" + textBox2.Text + "=");
                    Gonderici.Send(s, polat_alemdar, 0, polat_alemdar.Length, 59999);
                    MessageBox.Show("SMS gönderme talimatı iletildi.", "SMS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception) { }
            }
        }
    }
}
