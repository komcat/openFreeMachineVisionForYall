using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCVwpf.ImageClass
{
    public class CornerDetectionParameters
    {
        public double MinDistance { get; set; } = 30;
        public double QualityLevel { get; set; } = 0.1;
        public int BlockSize { get; set; } = 3;
        public int MaxCorners { get; set; } = 20;

        public event EventHandler ParametersChanged;

        public void NotifyParametersChanged()
        {
            ParametersChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
