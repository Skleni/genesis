#region License

// GlobalLightOptions.cs
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

using CkMp.Data.Enumerations;

namespace CkMp.Data.Lighting
{
    public class GlobalLightOptions
    {
        public TimeOfDay TimeOfDay { get; set; }

        public LightOptions Morning { get; set; }
        public LightOptions Afternoon { get; set; }
        public LightOptions Evening { get; set; }
        public LightOptions Night { get; set; }

        public int GetSize(bool includingHeader)
        {
            return 1 + // TimeOfDay
                   3 + // null
                   Morning.GetSize() +
                   Afternoon.GetSize() +
                   Evening.GetSize() +
                   Night.GetSize() +
                   4 +
                   (includingHeader ? 10 : 0);
        }

        public static GlobalLightOptions GetDefault()
        {
            return new GlobalLightOptions()
            {
                TimeOfDay = TimeOfDay.Afternoon,
                Morning = new LightOptions()
                {
                    ObjectAmbientColor = new Color() { R = 0.5f, G = 0.4f, B = 0.3f },
                    TerrainAmbientColor = new Color() { R = 0.5f, G = 0.4f, B = 0.3f },
                    ObjectsSun = new Light()
                    {
                        Color = new Color() { R = 0.9f, G = 0.7f, B = 0.6f },
                        Vector = new Vector() { X = -0.96f, Y = 0.05f, Z = -0.28f }
                    },
                    TerrainSun = new Light()
                    {
                        Color = new Color() { R = 0.9f, G = 0.7f, B = 0.6f },
                        Vector = new Vector() { X = -0.96f, Y = 0.05f, Z = -0.28f }
                    },
                    ObjectsAccent1 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    TerrainAccent1 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    ObjectsAccent2 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    TerrainAccent2 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    }
                },
                Afternoon = new LightOptions()
                {
                    ObjectAmbientColor = new Color() { R = 0.22f, G = 0.2f, B = 0.17f },
                    TerrainAmbientColor = new Color() { R = 0.22f, G = 0.2f, B = 0.17f },
                    ObjectsSun = new Light()
                    {
                        Color = new Color() { R = 1f, G = 1f, B = 1f },
                        Vector = new Vector() { X = -0.81f, Y = 0.38f, Z = -0.45f }
                    },
                    TerrainSun = new Light()
                    {
                        Color = new Color() { R = 1f, G = 1f, B = 1f },
                        Vector = new Vector() { X = -0.81f, Y = 0.38f, Z = -0.45f }
                    },
                    ObjectsAccent1 = new Light()
                    {
                        Color = new Color() { R = 0.23f, G = 0.23f, B = 0.31f },
                        Vector = new Vector() { X = 0.79f, Y = 0.62f, Z = 0f }
                    },
                    TerrainAccent1 = new Light()
                    {
                        Color = new Color() { R = 0.23f, G = 0.23f, B = 0.47f },
                        Vector = new Vector() { X = 0.79f, Y = 0.62f, Z = 0f }
                    },
                    ObjectsAccent2 = new Light()
                    {
                        Color = new Color() { R = 0.12f, G = 0.12f, B = 0.08f },
                        Vector = new Vector() { X = 0.81f, Y = -0.48f, Z = -0.34f }
                    },
                    TerrainAccent2 = new Light()
                    {
                        Color = new Color() { R = 0.12f, G = 0.12f, B = 0.08f },
                        Vector = new Vector() { X = 0.81f, Y = -0.48f, Z = -0.34f }
                    }
                },
                Evening = new LightOptions()
                {
                    ObjectAmbientColor = new Color() { R = 0.25f, G = 0.23f, B = 0.2f },
                    TerrainAmbientColor = new Color() { R = 0.25f, G = 0.23f, B = 0.2f },
                    ObjectsSun = new Light()
                    {
                        Color = new Color() { R = 0.6f, G = 0.5f, B = 0.4f },
                        Vector = new Vector() { X = -1f, Y = 0f, Z = -0.2f }
                    },
                    TerrainSun = new Light()
                    {
                        Color = new Color() { R = 0.6f, G = 0.5f, B = 0.4f },
                        Vector = new Vector() { X = -1f, Y = 0f, Z = -0.2f }
                    },
                    ObjectsAccent1 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    TerrainAccent1 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    ObjectsAccent2 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    TerrainAccent2 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    }
                },
                Night = new LightOptions()
                {
                    ObjectAmbientColor = new Color() { R = 0.1f, G = 0.1f, B = 0.15f },
                    TerrainAmbientColor = new Color() { R = 0.1f, G = 0.1f, B = 0.15f },
                    ObjectsSun = new Light()
                    {
                        Color = new Color() { R = 0.2f, G = 0.2f, B = 0.3f },
                        Vector = new Vector() { X = -1f, Y = 1f, Z = -2f }
                    },
                    TerrainSun = new Light()
                    {
                        Color = new Color() { R = 0.2f, G = 0.2f, B = 0.3f },
                        Vector = new Vector() { X = -1f, Y = 1f, Z = -2f }
                    },
                    ObjectsAccent1 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    TerrainAccent1 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    ObjectsAccent2 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    },
                    TerrainAccent2 = new Light()
                    {
                        Color = new Color(),
                        Vector = new Vector() { X = 0f, Y = 0f, Z = -1f }
                    }
                }
            };
        }
    }
}
