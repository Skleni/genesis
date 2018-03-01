#region License

// RampGenerator.cs
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
using System.Linq;
using CkMp.Data.Map;
using Genesis.Math;

namespace Genesis.Tools
{
    public class RampGenerator
    {
        private const double piOverTwo = System.Math.PI / 2;
        private static readonly double range = System.Math.Tanh(piOverTwo);

        public int? RandomOffsetSeed { get; set; }

        public RampGenerator(int? randomOffsetSeed)
        {
            RandomOffsetSeed = randomOffsetSeed;
        }

        public void GenerateFlatArea(Map map, Coordinates center, byte height, int radiusInTiles, float radius)
        {
            var offset = new Coordinates(radiusInTiles, 0);
            var left = center - offset;
            var right = center + offset;
            map.HeightMap[left.X, left.Y] = map.HeightMap[right.X, right.Y] = height;
            GenerateRamp(
                map,
                map.CoordinatesToPosition(left),
                map.CoordinatesToPosition(right),
                radiusInTiles * Map.TileWidth,
                radius);
        }

        public void GenerateRamp(Map map, Point start, Point end, float halfWidth, float radius)
        {
            var random = RandomOffsetSeed.HasValue ? new Random(RandomOffsetSeed.Value) : null;

            var primaryAxisVector = (end - start);
            float length = primaryAxisVector.VectorLength();

            Coordinates startCoordinates = map.PositionToCoordinates(start);
            Coordinates endCoordinates = map.PositionToCoordinates(end);

            byte startHeight = map.HeightMap[startCoordinates.X, startCoordinates.Y];
            byte endHeight = map.HeightMap[endCoordinates.X, endCoordinates.Y];
            int heightDifference = endHeight - startHeight;

            float halfLength = length / 2f;
            
            var normal = new Point(primaryAxisVector.Y, -primaryAxisVector.X) / length; ;
            var halfWidthVector = normal * (halfWidth + radius);
            var center = start + (primaryAxisVector / 2);

            var v = primaryAxisVector / length * radius;
            Point[] corners = new Point[]
            {
                start - v + halfWidthVector,
                start - v - halfWidthVector,
                end + v + halfWidthVector,
                end + v - halfWidthVector
            };

            var primaryAxis = new Line(start, end);
            var secondaryAxis = new Line(center + halfWidthVector, center - halfWidthVector);

            var min = map.PositionToCoordinates(new Point(corners.Min(p => p.X), corners.Min(p => p.Y)));            
            var max = map.PositionToCoordinates(new Point(corners.Max(p => p.X), corners.Max(p => p.Y)));

            Grid<byte> heights = new Grid<byte>(max.X - min.X, max.Y - min.Y);

            for (int x = 0; x < heights.Width; x++)
            {
                for (int y = 0; y < heights.Height; y++)
                {
                    var coordinates = new Coordinates(min.X + x, min.Y + y);

                    if (!CheckCoordinates(coordinates, map.HeightMap))
                        continue;

                    var point = map.CoordinatesToPosition(coordinates);
                    var pointOnPrimaryAxis = primaryAxis.NearestPointOnLine(point);
                    var pointOnSecondaryAxis = secondaryAxis.NearestPointOnLine(point);
                    
                    var nx = point - pointOnPrimaryAxis;
                    var ny = point - pointOnSecondaryAxis;
                    
                    float dx = nx.VectorLength();
                    float dy = ny.VectorLength();
                    
                    byte height;

                    if (dx <= halfWidth && dy <= halfLength)
                    {
                        height = (byte)(startHeight + (heightDifference * (pointOnPrimaryAxis - start).VectorLength() / length));
                    }
                    else
                    {
                        Point pointOnBorder = center;
                        if (dx > 0f)
                            pointOnBorder += (nx / dx) * System.Math.Min(dx, halfWidth);
                        if (dy > 0f)
                            pointOnBorder += (ny / dy) * System.Math.Min(dy, halfLength);

                        var borderToTarget = point - pointOnBorder;
                        var borderToTargetLength = borderToTarget.VectorLength();

                        if (borderToTargetLength > radius)
                        {
                            height = (byte)map.HeightMap[coordinates.X, coordinates.Y];
                        }
                        else
                        {
                            var target = pointOnBorder + (borderToTarget / borderToTargetLength * radius);
                            var targetCoordinates = map.PositionToCoordinates(target);

                            height = (byte)(startHeight + (heightDifference * (pointOnPrimaryAxis - start).VectorLength() / length));
                            byte targetHeight = map.HeightMap[targetCoordinates.X, targetCoordinates.Y];

                            float weight = borderToTargetLength / radius;
                            if (RandomOffsetSeed.HasValue)
                                weight *= 1f + (random.Next(6) - 3) * 0.01f;
                            weight = (float)((System.Math.Tanh(((weight * 2) - 1) * piOverTwo) + range) / (2 * range));
                            height = (byte)(height + (targetHeight - height) * weight);
                        }
                    }

                    heights[x, y] = height;                        
                }
            }

            for (int x = 0; x < heights.Width; x++)
            {
                for (int y = 0; y < heights.Height; y++)
                {
                    map.HeightMap[min.X + x, min.Y + y] = heights[x, y];
                    //map.Tiles[min.X + x, min.Y + y].Impassable = true;
                }
            }
        }

        private bool CheckCoordinates(Coordinates coordinates, HeightMap heightMap)
        {
            return coordinates.X >= 0 &&
                   coordinates.Y >= 0 &&
                   coordinates.X < heightMap.Width &&
                   coordinates.Y < heightMap.Height;
        }
    }
}
