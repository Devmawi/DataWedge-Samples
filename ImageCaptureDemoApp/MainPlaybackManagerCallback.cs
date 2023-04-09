using Android.App;
using Android.Content;
using Android.InputMethodServices;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using OpenQA.Selenium.Appium.Android;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomaricImageCaptureDemoApp
{
    public class MainPlaybackManagerCallback: AudioManager.AudioPlaybackCallback
    {
        public MainActivity Activity { get; set; }
        public DateTime LastChange { get; set; } = DateTime.Now;
        public AudioManager AudioManager { get; set; }
        public MainPlaybackManagerCallback(MainActivity activity, AudioManager audioManager)
        {
            Activity = activity;
            AudioManager = audioManager;
        }
        public override void OnPlaybackConfigChanged(IList<AudioPlaybackConfiguration> configs)
        {
            var list = configs.ToArray();
            if (list.Length > 0)
            {
                var config = list[0];
                if (config.AudioAttributes.Usage == AudioUsageKind.Notification)
                {
                    var desc = config.AudioAttributes.VolumeControlStream;
                    if (!AudioManager.IsStreamMute(desc) && (DateTime.Now - LastChange).TotalMilliseconds > 250 && Activity.CaptureSessionActive)
                    {
                        LastChange = DateTime.Now;
                        //Activity.Scanner.CancelRead();
                        //Intent i = new Intent();
                        //i.SetAction("com.symbol.datawedge.api.ACTION");
                        ////i.PutExtra("com.symbol.datawedge.api.SOFT_TRIGGER", "TOGGLE_SCANNING");
                        //i.PutExtra("SEND_RESULT", "true");
                        //i.PutExtra("COMMAND_IDENTIFIER", "123456789");

                        //i.PutExtra("com.symbol.datawedge.api.SOFT_SCAN_TRIGGER", "STOP_SCANNING");
                        //Activity.SendBroadcast(i);

                        //BaseInputConnection mInputConnection = new BaseInputConnection(Activity.Window.DecorView.RootView, true);

                        //mInputConnection.SendKeyEvent(new Android.Views.KeyEvent(KeyEventActions.Down, Android.Views.Keycode.ButtonR1));
                        //Task.Run(()=>
                        //{
                        //    //Activity.DispatchKeyEvent(new Android.Views.KeyEvent(KeyEventActions.Down, Android.Views.Keycode.ButtonR1));
                        //    //Instrumentation inst = new Instrumentation();
                        //    //inst.SendKeyDownUpSync(Android.Views.Keycode.ButtonR1);

                        //    var windowService = (IWindowManager)Activity.GetSystemService(Service.WindowService);

                        //}).Wait();
                    }


                }
            }

            base.OnPlaybackConfigChanged(configs);
        }
    }
}