using System;
using System.Windows.Forms;

namespace SV
{
    public partial class Port : Form
    {
        public Port()
        {
            InitializeComponent();
            button1.DialogResult = DialogResult.OK;
            //MessageBox.Show(int.MaxValue.ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
          
            Form1.port_no = (int)numericUpDown1.Value;
            Close();
        }
    }
}
