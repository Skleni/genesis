#region License

// TerrainGenerator.cs
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
using System.Threading;
using CkMp.Data.Map;
using Genesis.Core;
using Genesis.Math;

namespace Genesis.Processors
{
    public class TerrainGenerator : IMapProcessor
    {
        public bool Detail { get; set; }

        public string Description
        {
            get
            {
                return "Generating terrain" + (Detail ? " details" : string.Empty);
            }
        }

        public bool UpdatePreview
        {
            get { return true; }
        }

        public void Process(Map map, MapInfo info)
        {
            var noise = new PerlinNoise(info.MapId);

            for (int i = 0; i < map.HeightMap.Width; i++)
            {
                for (int j = 0; j < map.HeightMap.Height; j++)
                {
                    foreach (var layer in info.Settings.Layout.TerrainLayers.Where(l => l.IsDetail == Detail))
                    {
                        if (info.IsCancellationRequested)
                            return;

                        noise.Amplitude = layer.Elevation;
                        noise.Frequency = layer.Jaggedness;
                        noise.Persistence = layer.Ruggedness;
                        noise.Octaves = layer.LevelOfDetail;

                        float normalizedHeight = noise.Compute(i, j, info.MapId);
                        float height = normalizedHeight * layer.Height;

                        map.HeightMap[i, j] += (byte)height;
                    }
                }
            }
        }
    }
}
