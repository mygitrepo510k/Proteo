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

        private enum _soundTracks { testSound = Resource.Raw.Kalimba, refreshSound, clickSound };

        public void Play(_soundTracks sound)
        {
            //TODO: Implement some form of error checking to make sure its a valid enum.
            int soundPath = (int)sound;
            
            _player = MediaPlayer.Create(Application.Context, soundPath);
           _player.Start();
        }
    }
}