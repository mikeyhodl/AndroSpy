using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
            checkBox1.Checked = lines[lines.Length - 1] != "..." ? true : false;
        }
        string[] lines = File.ReadAllLines("settings.tht");
        private void button1_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                using (OpenFileDialog op = new OpenFileDialog()
                {
                    Multiselect = false,
                    Filter = "Ses dalgası (*.wav)|*.wav",
                    Title = "Yeni bağlantı için bir ses dalgası dosyası seçiniz.."
                })
                {
                    lines[lines.Length - 1] = op.ShowDialog() == DialogResult.OK ? op.FileName : "...";
                    File.WriteAllLines("settings.tht", lines);
                    Close();
                }
            }
            else
            {
                lines[lines.Length - 1] = "...";
                Close();
            }
        }
    }
}
