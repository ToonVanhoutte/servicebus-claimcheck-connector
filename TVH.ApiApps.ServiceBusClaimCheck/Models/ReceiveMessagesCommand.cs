using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.ServiceBusClaimCheck.Models
{
    public class ReceiveMessagesCommand
    {
        [Metadata("Queue Name", "Queue to receive messages from", VisibilityType.Default)]
        public string QueueName { get; set; }

        [Metadata("Amount", "The number of messages to receive", VisibilityType.Default)]
        public int Amount { get; set; }

        [Metadata("Timeout", "The number of seconds to wait before the batch arrives", VisibilityType.Default)]
        public int TimeOut { get; set; }
    }
}