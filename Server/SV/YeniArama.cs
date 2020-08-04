using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class YeniArama : Form
    {
        SoundPlayer sp;
        public YeniArama(string numara, string cagriTipi, string kurbanIsmi)
        {
            InitializeComponent();
            FormClosing += Bildirim_FormClosing;
            label1.Text = "Numara: " + numara;
            label2.Text = "Çağrı Tipi: " + cagriTipi;
            label3.Text = "Kurban İsmi: " + kurbanIsmi;
            Screen ekran = Screen.FromPoint(Location);
            Location = new Point(ekran.WorkingArea.Right - Width, ekran.WorkingArea.Bottom - Height);
            sp = new SoundPlayer("notify.wav"); sp.Play();
        }
        private void Bildirim_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (sp != null) { sp.Stop(); sp.Dispose(); }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Close();
        }
    }
}
