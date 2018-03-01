#region License

// StartingPositionGenerator.cs
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
using System.Threading;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Core;
using Genesis.Tiles;

namespace Genesis.Processors
{
    public class StartingPositionGenerator : IMapProcessor
    {
        /// <summary>
        /// Gets or sets the minimum distance between two starting positions (measured in BaseRadii).
        /// 4 means that there would have to fit another base between two starting positions:
        ///  * --- )(---  ---)(--- *
        /// P1  1     2    3    4  P2
        /// </summary>
        public int MinimumDistance { get; set; }

        public StartingPositionGenerator()
        {
            MinimumDistance = 4;
        }

        public string Description
        {
            get
            {
                return "Generating starting positions";
            }
        }

        public bool UpdatePreview
        {
            get { return true; }
        }

        public void Process(Map map, MapInfo info)
        {
            // Imagine two starting positions at the top and bottom edge of the map (displaced by info.BaseRadius)
            // that are connected by a line through the map's center. By rotating the line, the positions are moved
            // horizontally and contrarily.
            // We're rotating the line to find the smallest possible angle (0° meaning a vertical line and horizontally
            // centered starting positions) at which the positions are still distant enough from each other.

            double width = (map.HeightMap.MapWidth / 2.0);
            double height = (map.HeightMap.MapHeight / 2.0);

            // Create the following triangle:
            //     ____
            //    |   /
            //  y |  / h
            //    |a/
            //    |/

            double y = height - info.BaseRadius;
            double h = MinimumDistance * info.BaseRadius / 2.0;

            double minAngle;
            if (y > h)
                minAngle = 0;
            else
                minAngle = System.Math.Acos(y / h);

            // Again, now the maximum angle:
            //
            //    /| 
            //   / |
            // a/h |
            // /___|
            //   x

            double x = width - info.BaseRadius;

            double _90 = System.Math.PI / 2;
            double _180 = 2 * _90;
            double _270 = 3 * _90;
            double _360 = 4 * _90;

            double maxAngle;
            if (x > h)
                maxAngle = _90;
            else
                maxAngle = _90 - System.Math.Acos(x / h);

            double angleToCorner = System.Math.Atan(width / height);
            double angle = minAngle >= maxAngle ? angleToCorner : new Random(info.MapId).NextDouble() * (maxAngle - minAngle) + minAngle;

            // If minAngle > maxAngle, the Map is too small to have the minimum distance between the starting positions.
            // In that case we just leave x and y as they are to put the starting positions as far out as possible.

            double length;
            for (int i = 0; i < info.NumberOfPlayers; i++)
            {
                double currentAngle = angle + i * (_360 / info.NumberOfPlayers);

                if (currentAngle < angleToCorner)
                {
                    length = y / System.Math.Cos(currentAngle);
                }
                else if (currentAngle < _90)
                {
                    length = x / System.Math.Cos(_90 - currentAngle);
                }
                else if (currentAngle < _180 - angleToCorner)
                {
                    length = x / System.Math.Cos(currentAngle - _90);              
                }
                else if (currentAngle < _180)
                {
                    length = y / System.Math.Cos(_180 - currentAngle);
                }
                else if (currentAngle < _180 + angleToCorner)
                {
                    length = y / System.Math.Cos(currentAngle - _180);
                }
                else if (currentAngle < _270)
                {
                    length = x / System.Math.Cos(_270 - currentAngle);
                }
                else if (currentAngle < _360 - angleToCorner)
                {
                    length = x / System.Math.Cos(currentAngle - _270);
                }
                else
                {
                    length = y / System.Math.Cos(_360 - currentAngle);
                }

                double newX = 0;
                double newY = 1;
                Rotate(ref newX, ref newY, -currentAngle);
                newX *= length;
                newY *= length;

                info.StartingPositions[i] = new Coordinates((int)(map.HeightMap.Border + width + newX), (int)(map.HeightMap.Border + height + newY));
                
                for (int j = info.StartingPositions[i].X - info.BaseRadius; j <= info.StartingPositions[i].X + info.BaseRadius; j++)
                {
                    for (int k = info.StartingPositions[i].Y - info.BaseRadius; k <= info.StartingPositions[i].Y + info.BaseRadius; k++)
                    {
                        if (info.IsWithinStartingRange(new Coordinates(j, k), info.BaseRadius))
                            info.Tiles[j, k] |= TileInfo.Base;
                    }
                }
            }

            for (int i = 0; i < info.StartingPositions.Length; i++)
            {
                Point position = map.CoordinatesToPosition(info.StartingPositions[i]);
                ScriptObject waypoint = info.ObjectFactory.CreateWaypoint(position.X, position.Y, string.Format("Player_{0}_Start", i + 1));
                map.Objects.Add(waypoint);
            }
        }

        private void Rotate(ref double x, ref double y, double angle)
        {
            double newX = x * System.Math.Cos(angle) - y * System.Math.Sin(angle);
            y = x * System.Math.Sin(angle) + y * System.Math.Cos(angle);
            x = newX;
        }
    }
}
