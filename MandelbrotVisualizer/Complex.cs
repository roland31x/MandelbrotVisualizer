using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer
{
    public class Complex
    {
        public static int MaxIterations = 500;
        public double Re { get; private set; }
        public double Im { get; private set; }

        static double ImResult = -1; // the result for i * i
        public char Sgn
        {
            get
            {
                if (Im < 0)
                    return '-';
                else
                    return '+';
            }
        }

        public Complex(double real = 0, double imaginary = 0)
        {
            Re = real;
            Im = imaginary;
        }
        public static Complex operator !(Complex nr) => new Complex(nr.Re, nr.Im * -1);
        public static Complex operator +(Complex left, Complex right) => new Complex(left.Re + right.Re, left.Im + right.Im);
        public static Complex operator -(Complex left, Complex right) => new Complex(left.Re - right.Re, left.Im - right.Im);

        // ( a + bi ) * ( c + di ) = a * c + a * d * i + b * i * c + b * d * ( i * i ) this is where the ImResult comes in
        // (a * c + b * d * (i*i) + (a * d + b * c)i
        public static Complex operator *(Complex left, Complex right) => new Complex(left.Re * right.Re + ImResult * left.Im * right.Im, right.Re * left.Im + left.Re * right.Im);
        public static Complex operator /(Complex left, Complex right)
        {
            Complex result = new Complex();
            Complex up = left * !right;
            Complex down = right * !right;
            result.Re = up.Re / down.Re;
            result.Im = up.Im / down.Im;
            return result;
        }
        public double Magnitude()
        {
            return Math.Sqrt(Re * Re + Im * Im);         
        }
        public double ManhattanDist()
        {
            return Math.Abs(Re - Im);
        }
        public override string ToString()
        {
            return $"{Re} {Sgn} {Math.Abs(Im)}i";
        }
        //public Point ToPoint()
        //{
        //    //return new Point(Re, Im);
        //}
        public static int NumberOfIterations(Complex toIterate)
        {
            int n = 0;

            Complex C = toIterate;
            Complex Result = new Complex();
            do
            {
                Result = Result * Result + C;
                n++;
            }
            while (Result.Magnitude() < 4 && n < MaxIterations);
            return n;
        }
    }
}
