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
        public MyDecimal()
        {
            intValue = 0;
            fraction = new int[50];
            usedDecimals = 0;
        }
        public static MyDecimal operator +(MyDecimal left, MyDecimal right)
        {
            MyDecimal toReturn = new MyDecimal();


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

    public struct BigDecimal : IEquatable<BigDecimal>, IComparable<BigDecimal>
    {
        public static BigDecimal Zero = new BigDecimal();
        public static BigDecimal Four = new BigDecimal(new BigInteger(4));
        public static BigDecimal Two = new BigDecimal(new BigInteger(2));

        public BigInteger IntegerPart { get; private set; }
        public BigInteger FractionalPart { get; private set; }

        public BigDecimal(BigInteger integerPart, BigInteger fractionalPart)
        {
            IntegerPart = integerPart;
            FractionalPart = fractionalPart;
        }

        public BigDecimal(BigInteger integerPart)
        {
            IntegerPart = integerPart;
            FractionalPart = BigInteger.Zero;
        }

        public BigDecimal()
        {
            IntegerPart = BigInteger.Zero;
            FractionalPart = BigInteger.Zero;
        }

        public static BigDecimal operator +(BigDecimal left, BigDecimal right)
        {
            BigInteger integerPart = left.IntegerPart + right.IntegerPart;
            BigInteger fractionalPart = left.FractionalPart + right.FractionalPart;

            // Check for carry-over from fractional part
            if (BigInteger.Abs(fractionalPart) >= BigInteger.Pow(10, Scale))
            {
                integerPart += BigInteger.DivRem(fractionalPart, BigInteger.Pow(10, Scale), out fractionalPart);
            }

            return new BigDecimal(integerPart, fractionalPart);
        }

        public static BigDecimal operator -(BigDecimal left, BigDecimal right)
        {
            BigInteger integerPart = left.IntegerPart - right.IntegerPart;
            BigInteger fractionalPart = left.FractionalPart - right.FractionalPart;

            // Check for borrow from fractional part
            if (fractionalPart < 0)
            {
                integerPart--;
                fractionalPart += BigInteger.Pow(10, Scale);
            }

            return new BigDecimal(integerPart, fractionalPart);
        }

        public static BigDecimal operator *(BigDecimal left, BigDecimal right)
        {
            BigInteger integerPart = left.IntegerPart * right.IntegerPart;
            BigInteger fractionalPart = left.FractionalPart * right.FractionalPart;

            // Scale down fractional part
            fractionalPart /= BigInteger.Pow(10, Scale * 2);

            // Add overflow from integer part multiplication
            BigInteger overflow = (left.IntegerPart * right.FractionalPart) + (right.IntegerPart * left.FractionalPart);
            overflow /= BigInteger.Pow(10, Scale);

            return new BigDecimal(integerPart + overflow, fractionalPart);
        }

        public static BigDecimal operator /(BigDecimal dividend, BigDecimal divisor)
        {
            BigInteger scaledDividend = (dividend.IntegerPart * BigInteger.Pow(10, Scale)) + dividend.FractionalPart;
            BigInteger scaledDivisor = (divisor.IntegerPart * BigInteger.Pow(10, Scale)) + divisor.FractionalPart;

            BigInteger integerPart = BigInteger.DivRem(scaledDividend, scaledDivisor, out BigInteger fractionalPart);
            fractionalPart *= BigInteger.Pow(10, Scale);

            return new BigDecimal(integerPart, fractionalPart);
        }

        public static bool operator ==(BigDecimal left, BigDecimal right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BigDecimal left, BigDecimal right)
        {
            return !left.Equals(right);
        }

        public static bool operator <(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator <=(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(BigDecimal left, BigDecimal right)
        {
            return left.CompareTo(right) >= 0;
        }

        public int CompareTo(BigDecimal other)
        {
            BigInteger scaledThis = (IntegerPart * BigInteger.Pow(10, Scale)) + FractionalPart;
            BigInteger scaledOther = (other.IntegerPart * BigInteger.Pow(10, Scale)) + other.FractionalPart;

            return scaledThis.CompareTo(scaledOther);
        }

        // Additional overloaded arithmetic operations can be implemented in a similar manner

        public override string ToString()
        {
            string decimalPart = FractionalPart.ToString().PadLeft(Scale, '0');
            return $"{IntegerPart}.{decimalPart}";
        }

        public bool Equals(BigDecimal other)
        {
            return this.CompareTo(other) == 0;
        }

        // Define the desired scale for the decimal representation
        private const int Scale = 50;
    }
}
