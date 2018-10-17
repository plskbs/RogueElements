﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RogueElements
{
    [Serializable]
    public struct Range : IEquatable<Range>
    {
        /// <summary>
        /// Start of the range (inclusive)
        /// </summary>
        public int Min;
        /// <summary>
        /// End of the range (exclusive)
        /// </summary>
        public int Max;

        public Range(int num)
        {
            Min = num;
            Max = num + 1;
        }

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public Range(Range other)
        {
            Min = other.Min;
            Max = other.Max;
        }

        public bool Contains(int mid)
        {
            return (Min <= mid && mid < Max);
        }

        public int GetRange()
        {
            return Max - Min;
        }


        public override string ToString()
        {
            return String.Format("({0}, {1}]", Min, Max);
        }

        public override bool Equals(object obj)
        {
            return (obj is Range) && Equals((Range)obj);
        }

        public bool Equals(Range other)
        {
            return (Min == other.Min && Max == other.Max);
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() ^ Max.GetHashCode();
        }


        public static Range operator +(Range value1, int value2)
        {
            return new Range(value1.Min + value2, value1.Max + value2);
        }
    }
}