using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MandelbrotVisualizer
{
    public static class ComplexMaths
    {
        public static Task<Color> GetColorForComplexNumber(decimal a, decimal b, int MaxIter)
        {
            decimal x = a;
            decimal y = b;
            int n = 0;
            do
            {
                decimal temp = x * x - y * y + a;
                y = 2 * x * y + b;
                x = temp;
                n++;
            } while (x * x + y * y < 16 && n < MaxIter);

            Color result;
            if (n == MaxIter)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)MaxIter);
            }

            return Task.FromResult(result);
        }
        public static Task<Color> GetColorForComplexNumber(MyDecimal a, MyDecimal b, int MaxIter)
        {
            MyDecimal x = a;
            MyDecimal y = b;
            int n = 0;
            do
            {
                MyDecimal temp = x * x - y * y + a;
                y = MyDecimal.Two * x * y + b;
                x = temp;
                n++;
            } while (x * x + y * y < MyDecimal.Four && n < MaxIter);

            Color result;
            if (n == MaxIter)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)MaxIter);
            }

            return Task.FromResult(result);
        }
        public static Task<Color> GetColorForComplexNumber(double a, double b, int MaxIter)
        {
            double x = a;
            double y = b;
            int n = 0;
            do
            {
                double temp = x * x - y * y + a;
                y = 2 * x * y + b;
                x = temp;
                n++;
            } while (x * x + y * y < 16 && n < MaxIter);

            Color result;
            if (n == MaxIter)
            {
                result = Color.FromRgb(0, 0, 0);
            }
            else
            {
                result = Rainbow((float)n / (float)MaxIter);
            }

            return Task.FromResult(result);
        }
        public static Color Rainbow(float progress)
        {
            float div = (Math.Abs(progress % 1) * 6);
            int ascending = (int)((div % 1) * 255);
            int descending = 255 - ascending;

            switch ((int)div)
            {
                case 0:
                    return Color.FromArgb(255, 255, (byte)ascending, 0);
                case 1:
                    return Color.FromArgb(255, (byte)descending, 255, 0);
                case 2:
                    return Color.FromArgb(255, 0, 255, (byte)ascending);
                case 3:
                    return Color.FromArgb(255, 0, (byte)descending, 255);
                case 4:
                    return Color.FromArgb(255, (byte)ascending, 0, 255);
                default: // case 5:
                    return Color.FromArgb(255, 255, 0, (byte)descending);
            }
        }
    }
}
