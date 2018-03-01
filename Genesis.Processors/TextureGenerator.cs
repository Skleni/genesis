#region License

// TextureGenerator.cs
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
using System.Threading;
using CkMp.Data.Map;
using Genesis.Core;
using Genesis.Math;
using Genesis.Settings;
using Genesis.Tiles;

namespace Genesis.Processors
{
    public class TextureGenerator : IMapProcessor
    {
        private Map map;
        private MapInfo info;
        private Grid<Texture> textureData;

        public string Description
        {
            get
            {
                return "Generating textures";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            this.map = map;
            this.info = info;

            //textureData = new Grid<Texture>(map.HeightMap.Width + 1, map.HeightMap.Height + 1);
            textureData = info.TextureData;

            info.Settings.Scenery.AddTexturesTo(map.Tiles);

            PerlinNoise noise = new PerlinNoise(info.MapId)
            {
                Persistence = 0.25f,
                Amplitude = 1.45f,
                Octaves = 1,
            };

            for (int i = 0; i < map.Tiles.Width; i++)
            {
                for (int j = 0; j < map.Tiles.Height; j++)
                {
                    if ((info.Tiles[i, j] & TileInfo.Cliff) == TileInfo.Cliff)
                        SetTexture(i, j, noise, info.Settings.Scenery.CliffTexture, info.Settings.Scenery.CliffOverlayTextures);
                    else
                        SetTexture(i, j, noise, info.Settings.Scenery.BaseTexture, info.Settings.Scenery.OverlayTextures);
                }
            }

            GenerateBlendTiles();
        }

        private void SetTexture(int x, int y, PerlinNoise noise, Texture baseTexture, IEnumerable<TextureData> overlayTextures)
        {
            SetTexture(x, y, baseTexture);
            SetTexture(x, y + 1, baseTexture);
            SetTexture(x + 1, y, baseTexture);
            SetTexture(x + 1, y + 1, baseTexture);

            map.Tiles[x, y].BaseTexture = baseTexture.GetTileIndex(x, y);

            foreach (var t in overlayTextures)
            {
                noise.Frequency = t.Granularity;
                if (noise.Compute(x, y, info.MapId) > 0.5)
                {
                    SetTexture(x, y, t.Texture);
                    SetTexture(x, y + 1, t.Texture);
                    SetTexture(x + 1, y, t.Texture);
                    SetTexture(x + 1, y + 1, t.Texture);

                    map.Tiles[x, y].BaseTexture = t.Texture.GetTileIndex(x, y);
                }
            }
        }

        public void SetTexture(int x, int y, Texture texture)
        {
            if (textureData[x, y] == null ||
                textureData[x, y] == info.Settings.Scenery.BaseTexture && info.Settings.Scenery.OverlayTextures.Any(t => t.Texture == texture) ||
                textureData[x, y] == info.Settings.Scenery.CliffTexture && info.Settings.Scenery.CliffOverlayTextures.Any(t => t.Texture == texture))
            {
                textureData[x, y] = texture;
            }
        }

        private void GenerateBlendTiles()
        {
            map.Tiles.NumberOfBlendTiles = 1;
            map.Tiles.BlendTiles = new List<BlendTile>();

            for (int x = 0; x < map.Tiles.Width; x++)
            {
                for (int y = 0; y < map.Tiles.Height; y++)
                {
                    GenerateBlendTiles(x, y);
                }
            }
        }

        private void GenerateBlendTiles(int x, int y)
        {
            Texture[] t = new Texture[]
            {
                textureData[x, y],
                textureData[x + 1, y],
                textureData[x, y + 1],
                textureData[x + 1, y + 1]
            };

            int count = t.Distinct().Count();
            if (count == 1)
            {
                map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                return;
            }
            else if (count == 4 || (t[0] == t[3] && t[1] == t[2]))
            {
                textureData[x + 1, y + 1] = textureData[x + 1, y];
                GenerateBlendTiles(x, y);
                return;
            }

            if (t[0] == t[1])
            {
                if (t[0] == t[2])
                {
                    map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[3], BlendType.BottomLeftSmall);
                }
                else if (t[0] == t[3])
                {
                    map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[2], BlendType.BottomRightSmall);
                }
                else
                {
                    if (t[2] == t[3])
                    {
                        map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                        AddBlendTile(x, y, t[2], BlendType.Bottom);
                    }
                    else
                    {
                        map.Tiles[x, y].BaseTexture = t[3].GetTileIndex(x, y);
                        AddBlendTile(x, y, t[0], BlendType.Top);
                        AddBlendTile(x, y, t[2], BlendType.BottomRightSmall);
                    }
                }
            }
            else if (t[0] == t[2])
            {
                if (t[0] == t[3])
                {
                    map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[1], BlendType.TopLeftSmall);
                }
                else if (t[1] == t[3])
                {
                    map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[1], BlendType.Left);
                }
                else
                {
                    map.Tiles[x, y].BaseTexture = t[3].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[0], BlendType.Right);
                    AddBlendTile(x, y, t[1], BlendType.TopLeftSmall);
                }
            }
            else if (t[2] == t[3])
            {
                if (t[2] == t[1])
                {
                    map.Tiles[x, y].BaseTexture = t[2].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[0], BlendType.TopRightSmall);
                }
                else
                {
                    map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                    AddBlendTile(x, y, t[2], BlendType.Bottom);
                    AddBlendTile(x, y, t[1], BlendType.TopLeftSmall);
                }

            }
            else if (t[1] == t[3])
            {
                map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                AddBlendTile(x, y, t[1], BlendType.Left);
                AddBlendTile(x, y, t[2], BlendType.BottomRightSmall);
            }
            else if (t[0] == t[3])
            {
                map.Tiles[x, y].BaseTexture = t[0].GetTileIndex(x, y);
                AddBlendTile(x, y, t[1], BlendType.TopLeftSmall);
                AddBlendTile(x, y, t[2], BlendType.BottomRightSmall);
            }
            else
            {
                map.Tiles[x, y].BaseTexture = t[1].GetTileIndex(x, y);
                AddBlendTile(x, y, t[0], BlendType.TopRightSmall);
                AddBlendTile(x, y, t[3], BlendType.BottomLeftSmall);
            }
        }

        private void AddBlendTile(int x, int y, Texture texture, BlendType blendType)
        {
            short tileIndex = texture.GetTileIndex(x, y);
            if (tileIndex == map.Tiles[x, y].BaseTexture)
                return;
            short blendTileIndex = map.Tiles.GetBlendTileIndex(tileIndex, blendType);

            if (map.Tiles[x, y].BlendTexture1 == 0)
                map.Tiles[x, y].BlendTexture1 = blendTileIndex;
            else if (map.Tiles[x, y].BlendTexture2 == 0)
                map.Tiles[x, y].BlendTexture2 = blendTileIndex;
            else if (map.Tiles[x, y].BlendTexture3 == 0)
                map.Tiles[x, y].BlendTexture3 = blendTileIndex;
        }
    }
}
