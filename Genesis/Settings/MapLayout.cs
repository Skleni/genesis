#region License

// MapLayout.cs
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
using System.Linq;
using System.Xml.Serialization;

namespace Genesis.Settings
{
    [Serializable]
    [XmlType]
    public class MapLayout
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public byte Height { get; set; }

        public List<TerrainLayer> TerrainLayers { get; set; }
        
        public bool HasValidHeights
        {
            get
            {
                int maxHeight = Height + TerrainLayers.Sum(l => System.Math.Max((int)l.Height, 0));
                int minHeight = Height - TerrainLayers.Sum(l => System.Math.Min((int)l.Height, 0));
                return minHeight >= 0 & maxHeight < 256;
            }
        }
        
        public MapLayout()
        {
            TerrainLayers = new List<TerrainLayer>();
        }
    }
}
