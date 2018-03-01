#region License

// Tiles.cs
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
using System.Linq;

namespace CkMp.Data.Map
{
    public class TileData : Grid<Tile>
    {
        public int NumberOfBaseTiles { get; set; }
        public int NumberOfBlendTiles { get; set; }

        public List<Texture> Textures { get; set; }
        public List<BlendTile> BlendTiles { get; set; }

        /// <summary>
        /// Stores all indices in the BlendTiles collection for a given tile index.
        /// </summary>
        private IDictionary<short, IList<short>> tileBlendTypes;

        public TileData(int width, int height) : base(width, height)
        {
            for (int i = 0; i < width * height; i++)
                this[i] = new Tile();

            this.Textures = new List<Texture>();
            this.BlendTiles = new List<BlendTile>();
            this.tileBlendTypes = new Dictionary<short, IList<short>>();
        }

        public int GetSize(bool includingHeader)
        {
            return 4
                 + (Width * Height) * 2 * 4
                 + Height * ((Width / 8) + (Width % 8 == 0 ? 0 : 1))
                 + 4
                 + 4
                 + 4
                 + 4
                 + this.Textures.Sum(t => t.GetSize())
                 + this.BlendTiles.Sum(b => b.GetSize())
                 + 8;
        }

        public Texture AddTexture(Texture texture)
        {
            var existing = Textures.FirstOrDefault(t => t.Name == texture.Name);
            if (existing != null)
                return existing;

            texture.BlockStartIndex = NumberOfBaseTiles;
            NumberOfBaseTiles += texture.BlockCount;
            Textures.Add(texture);
            return texture;
        }

        /// <summary>
        /// Gets the index of the blend tile. If the blend tile doesn't exist yet, it is added to the list.
        /// </summary>
        /// <param name="tileIndex">The tile index (in texture space).</param>
        /// <param name="blendType">The blend type.</param>
        /// <returns></returns>
        public short GetBlendTileIndex(short tileIndex, BlendType blendType)
        {
            // Get the indices of all the blend tiles already defined for this tile index.
            IList<short> indices;
            if (!tileBlendTypes.TryGetValue(tileIndex, out indices))
            {
                // No blend tiles for this tile index yet. Add a new list to the dictionary.
                indices = new List<short>();
                tileBlendTypes.Add(tileIndex, indices);
            }

            // Check if one of the existing blend tiles has the same blend type. If yes, return it.
            foreach (short i in indices)
            {
                if (BlendTiles[i].TileIndex == tileIndex && BlendTiles[i].BlendType == blendType)
                    return (short)(i + 1);
            }

            // Otherwise, create the new blend tile and add it before returning the index.
            BlendTiles.Add(new BlendTile() { TileIndex = tileIndex, BlendType = blendType });
            NumberOfBlendTiles++;
            indices.Add((short)(BlendTiles.Count - 1));

            return (short)BlendTiles.Count;
        }

        public Texture GetTexture(int i, int j)
        {
            return Textures.First(t => t.BlockStartIndex * 4 <= this[i, j].BaseTexture && (t.BlockStartIndex + t.BlockCount) * 4 > this[i, j].BaseTexture);
        }
    }
}
