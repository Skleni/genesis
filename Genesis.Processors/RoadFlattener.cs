#region License

// RoadFlattener.cs
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
using Genesis.Core;
using Genesis.Roads;
using Genesis.Tiles;
using Genesis.Tools;

namespace Genesis.Processors
{
    public class RoadFlattener : IMapProcessor
    {
        private const int crossroadFlatteningRadius = 5; // in tiles
        private const int overlapRadius = 2; // in tiles

        private Map map;
        private MapInfo info;
        private RampGenerator rampGenerator;

        public bool AddWaypaths { get; set; }

        public string Description
        {
            get
            {
                return "Flattening roads";
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
            this.rampGenerator = new RampGenerator(info.MapId);

            var crossroads = from n in info.Roads.Nodes
                             where n.Neighbors.Count > 2
                             select n;

            foreach (var crossroad in crossroads)
            {
                var coordinates = map.PositionToCoordinates(crossroad.Position);
                rampGenerator.GenerateFlatArea(map, coordinates, map.HeightMap[coordinates.X, coordinates.Y], crossroadFlatteningRadius, 15);
            }

            rampGenerator.RandomOffsetSeed = null;

            var visited = new List<RoadGraphNode>();

            var nodes = from n in info.Roads.Nodes
                        let coordinates = map.PositionToCoordinates(n.Position)
                        orderby map.HeightMap[coordinates.X, coordinates.Y]
                        select n;

            foreach (var node in nodes)
            {
                if (info.IsCancellationRequested)
                    return;

                foreach (var neighbor in node.Neighbors.Except(visited))
                {
                    FlattenRoad(node, neighbor);
                }
                visited.Add(node);
            }

            // try to fix visual artifacts
            for (int x = 0; x < info.Tiles.Width; x++)
            {
                for (int y = 0; y < info.Tiles.Height; y++)
                {
                    if (map.HeightMap[x, y] > 1 && info.Tiles[x, y].HasFlag(TileInfo.Road))
                        map.HeightMap[x, y] -= 2;
                }
            }
        }

        private void FlattenRoad(RoadGraphNode node1, RoadGraphNode node2)
        {
            Point start = node1.Position;
            Point end = node2.Position;
            var vector = (end - start).Normalize();

            if (node1.Neighbors.Count > 2)
            {
                start += vector * Map.TileWidth * crossroadFlatteningRadius;
            }
            else
            {
                start -= vector * Map.TileWidth * overlapRadius;
            }

            if (node2.Neighbors.Count > 2)
            {
                end -= vector * Map.TileWidth * crossroadFlatteningRadius;
            }
            else 
            {
                end += vector * Map.TileWidth * overlapRadius;
            }

            rampGenerator.GenerateRamp(map, start, end, 30, 15);
            if (AddWaypaths)
            {
                var path = new[] { info.ObjectFactory.CreateWaypoint(start.X, start.Y), info.ObjectFactory.CreateWaypoint(end.X, end.Y) };
                map.AddWaypath(string.Empty, path);
            }
        }
    }
}
