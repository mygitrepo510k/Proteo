using MWF.Mobile.Core.Models;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Services
{
    public interface IMainService
    {
        MobileData CurrentMobileData { get; set; }
        MobileApplicationDataChunkContentActivity CurrentDataChunkActivity { get; set; }
        Driver CurrentDriver { get; set; }
        Vehicle CurrentVehicle { get; set; }
        bool OnManifestPage { get; set; }

        void SendReadChunk(IEnumerable<MobileData> instructions);
        void SendDataChunk(bool revisedQuantity = false);
        void SendPhotoAndComment(string comment, List<Image> photos);
    }
}
