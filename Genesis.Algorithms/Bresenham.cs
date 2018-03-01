#region License

// Bresenham.cs
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

namespace Genesis.Algorithms
{
    /// <summary>
    /// Implementation of the Bresenham algorithm.
    /// </summary>
    public class Bresenham<TCoordinates>
    {
        private Func<int, int, TCoordinates> getCoordinates;

        /// <summary>
        /// Initializes a new instance of the <see cref="Bresenham&lt;TCoordinates&gt;"/> class.
        /// </summary>
        /// <param name="getCoordinates">A function converting an X and Y coordinate into the result type.</param>
        public Bresenham(Func<int, int, TCoordinates> getCoordinates)
        {
            this.getCoordinates = getCoordinates;
        }

        /// <summary>
        /// Rasterizes a line between two points on a 2D grid and returns the coordinates of all the tiles hit by this line.
        /// </summary>
        /// <param name="x1">The X coordinate of the start point.</param>
        /// <param name="y1">The Y coordinate of the start point</param>
        /// <param name="x2">The X coordinate of the end point</param>
        /// <param name="y2">The Y coordinate of the end point</param>
        /// <returns></returns>
        public IEnumerable<TCoordinates> GetCoordinates(int x1, int y1, int x2, int y2)
        {
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);

            int distance = Math.Max(dx, dy);
            int d, dInc1, dInc2;

            int x, xInc1, xInc2;
            int y, yInc1, yInc2;

            if (dx >= dy)
            {
                d = (2 * dy) - dx;
                dInc1 = dy << 1;
                dInc2 = (dy - dx) << 1;
                xInc1 = 1;
                xInc2 = 1;
                yInc1 = 0;
                yInc2 = 1;
            }
            else
            {
                d = (2 * dx) - dy;
                dInc1 = dx << 1;
                dInc2 = (dx - dy) << 1;
                xInc1 = 0;
                xInc2 = 1;
                yInc1 = 1;
                yInc2 = 1;
            }

            if (x1 > x2)
            {
                xInc1 = -xInc1;
                xInc2 = -xInc2;
            }

            if (y1 > y2)
            {
                yInc1 = -yInc1;
                yInc2 = -yInc2;
            }

            x = x1;
            y = y1;

            for (int i = 0; i < distance; i++)
            {
                if (d < 0)
                {
                    d += dInc1;
                    x += xInc1;
                    y += yInc1;
                }
                else
                {
                    d += dInc2;
                    x += xInc2;
                    y += yInc2;
                }

                yield return getCoordinates(x, y);
            }
        }
    }
}
