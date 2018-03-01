#region License

// Point.cs
// Author: Daniel Sklenitzka
//
// Copyright 2013 The CWC Team
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics;

namespace Genesis
{
    [DebuggerDisplay("({X}, {Y})")]
    public struct Point : IEquatable<Point>, IComparable<Point>
    {
        public float X { get; private set; }
        public float Y { get; private set; }

        private float? vectorLength;

        public Point(float x, float y)
            : this()
        {
            X = x;
            Y = y;
        }

        public float VectorLengthSquared()
        {
            return X * X + Y * Y;
        }

        public float VectorLength()
        {
            if (!vectorLength.HasValue)
                vectorLength = (float)System.Math.Sqrt(VectorLengthSquared());
            return vectorLength.Value;
        }

        public Point Normalize()
        {
            if (vectorLength.HasValue && vectorLength == 1f)
                return new Point(X, Y);

            var normalized = this / VectorLength();
            normalized.vectorLength = 1f;
            return normalized;
        }

        public Point ScaleTo(float length)
        {
            var scaled = Normalize() * length;
            scaled.vectorLength = length;
            return scaled;
        }

        public Point Truncate(float length)
        {
            if (VectorLengthSquared() > length * length)
            {
                return ScaleTo(length);
            }
            else
            {
                return new Point(X, Y);
            }
        }

        public Point Rotate(Point pivot, float angle)
        {
            float sin = (float)System.Math.Sin(angle);
            float cos = (float)System.Math.Cos(angle);

            // translate point back to origin
            var point = this - pivot;

            // rotate point
            point = new Point(
                point.X * cos - point.Y * sin,
                point.X * sin + point.Y * cos);
            
            // translate point back
            return point + pivot;
        }

        public bool Equals(Point other)
        {
            return other.X == X && other.Y == Y;
        }

        public int CompareTo(Point other)
        {
            int result = X.CompareTo(other.X);
            return result == 0 ? Y.CompareTo(other.Y) : result;
        }

        public static Point operator -(Point a, Point b)
        {
            return new Point(a.X - b.X, a.Y - b.Y);
        }

        public static Point operator +(Point a, Point b)
        {
            return new Point(a.X + b.X, a.Y + b.Y);
        }

        public static Point operator -(Point a, float offset)
        {
            return new Point(a.X - offset, a.Y - offset);
        }

        public static Point operator +(Point a, float offset)
        {
            return new Point(a.X + offset, a.Y + offset);
        }

        public static Point operator *(Point a, float offset)
        {
            return new Point(a.X * offset, a.Y * offset);
        }

        public static Point operator /(Point a, float offset)
        {
            return new Point(a.X / offset, a.Y / offset);
        }
    }
}
