#region License

// Flattener.cs
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

using System.Linq;
using CkMp.Data.Map;
using Genesis.Core;
using Genesis.Tiles;
using Genesis.Tools;

namespace Genesis.Processors
{
    public class Flattener : IMapProcessor
    {
        public string Description
        {
            get
            {
                return "Flattening key locations";
            }
        }

        public bool UpdatePreview
        {
            get { return true; }
        }

        public void Process(Map map, MapInfo info)
        {
            var graph = new TileGraph(info.Tiles, TileInfo.Free);
            var generator = new RampGenerator(info.MapId);
            foreach (var area in info.FlattenedAreas)
            {
                if (info.IsCancellationRequested)
                    return;

                var coordinates = map.PositionToCoordinates(area.Center);
                var height = (byte)graph.GetNeighbors(coordinates, (int)(area.Radius / Map.TileWidth) + 3, true, true).Average(c => map.HeightMap[c.X, c.Y]);
                generator.GenerateFlatArea(map, coordinates, height, (int)(area.Radius / Map.TileWidth), area.Radius);
            }
        }
    }
}
