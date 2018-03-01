#region License

// Area.cs
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

using System.Collections.Generic;

namespace CkMp.Data.Objects
{
    public class Area
    {
        public Area()
        {
            Points = new List<int[]>();
        }

        public string Name { get; set; }
        public int Id { get; set; }
        public bool IsWater { get; set; }
        public bool IsRiver { get; set; }
        public int RiverStart { get; set; } // [-1; Points.Count)
        public List<int[]> Points { get; private set; }

        public int GetSize()
        {
            return 2 //Name length
                 + Name.Length
                 + 2 //null
                 + 4 //Id
                 + 2 //flags
                 + 4 //RiverStart
                 + 4 //Points
                 + Points.Count * 3 * 4;
        }
    }
}
