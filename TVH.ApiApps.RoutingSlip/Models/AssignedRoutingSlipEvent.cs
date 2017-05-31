using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class AssignedRoutingSlipEvent
    {
        [Metadata("Routing Slip", "The content of the routing slip XML.", VisibilityType.Default)]
        public string RoutingSlip { get; set; }
    }
}