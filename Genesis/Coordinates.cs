#region License

// Coordinates.cs
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
    public struct Coordinates : IEquatable<Coordinates>, IComparable<Coordinates>
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Coordinates(int x, int y) : this()
        {
            X = x;
            Y = y;
        }

        public bool Equals(Coordinates other)
        {
            return other.X == X && other.Y == Y;
        }

        public int CompareTo(Coordinates other)
        {
            int result = X.CompareTo(other.X);
            return result == 0 ? Y.CompareTo(other.Y) : result;
        }

        public override string ToString()
        {
            return string.Format("({0:N0, {1:N1})", this.X, this.Y);
        }

        public int SquaredDistance(Coordinates other)
        {
            var vector = other - this;
            return vector.X * vector.X + vector.Y * vector.Y;            
        }

        public static Coordinates operator - (Coordinates a, Coordinates b)
        {
            return new Coordinates(a.X - b.X, a.Y - b.Y);
        }

        public static Coordinates operator + (Coordinates a, Coordinates b)
        {
            return new Coordinates(a.X + b.X, a.Y + b.Y);
        }

        public static Coordinates operator -(Coordinates a, int offset)
        {
            return new Coordinates(a.X - offset, a.Y - offset);
        }

        public static Coordinates operator +(Coordinates a, int offset)
        {
            return new Coordinates(a.X + offset, a.Y + offset);
        }
    }
}
