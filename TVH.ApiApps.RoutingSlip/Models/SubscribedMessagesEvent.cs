﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class SubscribedMessagesEvent
    {
        [Metadata("Message Batch", "Content of the subscribed messages", VisibilityType.Default)]
        public List<byte[]> MessageBatch { get; set; }

        [Metadata("Routing Slip", "The content of the routing slip XML", VisibilityType.Default)]
        public string RoutingSlip { get; set; }

        [Metadata("Step Properties", "The properties of the current routing step (service)", VisibilityType.Default)]
        public JObject StepProperties { get; set; }

        [Metadata("Context", "The context throughtout the complete routing slip", VisibilityType.Default)]
        public JObject Context { get; set; }

        [Metadata("Lock Tokens", "Tokens to complete or abandon the subscribed message", VisibilityType.Default)]
        public List<string> LockTokens { get; set; }

        [Metadata("Queue Name", "Name of the queue the message is received from", VisibilityType.Default)]
        public string QueueName { get; set; }

        [Metadata("Flow", "Name of the flow the message is part of", VisibilityType.Default)]
        public string Flow { get; set; }

        [Metadata("Correlation ID", "Correlation ID throughout the complete routing slip", VisibilityType.Default)]
        public string CorrelationId { get; set; }

        [Metadata("Step Number", "Number of the step within the routing slip", VisibilityType.Default)]
        public int CurrentStepNumber { get; set; }

        [Metadata("BatchSize", "Number of message in the batck", VisibilityType.Default)]
        public int BatchSize { get; set; }
    }
}