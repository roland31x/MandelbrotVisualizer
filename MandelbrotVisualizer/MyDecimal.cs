using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MandelbrotVisualizer
{
    public struct MyDecimal : IEquatable<MyDecimal>, IComparable<MyDecimal>
    {
        public static MyDecimal Four = new MyDecimal(4m);
        public static MyDecimal Two = new MyDecimal(2m);
        public static MyDecimal MinusOne = new MyDecimal(-1m);
        public static MyDecimal Zero = new MyDecimal(0m);
        
        int intValue;
        int[] fraction;
        int usedDecimals;
        int CurrentSize { get { return fraction.Length; } }
        public MyDecimal(decimal value)
        {
            intValue = (int)Math.Floor(value);
            fraction = new int[50];
            StringBuilder sb = new StringBuilder();
            sb.Append(value);
            bool ok = false;
            int j = 0;
            for (int i = 0; i < sb.Length; i++)
            {
                if (ok)
                {
                    fraction[j] = sb[i] - '0';
                    j++;
                }
                else if (!ok)
                {
                    if (sb[i] == '.')
                    {
                        ok = true;
                    }
                }
            }
            usedDecimals = j;
        }
        public MyDecimal(int size)
        {
            intValue = 0;
            fraction = new int[size > 50 ? size : 50];
            usedDecimals = 0;
        }
        public static MyDecimal operator +(MyDecimal left, MyDecimal right)
        {
            MyDecimal toReturn = new MyDecimal(Math.Min(left.CurrentSize,right.CurrentSize));



            return toReturn;

        }
        public static MyDecimal operator -(MyDecimal left, MyDecimal right)
        {
            MyDecimal toReturn = new MyDecimal();


            return toReturn;
        }
        public static MyDecimal operator *(MyDecimal left, MyDecimal right)
        {
            MyDecimal toReturn = new MyDecimal();


            return toReturn;
        }
        public static bool operator <(MyDecimal left, MyDecimal right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(MyDecimal left, MyDecimal right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(MyDecimal left, MyDecimal right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(MyDecimal left, MyDecimal right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(MyDecimal left, MyDecimal right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(MyDecimal left, MyDecimal right)
        {
            return left.CompareTo(right) != 0;
        }
        void ExtendPrecision(int AmountOfExtraDigits)
        {
            int[] newfrac = new int[CurrentSize + AmountOfExtraDigits];
            for (int i = 0; i < usedDecimals; i++)
            {
                newfrac[i] = fraction[i];
            }
            fraction = newfrac;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(intValue);
            stringBuilder.Append('.');
            for (int i = 0; i < usedDecimals; i++)
            {
                stringBuilder.Append(fraction[i]);
            }
            return stringBuilder.ToString();
        }

        public int CompareTo(MyDecimal other)
        {
            
            if(this.intValue < other.intValue)
            {
                return -1;
            }
            else if(this.intValue > other.intValue)
            {
                return 1;
            }
            else if(this.intValue == other.intValue)
            {
                for(int i = 0; i < Math.Min(usedDecimals,other.usedDecimals); i++)
                {
                    if (this.fraction[i] < other.fraction[i])
                    {
                        return -1;
                    }
                    else if (this.fraction[i] > other.fraction[i])
                    {
                        return 1;
                    }
                }
                if(this.usedDecimals < other.usedDecimals)
                {
                    return -1;
                }
                else if(this.usedDecimals > other.usedDecimals)
                {
                    return 1;
                }
            }
            return 0;
        }

        public bool Equals(MyDecimal other)
        {
            return this.CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is MyDecimal)
            {
                return this.Equals(obj);
            }
            else return false;
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }
}
