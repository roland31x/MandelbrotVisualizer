using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer
{
    public static class PrecisionEngine
    {
        static Precision _cp = Precision.DOUBLE;
        public static Precision CurrentPrecision { get { return _cp; } set { _cp = value; } }

    }
    public enum Precision
    {
        DOUBLE,
        DECIMAL,
        HIGHPRECISION,
    }
}
