﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.Models;
using Cirrious.CrossCore;
using MWF.Mobile.Core.Services;

namespace MWF.Mobile.Core.ViewModels
{

    public abstract class NavData
    {
        public NavData()
        {
            OtherData = new Dictionary<string, object>();
        }

        public abstract object GetData();
        public abstract void SetData(object data);

        public Guid NavGUID { get; set; }
        public Dictionary<string, object> OtherData { get; set; }

        public void Reinflate()
        {
            var navService = Mvx.Resolve<INavigationService>();
            if (navService!=null)
            {
                navService.PopulateNavData(this);
            }
        }
    }

    public class NavData<T>: NavData where T: class, IBlueSphereEntity
    {

        #region public properties

       
        public T Data { get; set; }
       

        #endregion

        #region public methods

        public override object GetData()
        {
            return Data;
        }

        public override void SetData(object data)
        {
            Data = data as T;
        }

        #endregion
    }
}
