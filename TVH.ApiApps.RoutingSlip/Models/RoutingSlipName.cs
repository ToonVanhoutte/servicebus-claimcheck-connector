using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class RoutingSlipName
    {
        [Metadata("Routing Slip Name", "Provide the name of the routing slip to assign.", VisibilityType.Default)]
        public string routingSlipName { get; set; }
    }
}