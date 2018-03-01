#region License

// QuadraticBezierCurve.cs
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
using System.Collections.Generic;

namespace Genesis.Math
{
    public class QuadraticBezierCurve : ICurve
    {
        public Point A { get; set; }
        public Point B { get; set; }
        public Point C { get; set; }

        public QuadraticBezierCurve(Point a, Point b, Point c)
        {
            A = a;
            B = b;
            C = c;
        }

        public IEnumerable<Point> GetPoints(int count)
        {
            if (count < 2)
                throw new ArgumentOutOfRangeException("count", "Count must be at least 2.");

            float a = 1.0f;
            float b = 1.0f - a;

            for (int i = 0; i < count - 1; i++)
            {
                yield return new Point(
                    (A.X * a * a) + (2 * B.X * a * b) + (C.X * b * b),
                    (A.Y * a * a) + (2 * B.Y * a * b) + (C.Y * b * b));

                a -= 1.0f / (count - 1);
                b = 1.0f - a;
            }

            yield return C;
        }
    }
}
