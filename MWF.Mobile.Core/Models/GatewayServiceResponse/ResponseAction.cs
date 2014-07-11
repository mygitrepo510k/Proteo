using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Models.GatewayServiceResponse
{

    public class ResponseAction<TData>
    {
        public Guid ActionID { get; set; }
        public TData Data { get; set; }
    }

}
