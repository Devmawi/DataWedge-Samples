using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppVolumeChange
{
    public class AppAudioManagerCB: AudioManager.AudioPlaybackCallback
    {
        public DateTime LastChange { get; set; } = DateTime.Now;
        public AudioManager m { get; set; }
        public AppAudioManagerCB(AudioManager am) {
            m = am;
        }

        public override void OnPlaybackConfigChanged(IList<AudioPlaybackConfiguration> configs)
        {
            var list = configs.ToArray();
            if(list.Length > 0)
            {
                var config = list[0];
                if(config.AudioAttributes.Usage == AudioUsageKind.Notification)
                {
                    var desc = config.AudioAttributes.VolumeControlStream;
                    if (!m.IsStreamMute(desc) && (DateTime.Now - LastChange).TotalMilliseconds > 500)
                    {
                        var sp = m.SpeakerphoneOn;
                        LastChange = DateTime.Now;
                    }
                   
               
                }


            }
            base.OnPlaybackConfigChanged(configs);
        }
    }
}