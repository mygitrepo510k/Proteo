using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MWF.Mobile.Core.Portable;
using Android.Media;

namespace MWF.Mobile.Android.Portable
{
    public class Sound
        : ISound
    {
        private MediaPlayer _player;

        public void Play(Enum sound)
        {
            //TODO: Implement some form of enum.
            
            _player = MediaPlayer.Create(Application.Context, Resource.Raw.Kalimba);
           _player.Start();
        }
    }
}