using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MandelbrotVisualizer
{
    public struct MyDecimal : IEquatable<MyDecimal>, IComparable<MyDecimal>
    {
        public static MyDecimal Four = new MyDecimal(4m);
        public static MyDecimal Two = new MyDecimal(2m);
        public static MyDecimal MinusOne = new MyDecimal(-1m);
        public static MyDecimal Zero = new MyDecimal(0m);

        bool isPositive;
        int intValue;
        int Sign { get { return Math.Sign(intValue); } }
        int[] fraction;
        int usedDecimals
        {
            get
            {
                for (int i = CurrentSize - 1; i >= 0; i--)
                {
                    if (fraction[i] != 0)
                    {
                        return i + 1;
                    }
                }
                return 0;
            }
        }
        int CurrentSize { get { return fraction.Length; } }
        public MyDecimal(decimal value)
        {
            if (value > 0)
            {
                isPositive = true;
            }
            else
            {
                isPositive = false;
            }
            intValue = (int)Math.Floor(Math.Abs(value));
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
        }
        public MyDecimal(int size)
        {
            isPositive = true;
            intValue = 0;
            fraction = new int[size > 50 ? 50 * (size / 50 + 1) : 50];
        }
        public static MyDecimal operator +(MyDecimal left, MyDecimal right)
        {
            if (!left.isPositive && !right.isPositive)
            {
                if (left.isPositive)
                {
                    return left - right;
                }
                else if (right.isPositive)
                {
                    return right - left;
                }
                else
                {
                    return (right.Absolute() + left.Absolute()).Negative();
                }
            }

            MyDecimal toReturn = new MyDecimal(Math.Max(left.CurrentSize, right.CurrentSize));
            int sizediff = left.CurrentSize - right.CurrentSize;
            if (sizediff > 0)
            {
                right.ExtendPrecision(sizediff);
            }
            else if (sizediff < 0)
            {
                left.ExtendPrecision(Math.Abs(sizediff));
            }
            toReturn.intValue = left.intValue + right.intValue;
            for (int i = Math.Max(left.usedDecimals, right.usedDecimals); i >= 1; i--)
            {
                toReturn.fraction[i] += left.fraction[i] + right.fraction[i];
                if (toReturn.fraction[i] >= 10)
                {
                    toReturn.fraction[i - 1] += toReturn.fraction[i] / 10;
                    toReturn.fraction[i] %= 10;
                }
            }
            toReturn.fraction[0] += left.fraction[0] + right.fraction[0];
            if (toReturn.fraction[0] >= 10)
            {
                toReturn.intValue += toReturn.fraction[0] / 10;
                toReturn.fraction[0] %= 10;
            }


            return toReturn;

        }
        public static MyDecimal operator -(MyDecimal left, MyDecimal right)
        {
            MyDecimal toReturn = new MyDecimal(Math.Max(left.CurrentSize, right.CurrentSize));




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
        MyDecimal Negative()
        {
            MyDecimal neg = Clone();
            neg.isPositive = false;
            return neg;
        }
        MyDecimal Absolute()
        {
            MyDecimal abs = Clone();
            abs.isPositive = true;
            return abs;
        }
        MyDecimal Clone()
        {
            MyDecimal toReturn = new MyDecimal(this.CurrentSize);
            for (int i = 0; i < usedDecimals; i++)
            {
                toReturn.fraction[i] = fraction[i];
            }
            toReturn.intValue = intValue;
            toReturn.isPositive = isPositive;
            return toReturn;
        }
        void ExtendPrecision(int AmountOfExtraDigits)
        {
            int size = CurrentSize + AmountOfExtraDigits;
            int[] newfrac = new int[50 * (size / 50 + 1)];
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

            if (this.intValue < other.intValue)
            {
                return -1;
            }
            else if (this.intValue > other.intValue)
            {
                return 1;
            }
            else if (this.intValue == other.intValue)
            {
                for (int i = 0; i < Math.Min(usedDecimals, other.usedDecimals); i++)
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
                if (this.usedDecimals < other.usedDecimals)
                {
                    return -1;
                }
                else if (this.usedDecimals > other.usedDecimals)
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
