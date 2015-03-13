﻿using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Portable;

namespace MWF.Mobile.Core.ViewModels
{


    public class CameraImageViewModel : MvxViewModel
    {

        #region Private Members

        private Image _image;
        private MvxCommand _deleteCommand;
        private MvxCommand _displayCommand;
        private CameraViewModel _parentCameraViewModel;

        #endregion

        #region Construction

        public CameraImageViewModel(Image image, CameraViewModel camera)
        {
            _image = image;
            _parentCameraViewModel = camera;
        }

        #endregion

        #region public properties

        public byte[] Bytes
        {
            get { return _image.Bytes; }
        }

        public Image Image
        {
            get { return _image; }
            set 
            { 
                _image = value;
                RaisePropertyChanged(() => Bytes);
            }
        }


        public System.Windows.Input.ICommand DisplayCommand
        {
            get { return (_displayCommand = _displayCommand ?? new MvxCommand(Display)); }
        }

        
        public System.Windows.Input.ICommand DeleteCommand
        {
            get { return (_deleteCommand = _deleteCommand ?? new MvxCommand(Delete)); }
        }


        #endregion

        #region Private Methods

        private void Delete()
        {
            Mvx.Resolve<IUserInteraction>().Confirm("Are you sure you want to delete this picture?", isConfirmed =>
            {
                if (isConfirmed)
                {
                    _parentCameraViewModel.Delete(this);
                }
            }, "Delete Picture", "Delete", "Cancel");
        }

        private void Display()
        {
            Mvx.Resolve<ICustomUserInteraction>().PopUpImage(_image.Bytes, null, null, null, "Close");
        }

        #endregion


    }
}