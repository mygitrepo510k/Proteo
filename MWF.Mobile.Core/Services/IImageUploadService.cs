using MWF.Mobile.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IImageUploadService
    {
        Task SendPhotoAndCommentAsync(string comment, List<Image> photos, Driver currentDriver, MWF.Mobile.Core.Models.Instruction.MobileData currentMobileData);
    }
}
