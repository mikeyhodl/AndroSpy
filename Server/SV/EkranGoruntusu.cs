using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class EkranGoruntusu : Form
    {
        Socket sck = default; public string ID;
        public int max = 0;
        public EkranGoruntusu(Socket sock , string id)
        {
            InitializeComponent();
            ID = id;
            sck = sock;
        }
        public void resim()
        {
            Invoke((MethodInvoker)delegate
            {
                try
                {
                    NetworkStream networkStream = new NetworkStream(sck);
                    BinaryReader binaryReader = new BinaryReader(networkStream);
                    int thisRead = 0;
                    int blockSize = 1024;
                    byte[] dataByte = new byte[blockSize];
                    using (MemoryStream ms = new MemoryStream())
                    {

                        lock (this)
                        {
                            while (true)
                            {
                                thisRead = binaryReader.Read(dataByte, 0, blockSize);//networkStream.Read(dataByte, 0, blockSize);
                                ms.Write(dataByte, 0, thisRead);
                                /*
                                Application.DoEvents();
                                label3.Text = ms.ToArray().Length.ToString() + " byte";
                                try
                                {
                                    decimal max_ = max;
                                    decimal receiv = ms.ToArray().Length;
                                    int per = Convert.ToInt32((receiv / max_) * 100);
                                    progressBar1.Value = per;
                                    label4.Text = "Alınıyor %" + per.ToString();
                                }
                                catch (Exception) { }
                                */
                                if (ms.ToArray().Length == max) { break; } //break if all data is received.
                            }
                            File.WriteAllBytes("ss.png", ms.ToArray());
                            pictureBox1.Image = (Bitmap)new ImageConverter().ConvertFrom(Form1.Decompress(ms.ToArray())); //Image.FromStream(ms);
                            //label4.Text = "Alındı %" + progressBar1.Value.ToString();
                            button1.Enabled = true;

                            //binaryReader.Close();
                            //binaryReader.Dispose();
                            networkStream.Flush();
                            //networkStream.Close();
                            //networkStream.Dispose();

                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                byte[] veri = Encoding.UTF8.GetBytes("EKRANGORUNTUSU|");
                Gonderici.Send(sck, veri, 0, veri.Length, 59999);
            }
            catch (Exception) { }
        }
    }
}
