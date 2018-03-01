#region License

// StartingPositionFlattener.cs
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

using System.Threading;
using CkMp.Data.Map;
using Genesis.Core;
using Genesis.Tools;

namespace Genesis.Processors
{
    public class StartingPositionFlattener : IMapProcessor
    {
        public int Radius { get; set; }

        public StartingPositionFlattener()
        {
            Radius = 20;
        }

        public string Description
        {
            get
            {
                return "Flattening starting positions";
            }
        }

        public bool UpdatePreview
        {
            get { return true; }
        }

        public void Process(Map map, MapInfo info)
        {
            var generator = new RampGenerator(info.MapId);
            foreach (var startingPosition in info.StartingPositions)
            {
                if (!info.IsCancellationRequested)
                {
                    //data.FlattenedAreas.Add(new Math.Circle(map.CoordinatesToPosition(startingPosition), data.BaseRadius * Map.TileWidth));
                    generator.GenerateFlatArea(map, startingPosition, map.HeightMap[startingPosition.X, startingPosition.Y], info.BaseRadius, Radius * Map.TileWidth);
                }
            }
        }
    }
}
