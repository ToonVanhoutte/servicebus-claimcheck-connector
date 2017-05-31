using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class AssignRoutingSlipCommand
    {
        [Metadata("Routing Slip Name", "Provide the name of the routing slip to assign.", VisibilityType.Default)]
        public string RoutingSlipName { get; set; }
    }
}