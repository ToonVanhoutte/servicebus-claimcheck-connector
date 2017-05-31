using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class AcknowledgeMessageCommand
    {
        [Metadata("Queue Name","Name of the queue", VisibilityType.Default)]
        public string QueueName { get; set; }

        [Metadata("Lock Token", "Token to complete / abandon the message", VisibilityType.Default)]
        public string LockToken { get; set; }

        [Metadata("Ingore Lock Lost Exception", "Boolean to indicate if a 'lock lost' exception can be ignored", VisibilityType.Advanced)]
        public bool IgnoreLockLostException { get; set; } = true;
    }
}