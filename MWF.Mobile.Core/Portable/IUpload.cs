using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Portable
{
    public interface IUpload
    {
                bool UploadFile(System.Uri address,string username, string password,  string path);
    }
}
