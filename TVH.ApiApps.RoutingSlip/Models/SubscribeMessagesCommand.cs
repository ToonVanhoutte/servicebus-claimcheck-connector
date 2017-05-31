using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class SubscribeMessagesCommand
    {
        [Metadata("Step Name", "The name of the subscribe step (service)", VisibilityType.Default)]
        public string StepName { get; set; }

        [Metadata("Amount", "The number of messages to receive in the batch", VisibilityType.Default)]
        public int Amount { get; set; }

        [Metadata("Timeout", "The number of seconds to wait before the batch arrives", VisibilityType.Default)]
        public int TimeOut { get; set; }
    }
}