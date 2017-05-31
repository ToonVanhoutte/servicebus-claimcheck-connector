using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.RoutingSlip
{
    public class RoutingSlip
    {
        public RoutingSlip()
        {
            RoutingHeader = new RoutingHeader();
            RoutingSteps = new List<RoutingStep>();


        }
        public RoutingHeader RoutingHeader { get; set; }
        public List<RoutingStep> RoutingSteps { get; set; }
    }
}
