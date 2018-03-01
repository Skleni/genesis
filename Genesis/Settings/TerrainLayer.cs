#region License

// TerrainLayer.cs
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
using System.ComponentModel;
using System.Xml.Serialization;

namespace Genesis.Settings
{    
    [Serializable]
    public class TerrainLayer
    {
        [XmlAttribute]
        public string Name { get; set; }
        [XmlAttribute]
        public short Height { get; set; }
        [XmlAttribute]
        public float Elevation { get; set; }
        [XmlAttribute]
        public float Jaggedness { get; set; }
        [XmlAttribute]
        public float Ruggedness { get; set; }
        [XmlAttribute]
        public int LevelOfDetail { get; set; }
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsCliff { get; set; }
        [XmlAttribute]
        [DefaultValue(false)]
        public bool IsDetail { get; set; }
    }
}
