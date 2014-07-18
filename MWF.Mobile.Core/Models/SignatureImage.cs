using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models
{
    
    public class SignatureImage
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public IList<PointF> Points { get; set; }

        public string ToBlueSphereFormat()
        {
            //TODO: implement
            return null;
        }
    }

}
