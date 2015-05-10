using Cirrious.MvvmCross.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.ViewModels.Interfaces
{
    public interface IModalViewModel<TResult>: IMvxViewModel 
    {
        Guid MessageId { get; set; }

        void Cancel();
        void ReturnResult(TResult result);
    }
}
