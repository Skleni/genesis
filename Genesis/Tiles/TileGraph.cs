#region License

// TileGraph.cs
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
using CkMp.Data.Map;
using Genesis.Algorithms;

namespace Genesis.Tiles
{
    public class TileGraph
    {
        private bool valid;
        private bool invalid;

        public Grid<TileInfo> Tiles { get; set; }
        public TileInfo InvalidTiles { get; set; }

        public TileGraph(Grid<TileInfo> tiles, TileInfo invalidTiles)
        {
            Tiles = tiles;
            InvalidTiles = invalidTiles;
        }

        public IEnumerable<Coordinates> GetNeighbors(Coordinates coordinates, int radius, bool valid = true, bool invalid = false)
        {
            return GetNeighbors(coordinates.X, coordinates.Y, radius, valid, invalid);
        }

        public IEnumerable<Coordinates> GetNeighbors(int x, int y, int radius, bool valid = true, bool invalid = false)
        {
            this.valid = valid;
            this.invalid = invalid;

            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    if ((i != 0 || j != 0))
                    {
                        bool result = CheckCoordinates(x + i, y + j);
                        if ((result && valid) || (!result && invalid))
                            yield return new Coordinates(x + i, y + j);
                    }
                }
            }
        }

        public bool CheckCoordinates(int x, int y)
        {
            if (!Tiles.CheckCoordinates(x, y))
                return false;

            return (Tiles[x, y] & InvalidTiles) == TileInfo.Free;
        }

        public bool HasInvalidNeighbors(int x, int y, int radius)
        {
            return GetNeighbors(x, y, radius, false, true).Any();
        }

        public void UpdateTiles(IEnumerable<Coordinates> tiles, TileInfo info, int radius = 1)
        {
            foreach (var tile in tiles.SelectMany(t => GetNeighbors(t, radius)))
                Tiles[tile.X, tile.Y] |= info;
        }

        /// <summary>
        /// Gets the tiles along a path or null if any invalid tiles are encountered.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="path">The path.</param>
        public IEnumerable<Coordinates> GetTiles(Map map, IEnumerable<Point> path)
        {
            var bresenham = new Bresenham<Coordinates>((x, y) => new Coordinates(x, y));

            var lastPosition = path.First();

            var tiles = new List<Coordinates>();
            foreach (var position in path.Skip(1))
            {
                var start = map.PositionToCoordinates(position);
                var end = map.PositionToCoordinates(lastPosition);

                var coordinates = bresenham.GetCoordinates(start.X, start.Y, end.X, end.Y);
                if (coordinates.Any(c => !CheckCoordinates(c.X, c.Y)))
                {
                    return null;
                }
                else
                {
                    tiles.AddRange(coordinates);
                }

                lastPosition = position;
            }

            return tiles;
        }
    }
}
