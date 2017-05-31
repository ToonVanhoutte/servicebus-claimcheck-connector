using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRex.Metadata;

namespace TVH.ApiApps.RoutingSlip.Models
{
    public class PublishMessageCommand
    {
        [Metadata("Routing Slip", "Provide the routing slip XML", VisibilityType.Default)]
        public string RoutingSlip { get; set; }

        [Metadata("Body", "Provide the message body to be published", VisibilityType.Default)]
        public byte[] MessageBody { get; set; }

        [Metadata("Context", "Context to be added to the routing slip XML", VisibilityType.Advanced)]
        public JObject Context { get; set; }
    }
}