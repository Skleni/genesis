#region License

// ExtensionMethods.cs
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
using System.Linq;
using CkMp.Data.Map;
using CkMp.Data.Objects;

namespace Genesis
{
    public static class ExtensionMethods
    {
        public static Point GetPosition(this ScriptObject scriptObject)
        {
            return new Point(scriptObject.X, scriptObject.Y);
        }

        public static bool IsWithinMap(this Map map, Coordinates coordinates)
        {
            return
                coordinates.X > map.HeightMap.Border &&
                coordinates.Y > map.HeightMap.Border &&
                coordinates.X < map.HeightMap.Width - map.HeightMap.Border &&
                coordinates.Y < map.HeightMap.Height - map.HeightMap.Border;
        }

        public static bool IsWithinMap(this Map map, Point point)
        {
            return IsWithinMap(map, map.PositionToCoordinates(point));
        }

        public static void AddWaypath(this Map map, string name, params ScriptObject[] waypoints)
        {
            map.AddWaypath(name, waypoints.AsEnumerable());
        }

        public static void AddWaypath(this Map map, string name, IEnumerable<ScriptObject> waypoints)
        {
            ScriptObject previous = null;
            foreach (var waypoint in waypoints)
            {
                for (int i = 1; i <= 3; i++)
			    {
                    string label = "waypointPathLabel" + i;
                    Property property;
                    if (waypoint.Properties.TryGetValue(label, out property))
                    {
                        if (property.Value.Equals(name))
                            break;
                    }
                    else
                    {
                        waypoint.SetProperty(label, PropertyType.OneByteString, name);
                        break;
                    }
			    }

                if (!map.Objects.Contains(waypoint))
                    map.Objects.Add(waypoint);

                if (previous != null)
                {
                    map.Paths.Add(new Tuple<int, int>(
                        (int)previous.Properties["waypointID"].Value,
                        (int)waypoint.Properties["waypointID"].Value));
                }

                previous = waypoint;
            }
        }

        public static Point CoordinatesToPosition(this Map map, Coordinates coordinates)
        {
            return CoordinatesToPosition(map.HeightMap, coordinates);
        }

        public static Coordinates PositionToCoordinates(this Map map, Point point)
        {
            return PositionToCoordinates(map.HeightMap, point);
        }

        public static Point CoordinatesToPosition(this HeightMap heightMap, Coordinates coordinates)
        {
            return new Point(((coordinates.X - heightMap.Border) * Map.TileWidth) + Map.TileWidth / 2f, ((coordinates.Y - heightMap.Border) * Map.TileWidth) + Map.TileWidth / 2f);
        }

        public static Coordinates PositionToCoordinates(this HeightMap heightMap, Point point)
        {
            return new Coordinates((int)(point.X / Map.TileWidth) + heightMap.Border, (int)(point.Y / Map.TileWidth) + heightMap.Border);
        }
    }
}
