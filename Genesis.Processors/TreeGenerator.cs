#region License

// TreeGenerator.cs
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
using System.Threading;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Core;
using Genesis.Settings;
using Genesis.Tiles;

namespace Genesis.Processors
{
    public class TreeGenerator : IMapProcessor
    {
        private const TileInfo invalidTiles = TileInfo.Cliff | TileInfo.Road | TileInfo.Structure;

        private MapInfo info;
        private TileGraph tileGraph;

        public string Description
        {
            get
            {
                return "Generating trees";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            if (info.Settings.Scenery.Trees.Count == 0)
                return;

            this.info = info;
            this.tileGraph = new TileGraph(info.Tiles, invalidTiles);

            int min, max;

            double formula = map.Tiles.Count / 300.0;
            switch (info.Settings.NumberOfTrees)
            {
                case Frequency.High:
                    min = max = (int)(formula * 6);
                    break;
                case Frequency.Low:
                    min = max = (int)(formula * 1.5);
                    break;
                default:
                    min = max = (int)(formula * 3);
                    break;
            }

            Random random = new Random(info.MapId);

            IList<ScriptObject> trees = new List<ScriptObject>();

            int width = map.HeightMap.MapWidth * (int)Map.TileWidth;
            int height = map.HeightMap.MapHeight * (int)Map.TileWidth;

            int count = random.Next(min, max + 1);
            while (trees.Count < count && !info.IsCancellationRequested)
            {
                float x = random.Next(width);
                float y = random.Next(height);
                for (int j = 0; j < random.Next(20); j++)
                {
                    float dx = (float)(random.NextDouble() * 40) + 10;
                    float dy = (float)(random.NextDouble() * 40) + 10;

                    int direction = random.Next(3);
                    switch (direction)
                    {
                        case 1:
                            dx *= -1;
                            break;
                        case 2:
                            dy *= -1;
                            break;
                    }

                    x = (x + dx + width) % width;
                    y = (y + dy + height) % height;

                    var coordinates = map.PositionToCoordinates(new Point(x, y));

                    if (info.IsWithinStartingRange(coordinates, 10))
                        continue;

                    if (IsTileValid(coordinates.X, coordinates.Y))
                    {
                        int tree = random.Next(info.Settings.Scenery.Trees.Count);
                        trees.Add(info.ObjectFactory.CreateObject(info.Settings.Scenery.Trees[tree], x, y, 0, (float)(((random.NextDouble() * 2) - 1) * System.Math.PI)));
                    }
                }
            }

            for (int i = 0; i < trees.Count - 1; i++)
            {
                if (trees[i] != null)
                {
                    for (int j = i + 1; j < trees.Count; j++)
                    {
                        if (trees[j] != null && trees[i].SquaredDistanceOnGround(trees[j]) < 100)
                            trees[j] = null;
                    }
                    map.Objects.Add(trees[i]);
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
