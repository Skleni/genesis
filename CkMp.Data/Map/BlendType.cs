#region License

// BlendType.cs
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

namespace CkMp.Data.Map
{
    public struct BlendType : IEquatable<BlendType>
    {
        private long properties;

        public bool IsLargeAngledEdge { get { return ((properties >> 56) & 1) == 1; } }
        public bool IsTopRightCorner { get { return ((properties >> 48) & 1) == 1; } }
        public bool IsRightAngledEdge { get { return ((properties >> 40) & 1) == 1; } }
        public bool IsLeftAngledEdge { get { return ((properties >> 32) & 1) == 1; } }
        public bool IsHorizontalEdge { get { return ((properties >> 24) & 1) == 1; } }
        public bool IsVerticalEdge { get { return ((properties >> 16) & 1) == 1; } }

        private BlendType(long properties)
	    {
            this.properties = properties;
	    }

        public override bool Equals(object obj)
        {
            return obj is BlendType && this.Equals((BlendType)obj);
        }

        public bool Equals(BlendType other)
        {
            return this.properties.Equals(other.properties);
        }

        public static bool operator == (BlendType x, BlendType y)
        {
            return x.Equals(y);
        }

        public static bool operator != (BlendType x, BlendType y)
        {
            return !x.Equals(y);
        }

        public override int GetHashCode()
        {
            return properties.GetHashCode();
        }

        public int GetSize()
        {
            return 8;
        }

        public override string ToString()
        {
 	         if (this == Top)
                 return "Top";
             else if (this == Bottom)
                 return "Bottom";
             else if (this == Left)
                 return "Left";
             else if (this == Right)
                 return "Right";
             else if (this == TopLeftSmall)
                 return "TopLeftSmall";
             else if (this == TopLeftLarge)
                 return "TopLeftLarge";
             else if (this == TopRightSmall)
                 return "TopRightSmall";
             else if (this == TopRightLarge)
                 return "TopRightLarge";
             else if (this == BottomLeftSmall)
                 return "BottomLeftSmall";
             else if (this == BottomLeftLarge)
                 return "BottomLeftLarge";
             else if (this == BottomRightSmall)
                 return "BottomRightSmall";
             else if (this == BottomRightLarge)
                 return "BottomRightLarge";
             else
                 return "Unknown: " + properties;
        }

        public static explicit operator BlendType(long properties)
        {
            return new BlendType(properties);
        }

        public static explicit operator long(BlendType blendType)
        {
            return blendType.properties;
        }

        public static BlendType Top = new BlendType(281474993487872);
        public static BlendType Bottom = new BlendType(16777216);
        public static BlendType Left = new BlendType(65536);
        public static BlendType Right = new BlendType(281474976776192);
        public static BlendType TopLeftSmall = new BlendType(281479271677952);
        public static BlendType TopLeftLarge = new BlendType(72339073309605888);
        public static BlendType TopRightSmall = new BlendType(282574488338432);
        public static BlendType TopRightLarge = new BlendType(72340168526266368);
        public static BlendType BottomLeftSmall = new BlendType(4294967296);
        public static BlendType BottomLeftLarge = new BlendType(72057598332895232);
        public static BlendType BottomRightSmall = new BlendType(1099511627776);
        public static BlendType BottomRightLarge = new BlendType(72058693549555712);
    }
}
