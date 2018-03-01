#region License

// Angle.cs
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

namespace Genesis.Math
{
    public static class Angle
    {
        public static float Between(Point vector1, Point vector2)
        {
            float f = (vector1.X * vector2.X + vector1.Y * vector2.Y) /
                      (vector1.VectorLength() * vector2.VectorLength());
            if (f < -1f)
                f = -1f;
            else if (f > 1f)
                f = 1f;

            return (float)System.Math.Acos(f);
        }

        public static float ToRadians(float degrees)
        {
            return (float)(degrees * System.Math.PI / 180);
        }

        public static float ToDegrees(float radians)
        {
            return (float)(radians / System.Math.PI * 180);
        }
    }
}
