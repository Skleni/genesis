#region License

// ScriptObject.cs
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

namespace CkMp.Data.Objects
{
    public class ScriptObject : PropertyObject
    {
        public const string WaypointObjectType = "*Waypoints/Waypoint";

        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public float Rotation { get; set; }

        public string Type { get; set; }

        public RoadOptions RoadOptions { get; set; }

        public float SquaredDistanceOnGround(float x, float y)
        {
            float dx = this.X - x;
            float dy = this.Y - y;

            return dx * dx + dy * dy;
        }

        public float SquaredDistanceOnGround(ScriptObject other)
        {
            return SquaredDistanceOnGround(other.X, other.Y);
        }

        public int GetSize(bool includingHeader)
        {
            return (includingHeader ? 10 : 0)
                 + 4 * sizeof(float)
                 + 4 //null
                 + 2 //type length
                 + Type.Length
                 + GetSize();
        }

        public bool IsWaypoint
        {
            get { return Type == WaypointObjectType; }
        }
    }
}
