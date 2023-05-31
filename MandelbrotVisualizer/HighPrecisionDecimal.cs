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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

    public struct HighPrecisionDecimal : IEquatable<HighPrecisionDecimal>, IComparable<HighPrecisionDecimal>
    {
        public static HighPrecisionDecimal Four = new HighPrecisionDecimal(4m);
        public static HighPrecisionDecimal Two = new HighPrecisionDecimal(2m);
        public static HighPrecisionDecimal MinusOne = new HighPrecisionDecimal(-1m);
        public static HighPrecisionDecimal Zero = new HighPrecisionDecimal(0m);

        public static int CurrentMaxPrecision = 28;

        bool isPositive;
        int intValue;
        int[] fraction;
        int? _uD;
        int usedDecimals
        {
            get
            {
                if (_uD == null)
                {
                    for (int i = CurrentSize - 1; i >= 0; i--)
                    {
                        if (fraction[i] != 0)
                        {
                            _uD = i + 1;
                            return i + 1;
                        }
                    }
                    _uD = 0;
                    return 0;

                }
                else
                {
                    return (int)_uD;
                }

            }
        }
        int CurrentSize { get { return fraction.Length; } }
        public HighPrecisionDecimal(string value)
        {
            isPositive = true;
            fraction = new int[CurrentMaxPrecision + 1];
            if (value.Contains('-'))
            {
                value = value.Replace("-", "");
                isPositive = false;
            }
            string[] number = value.Split('.');
            intValue = int.Parse(number[0]);
            for (int i = 0; i < number[1].Length && i < CurrentMaxPrecision; i++)
            {
                fraction[i] = number[1][i] - '0';
            }

        }
        public HighPrecisionDecimal(decimal value)
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
            fraction = new int[CurrentMaxPrecision + 1];

            StringBuilder sb = new StringBuilder();
            sb.Append(value);
            bool ok = false;
            int j = 0;
            for (int i = 0; i < sb.Length && i < CurrentMaxPrecision; i++)
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
        public HighPrecisionDecimal(int uselessnumber)
        {
            isPositive = true;
            intValue = 0;
            fraction = new int[CurrentMaxPrecision + 1];
        }
        public static HighPrecisionDecimal operator +(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            if (!left.isPositive || !right.isPositive)
            {
                if (left.isPositive)
                {
                    return left - right.Absolute();
                }
                else if (right.isPositive)
                {
                    return right - left.Absolute();
                }
                else
                {
                    return (right.Absolute() + left.Absolute()).Negative();
                }
            }

            HighPrecisionDecimal toReturn = new HighPrecisionDecimal(Math.Max(left.CurrentSize, right.CurrentSize));

            //int sizediff = left.CurrentSize - right.CurrentSize;
            //if (sizediff > 0)
            //{
            //    right.ExtendPrecision(sizediff);
            //}
            //else if (sizediff < 0)
            //{
            //    left.ExtendPrecision(Math.Abs(sizediff));
            //}

            toReturn.intValue = left.intValue + right.intValue;
            for (int i = Math.Max(left.usedDecimals, right.usedDecimals) - 1; i >= 1; i--)
            {
                toReturn.fraction[i] += left.fraction[i] + right.fraction[i];
                if (toReturn.fraction[i] >= 10)
                {
                    toReturn.fraction[i - 1] += 1;
                    toReturn.fraction[i] -= 10;
                }
            }
            toReturn.fraction[0] += left.fraction[0] + right.fraction[0];
            if (toReturn.fraction[0] >= 10)
            {
                toReturn.intValue += 1;
                toReturn.fraction[0] -= 10;
            }


            return toReturn;

        }
        public static HighPrecisionDecimal operator -(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            if (left.isPositive && !right.isPositive)
            {
                return left + right.Absolute();
            }
            if (!left.isPositive && right.isPositive)
            {
                return (left.Absolute() + right).Negative();
            }
            if (!left.isPositive && !right.isPositive)
            {
                if (left.Absolute() < right.Absolute())
                {
                    return right.Absolute() - left.Absolute();
                }
                else
                {
                    return (left.Absolute() - right.Absolute()).Negative();
                }
            }
            if (left.Absolute() < right.Absolute())
            {
                return (right.Absolute() - left.Absolute()).Negative();
            }

            HighPrecisionDecimal toReturn = new HighPrecisionDecimal(Math.Max(left.CurrentSize, right.CurrentSize));

            //int sizediff = left.CurrentSize - right.CurrentSize;
            //if (sizediff > 0)
            //{
            //    right.ExtendPrecision(sizediff);
            //}
            //else if (sizediff < 0)
            //{
            //    left.ExtendPrecision(Math.Abs(sizediff));
            //}

            for (int i = Math.Max(left.usedDecimals, right.usedDecimals) - 1; i >= 1; i--)
            {
                toReturn.fraction[i] += left.fraction[i] - right.fraction[i];
                if (toReturn.fraction[i] < 0)
                {
                    toReturn.fraction[i - 1] -= 1;
                    toReturn.fraction[i] += 10;
                }
            }
            toReturn.fraction[0] += left.fraction[0] - right.fraction[0];
            if (toReturn.fraction[0] < 0)
            {
                toReturn.intValue -= 1;
                toReturn.fraction[0] += 10;
            }
            toReturn.intValue += left.intValue - right.intValue;
            if (toReturn.intValue < 0)
            {
                toReturn.isPositive = false;
                toReturn.intValue = Math.Abs(toReturn.intValue);
            }

            return toReturn;
        }
        public static HighPrecisionDecimal operator *(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            if (left.intValue * right.intValue > 1000000)
            {
                throw new OverflowException("This struct is made for infinite precision towards zero!!");
            }
            bool ResultSign = true;
            if (!left.isPositive || !right.isPositive)
            {
                if (!(!left.isPositive && !right.isPositive))
                {
                    ResultSign = false;
                }
            }
            HighPrecisionDecimal toReturn = new HighPrecisionDecimal(left.CurrentSize + right.CurrentSize);
            int[] intvalues = new int[8];
            int[] leftint = new int[8];
            int[] rightint = new int[8];
            int leftaux = left.intValue;
            int rightaux = right.intValue;
            int helper = 7;
            while (leftaux > 0)
            {
                leftint[helper] = leftaux % 10;
                leftaux /= 10;
                helper--;
            }
            helper = 7;
            while (rightaux > 0)
            {
                rightint[helper] = rightaux % 10;
                rightaux /= 10;
                helper--;
            }


            for (int i = left.usedDecimals - 1; i >= 0; i--)
            {
                for (int j = right.usedDecimals - 1; j >= 0; j--)
                {
                    if (i + j + 1 > CurrentMaxPrecision)
                    {
                        continue;
                    }
                    toReturn.fraction[i + j + 1] += left.fraction[i] * right.fraction[j];
                    if (toReturn.fraction[i + j + 1] >= 10)
                    {
                        toReturn.fraction[i + j] += toReturn.fraction[i + j + 1] / 10;

                        toReturn.fraction[i + j + 1] %= 10;
                    }
                }
                for (int j = rightint.Length - 1; j >= 0; j--)
                {
                    if ((rightint.Length - 1 - j) - i - 1 < 0)
                    {
                        int fractionalindex = Math.Abs((rightint.Length - 1 - j) - i);
                        if (fractionalindex > CurrentMaxPrecision)
                        {
                            continue;
                        }
                        toReturn.fraction[fractionalindex] += left.fraction[i] * rightint[j];
                        if (toReturn.fraction[fractionalindex] >= 10)
                        {
                            if (fractionalindex == 0)
                            {
                                intvalues[intvalues.Length - 1] += toReturn.fraction[fractionalindex] / 10;
                            }
                            else
                            {
                                toReturn.fraction[fractionalindex - 1] += toReturn.fraction[fractionalindex] / 10;
                            }
                            toReturn.fraction[fractionalindex] %= 10;
                        }
                    }
                    else
                    {
                        intvalues[j] += left.fraction[i] * rightint[j];
                        if (intvalues[j] >= 10)
                        {
                            intvalues[j - 1] += intvalues[j] / 10;
                            intvalues[j] %= 10;
                        }
                    }

                }
            }
            for (int i = leftint.Length - 1; i >= 0; i--)
            {
                for (int j = right.usedDecimals - 1; j >= 0; j--)
                {
                    if ((leftint.Length - 1 - i) - j - 1 < 0)
                    {
                        int fractionalindex = Math.Abs((leftint.Length - 1 - i) - j);
                        if (fractionalindex > CurrentMaxPrecision)
                        {
                            continue;
                        }
                        toReturn.fraction[fractionalindex] += leftint[i] * right.fraction[j];
                        if (toReturn.fraction[fractionalindex] >= 10)
                        {
                            if (fractionalindex == 0)
                            {
                                intvalues[intvalues.Length - 1] += toReturn.fraction[fractionalindex] / 10;
                            }
                            else
                            {
                                toReturn.fraction[fractionalindex - 1] += toReturn.fraction[fractionalindex] / 10;
                            }
                            toReturn.fraction[fractionalindex] %= 10;
                        }
                    }
                    else
                    {
                        int intindex = Math.Abs((leftint.Length - 1 - i) - j - 1);
                        intvalues[intvalues.Length - 1 - intindex] += leftint[i] * right.fraction[j];
                        if (intvalues[intvalues.Length - 1 - intindex] >= 10)
                        {
                            intvalues[intvalues.Length - 1 - intindex - 1] += intvalues[intvalues.Length - 1 - intindex] / 10;
                            intvalues[intvalues.Length - 1 - intindex] %= 10;
                        }
                    }
                }
                for (int j = rightint.Length - 1; j >= 0; j--)
                {
                    int intindex = intvalues.Length - 1 - Math.Abs((leftint.Length - 1 - i - (rightint.Length - 1 - j)));
                    intvalues[intindex] += leftint[i] * rightint[j];
                    if (intvalues[intindex] >= 10)
                    {
                        intvalues[intindex - 1] += intvalues[intindex] / 10;
                        intvalues[intindex] %= 10;
                    }
                }
            }
            int newval = 0;
            for (int i = 0; i < intvalues.Length; i++)
            {
                newval += intvalues[intvalues.Length - 1 - i] * (int)Math.Pow(10, i);
            }
            toReturn.intValue = newval;
            toReturn.isPositive = ResultSign;
            return toReturn;
        }
        public static bool operator <(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            return left.CompareTo(right) < 0;
        }
        public static bool operator >(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            return left.CompareTo(right) > 0;
        }
        public static bool operator <=(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            return left.CompareTo(right) <= 0;
        }
        public static bool operator >=(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            return left.CompareTo(right) >= 0;
        }
        public static bool operator ==(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            return left.CompareTo(right) == 0;
        }
        public static bool operator !=(HighPrecisionDecimal left, HighPrecisionDecimal right)
        {
            return left.CompareTo(right) != 0;
        }
        HighPrecisionDecimal Negative()
        {
            HighPrecisionDecimal neg = Clone();
            neg.isPositive = false;
            return neg;
        }
        HighPrecisionDecimal Absolute()
        {
            HighPrecisionDecimal abs = Clone();
            abs.isPositive = true;
            return abs;
        }
        HighPrecisionDecimal Clone()
        {
            HighPrecisionDecimal toReturn = new HighPrecisionDecimal(this.CurrentSize);
            for (int i = 0; i < usedDecimals && i < CurrentMaxPrecision + 1; i++)
            {
                toReturn.fraction[i] = fraction[i];
            }
            toReturn.intValue = intValue;
            toReturn.isPositive = isPositive;
            return toReturn;
        }
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!isPositive)
            {
                stringBuilder.Append('-');
            }
            stringBuilder.Append(intValue);
            stringBuilder.Append('.');
            if (usedDecimals == 0)
            {
                stringBuilder.Append('0');
            }
            else
            {
                for (int i = 0; i < usedDecimals; i++)
                {
                    stringBuilder.Append(fraction[i]);
                }
            }
            return stringBuilder.ToString();
        }

        public int CompareTo(HighPrecisionDecimal other)
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
        public decimal ToDecimal()
        {
            decimal toReturn = 0;
            toReturn += intValue;

            for (int i = 0; i < 28 && i < usedDecimals; i++)
            {
                toReturn += fraction[i] / (decimal)Math.Pow(10, i + 1);
            }

            if (!isPositive)
            {
                toReturn *= -1;
            }



            return toReturn;
        }
        public double ToDouble()
        {
            double toReturn = 0;
            toReturn += intValue;

            for (int i = 0; i < usedDecimals; i++)
            {
                toReturn += fraction[i] / (double)Math.Pow(10, i + 1);
            }

            if (!isPositive)
            {
                toReturn *= -1;
            }

            return toReturn;

        }
        public bool Equals(HighPrecisionDecimal other)
        {
            return this.CompareTo(other) == 0;
        }

        public override bool Equals(object? obj)
        {
            if (obj is HighPrecisionDecimal)
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
