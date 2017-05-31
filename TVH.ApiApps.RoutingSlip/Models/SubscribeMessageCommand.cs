using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class SubscribeMessageCommand
    {
        [Metadata("Step Name", "The name of the subscribe step (service)", VisibilityType.Default)]
        public string StepName { get; set; }
    }
}