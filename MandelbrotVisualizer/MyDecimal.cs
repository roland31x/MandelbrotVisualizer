using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer
{
    public struct MyDecimal
    {
        int IntValue;
        int[] fraction;
        public MyDecimal(decimal value)
        {
            IntValue = (int)Math.Floor(value);
            fraction = new int[30];
            
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(IntValue.ToString());
            stringBuilder.Append('.');
            foreach(int i in fraction)
            {
                stringBuilder.Append(i.ToString());
            }
            return stringBuilder.ToString();
        }
    }
}
