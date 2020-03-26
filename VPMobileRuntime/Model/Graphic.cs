using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPMobileRuntime100_1_0.Model
{
    public interface Graphic
    {
        dynamic ID { get; }

        double X { get; }

        double Y { get; }

        string RenderValue { get; }

        bool Visible { get; }
    }
}
