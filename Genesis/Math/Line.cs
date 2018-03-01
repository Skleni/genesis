#region License

// Line.cs
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

namespace Genesis.Math
{
    public class Line
    {
        private const float EPSILON = 0.00001f;

        private float verticalLineX;
        private float intercept;

        public float Slope { get; set; }

        public float Intercept
        {
            set { intercept = value; }
            get
            {
                if (IsVertical)
                    throw new InvalidOperationException("Vertical lines do not intercept the vertical axis.");
                else
                    return intercept; 
            }
        }
        
        public bool IsVertical { get { return float.IsInfinity(Slope); } }

        public bool IsHorizontal { get { return System.Math.Abs(Slope) < EPSILON; } }

        public Line(float slope, float intercept)
        {
            Slope = slope;
            Intercept = intercept;
        }

        public Line(float x)
        {
            MakeVertical(x);
        }

        public Line(float x1, float y1, float x2, float y2)
        {
            if (System.Math.Abs(x1 - x2) < EPSILON)
                MakeVertical(x1);
            else if (x1 < x2)
            {
                Slope = (y2 - y1) / (x2 - x1);
                Intercept = y1 - Slope * x1;
            }
            else
            {
                Slope = (y1 - y2) / (x1 - x2);
                Intercept = y2 - Slope * x2;
            }
        }

        public Line(Point p1, Point p2) : this(p1.X, p1.Y, p2.X, p2.Y)
        {
        }
        
        public void MakeVertical(float x)
        {
            verticalLineX = x;
            Slope = float.PositiveInfinity;
        }

        public float CalculateY(float x)
        {
            return Slope * x + Intercept;
        }

        public bool IsParallelTo(Line line)
        {
            return (IsVertical && line.IsVertical) || System.Math.Abs(this.Slope - line.Slope) < EPSILON;
        }

        public Line NormalThroughPoint(Point point)
        {
            if (IsVertical)
                return new Line(0f, point.Y);

            if (IsHorizontal)
                return new Line(point.X);

            return new Line(point.X, point.Y, point.X + 1, point.Y - 1f / this.Slope);
        }

        public bool Intersect(Line line, out Point point)
        {
            if (IsParallelTo(line))
            {
                point = new Point();
                return false;
            }

            if (IsVertical)
            {
                point = new Point(verticalLineX, line.CalculateY(verticalLineX));
                return true;
            }

            if (line.IsVertical)
            {
                point = new Point(line.verticalLineX, CalculateY(line.verticalLineX));
                return true;
            }

            //find the intersection:

            //               this.Slope * x + this.Intercept == line.Slope * x + line.Intercept     // - line.Slope * x
            //(this.Slope - line.Slope) * x + this.Intercept == line.Intercept                      // - this.Intercept
            //                 (this.Slope - line.Slope) * x == line.Intercept - this.Intercept     // / (this.Slope - line.Slope)
            //                                             x == (line.Intercept - this.Intercept) / (this.Slope - line.Slope)

            float x = (line.Intercept - this.Intercept) / (this.Slope - line.Slope);
            point = new Point(x, CalculateY(x));

            return true;
        }

        public Point Intersect(Line line)
        {
            Point intersection;
            if (!Intersect(line, out intersection))
                throw new ArgumentException("Parallel lines do not intersect.");

            return intersection;
        }

        public Point NearestPointOnLine(Point point)
        {
            Line normal = NormalThroughPoint(point);
            return Intersect(normal);
        }

        public float Distance(Point point)
        {
            Point pointOnLine = NearestPointOnLine(point);
            return (pointOnLine - point).VectorLength();
        }

        public Point Mirror(Point point)
        {
            Point nearestPoint = NearestPointOnLine(point);
            return nearestPoint + (nearestPoint - point);
        }
    }
}
