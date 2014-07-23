using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels
{
    public abstract class BaseFragmentViewModel
        : MvxViewModel
    {

        abstract public string FragmentTitle { get; }

    }
}