#region License

// SymmetryAnalyzer.cs
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
using System.Collections.ObjectModel;
using System.Linq;
using CkMp.Data.Map;
using Genesis.Core;
using Genesis.Math;

namespace Genesis.Tools
{
    public class SymmetryAnalyzer
    {
        public ReadOnlyCollection<Line> Analyze(Map map, IEnumerable<Coordinates> startingPositions)
        {
            var points = startingPositions.Select<Coordinates, Point>(map.CoordinatesToPosition).ToArray();
            return Analyze(map, points);
        }

        public ReadOnlyCollection<Line> Analyze(Map map, Point[] startingPositions)
        {
            switch (startingPositions.Length)
            {
                case 2:
                    return AnalyzeTwoPlayerMap(map, startingPositions);                    
                case 4:
                    return AnalyzeFourPlayerMap(map, startingPositions);
                default:
                    throw new NotSupportedException("Currently only 2 and 4 player maps are supported.");
            }
        }

        private ReadOnlyCollection<Line> AnalyzeTwoPlayerMap(Map map, Point[] startingPositions)
        {
            var axes = new Line[2];

            axes[1] = new Line(startingPositions[0].X, startingPositions[0].Y, startingPositions[1].X, startingPositions[1].Y);
            
            float centerX = (float)startingPositions.Average(c => c.X);
            float centerY = (float)startingPositions.Average(c => c.Y);
            float slope = -1.0f / axes[1].Slope;
            
            axes[0] = new Line(slope, centerY - slope * centerX);

            return new ReadOnlyCollection<Line>(axes);
        }

        private ReadOnlyCollection<Line> AnalyzeFourPlayerMap(Map map, Point[] startingPositions)
        {
            var mostDistantPositions = (from p1 in startingPositions
                                        from p2 in startingPositions
                                        where !p1.Equals(p2)
                                        let distance = (p1 - p2).VectorLengthSquared()
                                        orderby distance descending
                                        select new [] { p1, p2 }).First();

            var remainingPositions = startingPositions.Except(mostDistantPositions).ToArray();

            var center = new Point(map.HeightMap.MapWidth * Map.TileWidth / 2, map.HeightMap.MapHeight * Map.TileWidth / 2);

            var axes = new []
            {
                new Line(mostDistantPositions[0], mostDistantPositions[1]),
                new Line(remainingPositions[0], remainingPositions[1]),
                new Line(0, center.Y, center.X * 2, center.Y),
                new Line(center.X, 0, center.X, center.Y * 2)
            };

            return new ReadOnlyCollection<Line>(axes);
        }
    }
}
