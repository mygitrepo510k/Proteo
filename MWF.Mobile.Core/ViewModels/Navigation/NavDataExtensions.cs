using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MWF.Mobile.Core.ViewModels.Navigation;
using MWF.Mobile.Core.Models.Instruction;

namespace MWF.Mobile.Core.ViewModels.Navigation.Extensions
{
    public static class NavDataHelper
    {
        public static MobileApplicationDataChunkContentActivity GetDataChunk(this NavData navData)
        {
            object obj = null;
            MobileApplicationDataChunkContentActivity dataChunk;
            navData.OtherData.TryGetValue("DataChunk", out obj);

            if (obj == null)
            {
                dataChunk = new MobileApplicationDataChunkContentActivity();
                navData.OtherData["DataChunk"] = dataChunk;
            }
            else
            {
                dataChunk = obj as MobileApplicationDataChunkContentActivity;
            }

            return dataChunk;
        }

        public static MobileData GetMobileData(this NavData navData)
        {

            if (navData is NavData<MobileData>)
                return (navData as NavData<MobileData>).Data;

            object obj;
            navData.OtherData.TryGetValue("MobileData", out obj);
            if (obj != null)
            {
                return obj as MobileData;
            }

            return null;
        }
    }
}
