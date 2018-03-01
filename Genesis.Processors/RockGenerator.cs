#region License

// RockGenerator.cs
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
using Genesis.Settings;
using Genesis.Tiles;

namespace Genesis.Processors
{
    public class RockGenerator : IMapProcessor
    {
        private const TileInfo invalidTiles = TileInfo.Cliff | TileInfo.Road | TileInfo.Structure;

        private MapInfo info;
        private TileGraph tileGraph;

        public Frequency Frequency { get; set; }

        public RockGenerator()
        {
            Frequency = Frequency.Medium;
        }

        public string Description
        {
            get
            {
                return "Generating rocks";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            if (info.Settings.Scenery.Rocks.Count == 0)
                return;

            this.info = info;
            this.tileGraph = new TileGraph(info.Tiles, invalidTiles);

            int count;

            switch (Frequency)
            {
                case Frequency.High:
                    count = map.Tiles.Count / 1000;
                    break;
                case Frequency.Low:
                    count = map.Tiles.Count / 3000;
                    break;
                default:
                    count = map.Tiles.Count / 2000;
                    break;
            }

            Random random = new Random(info.MapId);

            int width = map.HeightMap.MapWidth * (int)Map.TileWidth;
            int height = map.HeightMap.MapHeight * (int)Map.TileWidth;

            while (count > 0 && !info.IsCancellationRequested)
            {
                float x = random.Next(width);
                float y = random.Next(height);
                
                var coordinates = map.PositionToCoordinates(new Point(x, y));

                if (info.IsWithinStartingRange(coordinates, 10))
                    continue;

                if (IsTileValid(coordinates.X, coordinates.Y))
                {
                    int rock = random.Next(info.Settings.Scenery.Rocks.Count);
                    map.Objects.Add(info.ObjectFactory.CreateObject(info.Settings.Scenery.Rocks[rock], x, y, 0, (float)(((random.NextDouble() * 2) - 1) * System.Math.PI)));
                    count--;
                }
            }
        }

        private bool IsTileValid(int x, int y)
        {
            if ((info.Tiles[x, y] & invalidTiles) != TileInfo.Free)
                return false;

            return !tileGraph.HasInvalidNeighbors(x, y, 1);
        }
    }
}
