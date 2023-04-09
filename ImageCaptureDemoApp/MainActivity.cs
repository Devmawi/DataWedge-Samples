using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Android.Content;
using System.Linq;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Android.Graphics;
using Java.IO;
using Android.Hardware.Lights;
using Org.Json;
using static Android.Icu.Text.ListFormatter;
using System.Drawing.Imaging;
using Android.Media;
using System.Threading.Tasks;
using Symbol.XamarinEMDK;
using static Symbol.XamarinEMDK.EMDKManager;
using Symbol.XamarinEMDK.SimulScan;
using Symbol.XamarinEMDK.Barcode;
using Android.Views.InputMethods;

namespace AutomaricImageCaptureDemoApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, IEMDKListener
    {
        [BroadcastReceiver(Enabled = true, Exported = true)]
        [IntentFilter(new[] { "com.symbol.datawedge.DWDEMO2", "com.symbol.datawedge.api.ACTION", "com.symbol.datawedge.api.NOTIFICATION_ACTION" })]
        private class MainBroadcastReceiver : BroadcastReceiver
        {
            public MainActivity Activity { get; set; }
            public MainBroadcastReceiver() { }
            public override void OnReceive(Context context, Intent intent)
            {
                var bundle = intent.Extras;
                var keys = bundle.KeySet().ToArray();

                if (bundle.ContainsKey("com.symbol.datawedge.api.SWITCH_TO_PROFILE"))
                {
                    var sp = bundle.GetString("com.symbol.datawedge.api.SWITCH_TO_PROFILE");

                    TextView tv = Activity?.FindViewById<TextView>(Resource.Id.textView1);
                    tv.Text = $"Profile: {sp}";

                }else if (bundle.ContainsKey("com.symbol.datawedge.api.NOTIFICATION"))
                {
                    Bundle b = intent.GetBundleExtra("com.symbol.datawedge.api.NOTIFICATION");

                    keys = b.KeySet().ToArray();
                    var status = b.GetString("STATUS");
                    var notificationType = b.GetString("NOTIFICATION_TYPE");
                    var workflowName = b.GetString("selected_workflow_name");

                    if (status == "CAPTURING_STARTED")
                    {
                        Activity.CaptureSessionActive = true;
                    }

                    if (status == "CAPTURING_STOPPED")
                    {
                        Activity.CaptureSessionActive = false;
                    }
                }else
                {
                    keys = bundle.KeySet().ToArray();
                    var data = bundle.GetString("com.symbol.datawedge.data_string");

                    if(data != null)
                    {
                        TextView tv = Activity?.FindViewById<TextView>(Resource.Id.textView2);
                        tv.Text = $"Data: {data}";
                    }
                    

                    var image = bundle.GetString("com.symbol.datawedge.data");

                    if(image != null)
                    {
                        var jsonObj = JsonConvert.DeserializeObject<JArray>(image).First(e => e.Value<string>("imageformat") != null);
                        var uri = jsonObj["uri"].ToString();
                        var width = jsonObj.Value<int>("width");
                        var height = jsonObj.Value<int>("height");
                        int stride = jsonObj.Value<int>("stride");
                        int orientation = jsonObj.Value<int>("orientation");
                        string imageFormat = jsonObj.Value<string>("imageformat") ?? "YUV";

                        var img = GetImage(Activity.ContentResolver, uri);
                        var bitmap = ImageProcessing.GetInstance().GetBitmap(img, imageFormat, orientation, stride, width, height);

                        ImageView imageView = Activity.FindViewById<ImageView>(Resource.Id.imageView1);
                        imageView.SetImageBitmap(bitmap);
                    }

                  
                }
            }

            public byte[] GetImage(ContentResolver resolver, string uri)
            {
                var cursor = resolver.Query(Android.Net.Uri.Parse(uri), null, null, null);
                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                if (cursor != null)
                {
                    cursor.MoveToFirst();
                    baos.Write(cursor.GetBlob(cursor.GetColumnIndex("raw_data")));
                    String nextURI = cursor.GetString(cursor.GetColumnIndex("next_data_uri"));
                    while (nextURI != null && nextURI.Any())
                    {
                        Android.Database.ICursor cursorNextData = resolver.Query(Android.Net.Uri.Parse(nextURI),
                                null, null, null);
                        if (cursorNextData != null)
                        {
                            cursorNextData.MoveToFirst();
                            baos.Write(cursorNextData.GetBlob(cursorNextData.
                                    GetColumnIndex("raw_data")));
                            nextURI = cursorNextData.GetString(cursorNextData.
                                    GetColumnIndex("next_data_uri"));

                            cursorNextData.Close();
                        }

                    }
                    cursor.Close();
                }

                return baos.ToByteArray();
            }
        }

        public bool CaptureSessionActive { get; set; }

        MainPlaybackManagerCallback _mainPlaybackManagerCallback { get; set; }
        AudioManager _audioManager { get; set; }

        private readonly MainBroadcastReceiver _mainBroadcastReceiver = new MainBroadcastReceiver();


        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            // 103 Android.Views.Keycode.ButtonR1 for right trigger
            
            return base.OnKeyDown(keyCode, e);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            this._mainBroadcastReceiver.Activity = this;

            if(_mainPlaybackManagerCallback == null)
            {
                _audioManager = this.GetSystemService(AudioService) as AudioManager;
                _mainPlaybackManagerCallback = new MainPlaybackManagerCallback(this, _audioManager);

            }
            _audioManager.RegisterAudioPlaybackCallback(_mainPlaybackManagerCallback, new Handler());
            var filter = new IntentFilter();
            filter.AddAction("com.symbol.datawedge.api.ACTION");
            filter.AddAction("com.symbol.datawedge.DWDEMO2");
            filter.AddAction("com.symbol.datawedge.api.NOTIFICATION_ACTION");
            filter.AddCategory(Intent.CategoryDefault);
            RegisterReceiver(_mainBroadcastReceiver, filter);

            // Register for notifications - WORKFLOW_STATUS
            Bundle b = new Bundle();
            b.PutString("com.symbol.datawedge.api.APPLICATION_NAME", "com.companyname.automaricimagecapturedemoapp");
            b.PutString("com.symbol.datawedge.api.NOTIFICATION_TYPE", "WORKFLOW_STATUS");
            Intent i = new Intent();
            i.SetAction("com.symbol.datawedge.api.ACTION");
            i.PutExtra("com.symbol.datawedge.api.REGISTER_FOR_NOTIFICATION", b);//(1)
            this.SendBroadcast(i);

            Intent i2 = new Intent();
            i2.SetAction("com.symbol.datawedge.api.ACTION");
            i2.PutExtra("com.symbol.datawedge.api.SWITCH_TO_PROFILE", "DWDemo2");
            SendBroadcast(i2);

            //Intent i3 = new Intent();
            //i3.SetAction("com.symbol.datawedge.api.ACTION");

            //Task.Delay(1000).Wait();

            //i3.PutExtra("com.symbol.datawedge.api.SOFT_SCAN_TRIGGER", "START_SCANNING");
            //SendBroadcast(i3);

            //EMDKResults results = EMDKManager.GetEMDKManager(Android.App.Application.Context, this);
            var btn = FindViewById<Button>(Resource.Id.button1);
            btn.Click += Btn_Click;
            
            base.OnStart();

        }

        private void Btn_Click(object sender, EventArgs e)
        {
            
        }

        protected override void OnStop()
        {
            UnregisterReceiver(_mainBroadcastReceiver);
            _audioManager.UnregisterAudioPlaybackCallback(_mainPlaybackManagerCallback);
            base.OnStop();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnClosed()
        {
            
        }
        private EMDKManager emdkManager { get; set; }

        public Scanner Scanner { get; set; }
        public void OnOpened(EMDKManager p0)
        {
            emdkManager = p0;
            var sm = (BarcodeManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);
            var scanners = sm.SupportedDevicesInfo;
            Scanner = sm.GetDevice(scanners[0]);
            
        }
    }
}
