using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;

namespace MWF.Mobile.Core.Presentation
{

    public class CloseUpToViewPresentationHint<TViewModel>
        : MvxPresentationHint
        where TViewModel : IMvxViewModel
    {
    }

}
