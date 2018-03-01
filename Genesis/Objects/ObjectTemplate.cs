#region License

// ObjectTemplate.cs
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
using CkMp.Data;
using CkMp.Data.Map;
using CkMp.Data.Objects;

namespace Genesis.Objects
{
    public class ObjectTemplate
    {
        public const string RoadTemplate = "TwoLane";
        public const string TextureTemplate = "AsphaltType1";
        private Point origin;

        public IEnumerable<ScriptObject> Objects { get; set; }
        public IEnumerable<Point> BlockedTiles { get; set; }
        public bool IsInstantiated { get; internal set; }
        public TileData Tiles { get; set; }
        public Map Map { get; set; }

        public ObjectTemplate(IEnumerable<ScriptObject> objects, IEnumerable<Point> blockedTiles)
        {
            IsInstantiated = false;
            Objects = objects;
            BlockedTiles = blockedTiles;
        }

        private void CalculatePositions()
        {
            if (Objects.Any())
            {
                Point origin = CalculateOrigin();

                foreach (var obj in Objects)
                {
                    obj.X -= origin.X;
                    obj.Y -= origin.Y;
                }
            }

            BlockedTiles = BlockedTiles.Select(t => t - origin).ToArray();
        }

        public Point CalculateOrigin()
        {
            var nonRoads = Objects.Where(o => o.RoadOptions == RoadOptions.None);
            origin = new Point(nonRoads.Average(o => o.X), nonRoads.Average(o => o.Y));

            return origin;
        }

        public ObjectTemplate Clone()
        {
            return new ObjectTemplate(
                Objects.Select(original =>
                {
                    var clone = new ScriptObject()
                    {
                        X = original.X,
                        Y = original.Y,
                        Z = original.Z,
                        Rotation = original.Rotation,
                        RoadOptions = original.RoadOptions,
                        Type = original.Type
                    };

                    foreach (var property in original.Properties)
                    {
                        clone.Properties.Add(property.Key, new Property()
                        {
                            Name = property.Value.Name,
                            Type = property.Value.Type,
                            Value = property.Value.Value
                        });
                    }

                    return clone;
                }).ToArray(),
                BlockedTiles.Select(t => new Point(t.X, t.Y)).ToArray()) { Tiles = Tiles, Map = Map };
        }

        public static ObjectTemplate ReadFromFile(string fileName)
        {
            var reader = new Reader();
            reader.ReadFile(fileName);

            return new ObjectTemplate(
                reader.Map.Objects,
                GetImpassableCoordinates(reader.Map))
                {
                    Tiles = reader.Map.Tiles,
                    Map = reader.Map
                };
        }

        private static IEnumerable<Point> GetImpassableCoordinates(Map map)
        {
            for (int x = 0; x < map.Tiles.Width; x++)
            {
                for (int y = 0; y < map.Tiles.Height; y++)
                {
                    if (map.Tiles[x, y].Impassable)
                        yield return map.CoordinatesToPosition(new Coordinates(x, y));
                }
            }
        }
    }
}
