using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TVH.RoutingSlip
{
    public class RoutingStep
    {
        public RoutingStep(string routingStepName)
        {
            StepName = routingStepName;
            StepConfig = new List<Parameter>();
        }

        public RoutingStep()
        {

        }

        public string StepName { get; set; }
        public List<Parameter> StepConfig { get; set; }
    }
}
