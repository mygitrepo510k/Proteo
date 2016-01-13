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

namespace MWF.Mobile.Android.Controls
{
    public class BindableProgress
    {
        private readonly Context _context;
        public BindableProgress(Context context)
        {
            _context = context;
        }

        private ProgressDialog _dialog;

        public string Title { get; set; }
        public string Message { get; set; }

        public bool Visible
        {
            get { return _dialog != null; }
            set
            {
                if (value == Visible)
                    return;
                
                if (value)
                {
                    _dialog = new ProgressDialog(_context);
                    _dialog.SetTitle(Title);
                    _dialog.SetMessage(Message);
                    _dialog.Show();
                    _dialog.SetCanceledOnTouchOutside(false);
                }
                else
                {
                    _dialog.Hide();
                    _dialog = null;
                }
            }
        }
    }
}