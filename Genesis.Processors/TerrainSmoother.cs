#region License

// TerrainSmoother.cs
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
using Genesis.Core;
using Genesis.Tiles;

namespace Genesis.Processors
{
    public class TerrainSmoother : IMapProcessor
    {
        private const byte maxHeightDifference = 15;
        private const TileInfo invalidTiles = TileInfo.FuelDepotNear | TileInfo.FlagNear | TileInfo.Road | TileInfo.Structure;

        private Map map;
        private MapInfo info;
        private TileGraph tileGraph;
        private Random random;

        public string Description
        {
            get
            {
                return "Smoothing terrain";
            }
        }

        public bool UpdatePreview
        {
            get { return true; }
        }

        public void Process(Map map, MapInfo info)
        {
            this.map = map;
            this.info = info;
            this.tileGraph = new TileGraph(info.Tiles, invalidTiles);
            this.random = new Random(info.MapId);

            for (int i = 0; i < map.HeightMap.Width - 1; i++)
            {
                for (int j = 1; j < map.HeightMap.Height - 1; j++)
                {
                    Flatten(i, j);
                }
            }    
        }

        private bool Flatten(int i, int j)
        {
            if ((info.Tiles[i, j] & invalidTiles) != TileInfo.Free)
            {
                return false;
            }

            byte height = map.HeightMap[i, j];
            byte minHeight = (byte)(System.Math.Max(height - maxHeightDifference, 0));
            byte maxHeight = (byte)(System.Math.Min(height + maxHeightDifference, 255));

            bool impassable = TruncateHeight(i, j + 1, minHeight, maxHeight);

            impassable |= TruncateHeight(i + 1, j - 1, minHeight, maxHeight);
            impassable |= TruncateHeight(i + 1, j, minHeight, maxHeight);
            impassable |= TruncateHeight(i + 1, j + 1, minHeight, maxHeight);

            if (impassable)
            {
                map.Tiles[i, j].Impassable = true;
                info.Tiles[i, j] |= TileInfo.Cliff;
            }

            return impassable;
        }

        private bool TruncateHeight(int x, int y, byte min, byte max)
        {
            if (!CheckTile(x, y))
                return false;

            if (!CheckNeighbors(x, y))
            {
                return false;
            }

            if (map.HeightMap[x, y] > max)
            {
                map.HeightMap[x, y] = (byte)(max - random.Next(5));
                map.Tiles[x, y].Impassable = true;
                info.Tiles[x, y] |= TileInfo.Cliff;
                return true;
            }
            else if (map.HeightMap[x, y] < min)
            {
                map.HeightMap[x, y] = (byte)(min + random.Next(5));
                map.Tiles[x, y].Impassable = true;
                info.Tiles[x, y] |= TileInfo.Cliff;
                return true;
            }

            return false;
        }

        private bool CheckNeighbors(int x, int y)
        {
            return
                
                CheckTile(x - 1, y) &&
                CheckTile(x + 1, y) &&
                CheckTile(x, y - 1) &&
                CheckTile(x, y + 1);

        }

        private bool CheckTile(int x, int y)
        {
            if (!info.Tiles.CheckCoordinates(x, y))
                return false;

            return (info.Tiles[x, y] & invalidTiles) == TileInfo.Free;
        }   
    }
}
