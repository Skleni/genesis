#region License

// Texture.cs
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
    public class Texture
    {
        public String Name { get; set; }
        
        public int BlockStartIndex { get; set; }
        public int BlockCount { get; set; } //1 block = 4 tiles
        public int BlocksPerRow { get; set; } //always square root of BlockCount?
        public int Magic { get; set; } //always 0?

        public Texture()
        {

        }

        public Texture(string name, int blocksPerRow)
        {
            Name = name;
            BlocksPerRow = blocksPerRow;
            BlockCount = blocksPerRow * blocksPerRow;
        }

        public int GetSize()
        {
            return 4 * 4
                 + 2
                 + Name.Length;
        }

        public short GetTileIndex(int x, int y)
        {
            int blockX = (x / 2) % BlocksPerRow;
            int blockY = (y / 2) % BlocksPerRow;

            int index = blockY * 4 * BlocksPerRow + blockX * 4;

            if (x % 2 == 1)
                index++;
            if (y % 2 == 1)
                index += 2;

            return (short)(BlockStartIndex * 4 + index);
        }
    }
}
