#region License

// ParticleCliffGenerator.cs
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
using CkMp.Data.Map;
using Genesis.Core;
using Genesis.Settings;
using Genesis.Tiles;

namespace Genesis.Processors
{
    /// <summary>
    /// Generates cliffs by dropping particles.
    /// </summary>
    public class ParticleCliffGenerator : IMapProcessor
    {
        /// <summary>
        /// All tiles that prevent the dropping of particles.
        /// </summary>
        private const TileInfo invalidTiles = TileInfo.Base | TileInfo.FlagNear | TileInfo.FuelDepotNear | TileInfo.Road | TileInfo.Structure;

        /// <summary>
        /// Radius used to check if a particle can be dropped on a tile.
        /// </summary>
        private const int invalidTileRadius = 3;

        public const byte MaxHeightDifferencePerTile = 15;

        // magic numbers used to determine the number of cliffs based on the map size
        private const double lowDivisor = 9000;
        private const double mediumDivisor = 5000;
        private const double highDivisor = 3500;

        private const int minIterations = 500;
        private const int maxIterations = 1500;

        private Random random;
        private Map map;
        private MapInfo info;
        private TileGraph tileGraph;

        private Grid<byte> heightConstraints;

        public string Description
        {
            get
            {
                return "Generating mountains";
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

            this.heightConstraints = new Grid<byte>(map.HeightMap.Width, map.HeightMap.Height);
            for (int i = 0; i < map.Tiles.Width; i++)
            {
                for (int j = 0; j < map.Tiles.Height; j++)
                {
                    heightConstraints[i, j] = 255;

                    var invalidNeighbors = tileGraph.GetNeighbors(i, j, invalidTileRadius, false, true);
                    foreach (var neighbor in invalidNeighbors)
                    {
                        if (map.HeightMap.CheckCoordinates(neighbor.X, neighbor.Y))
                        {
                            int distance = System.Math.Max(System.Math.Abs(neighbor.X - i), System.Math.Abs(neighbor.Y - j));
                            heightConstraints[i, j] = (byte)System.Math.Min(heightConstraints[i, j], map.HeightMap[neighbor.X, neighbor.Y] + (distance - 1) * MaxHeightDifferencePerTile);
                        }
                    }
                }
            }

            int min, max;
            
            switch (info.Settings.NumberOfCliffs)
            {
                case Frequency.Low:
                    min = 0;
                    max = (int)(map.Tiles.Count / lowDivisor);
                    break;
                case Frequency.High:
                    min = (int)(map.Tiles.Count / mediumDivisor);
                    max = (int)(map.Tiles.Count / highDivisor);
                    break;
                default:
                    min = (int)(map.Tiles.Count / lowDivisor);
                    max = (int)(map.Tiles.Count / mediumDivisor);
                    break;
            }

            int count = random.Next(min, max);
            
            for (int i = 0; i < count && !info.IsCancellationRequested; i++)
                AddCliff();
        }

        private void AddCliff()
        {
            int iterations = random.Next(maxIterations - minIterations) + minIterations;

            int width = map.HeightMap.Width;
            int height = map.HeightMap.Height;

            int i, j;
            do
            {
                i = random.Next(width);
                j = random.Next(height);
            }
            while (map.HeightMap[i, j] >= heightConstraints[i, j]);

            for (int n = 0; n < iterations; n++)
            {
                DropParticles(i - 1, j - 1);
                DropParticles(i - 1, j);
                DropParticles(i - 1, j + 1);

                DropParticles(i, j - 1);
                DropParticles(i, j);
                DropParticles(i, j + 1);

                DropParticles(i + 1, j + 1);
                DropParticles(i + 1, j);
                DropParticles(i + 1, j - 1);

                DropParticles(i, j);

                i += random.Next(3) - 1;
                j += random.Next(3) - 1;
            }
        }

        //public bool IsPositionValid(int i, int j)
        //{
        //    // If this is already a cliff, the tile has been checked before 
        //    // and we can skip the expensive check of the neighbors.
        //    if ((info.Tiles[i, j] & TileInfo.Cliff) == TileInfo.Cliff)
        //        return true;

        //    if ((info.Tiles[i, j] & invalidTiles) != TileInfo.Free)
        //        return false;

        //    // Check if there are any invalid neighbors.
        //    return tileGraph.HasInvalidNeighbors(i, j, invalidTileRadius);
        //    //var invalidNeighbors = tileGraph.GetNeighbors(i, j, invalidTileRadius, false, true);

        //    //foreach (var neighbor in invalidNeighbors)
        //    //{
        //    //    int distance = System.Math.Max(System.Math.Abs(neighbor.X - i), System.Math.Abs(neighbor.Y - j));
        //    //    if (distance <= 1)
        //    //        return false;
        //    //}

        //    //if (invalidNeighbors.Any())
        //    //{
        //    //    minHeight = invalidNeighbors.Max(c => System.Math.Max(System.Math.Abs(c.X - i), System.Math.Max(c.Y - j)));
        //    //    return false;
        //    //}
        //    //else
        //    //    return true;
        //}

        private void DropParticles(int x, int y) 
        {
            map.HeightMap.WrapCoordinates(ref x, ref y);
            //if (!IsPositionValid(x, y))
            //    return;

            byte height = map.HeightMap[x, y];

            int index;

            foreach (Coordinates neighbor in tileGraph.GetNeighbors(x, y, 1))
            {
                //if (!IsPositionValid(neighbor.X, neighbor.Y))
                //    continue;

                index = map.HeightMap.Index(neighbor.X, neighbor.Y);

                if (map.HeightMap[index] < height)
                {
                    DropParticle(index);
                    return;
                }
            }

            index = map.HeightMap.Index(x, y);
            DropParticle(index);
        }

        private void DropParticle(int index)
        {
            if ((info.Tiles[index] & invalidTiles) != TileInfo.Free)
                return;



            int height = map.HeightMap[index] + random.Next(3) + 1;
            if (height <= heightConstraints[index])
            {
                info.Tiles[index] |= TileInfo.Cliff;
                map.Tiles[index].Impassable = true;
                map.HeightMap[index] = (byte)height;
                
            }
        }
    }
}
