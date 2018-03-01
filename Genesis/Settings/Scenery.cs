#region License

// Scenery.cs
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
using CkMp.Data.Enumerations;
using CkMp.Data.Map;

namespace Genesis.Settings
{
    [Serializable]
    [XmlType]
    public class Scenery
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public Weather Weather { get; set; }

        [XmlElement("BaseTexture")]
        public TextureData BaseTextureData { get; set; }

        [XmlIgnore]
        public Texture BaseTexture { get { return BaseTextureData.Texture; } }

        [XmlArray("OverlayTextures")]
        public List<TextureData> OverlayTextures { get; set; }

        [XmlElement("CliffTexture")]
        public TextureData CliffTextureData { get; set; }

        [XmlIgnore]
        public Texture CliffTexture { get { return CliffTextureData.Texture; } }

        [XmlArray("CliffOverlayTextures")]
        public List<TextureData> CliffOverlayTextures { get; set; }

        [XmlArray("Trees")]
        [XmlArrayItem("Tree")]
        public List<string> Trees { get; set; }

        [XmlArray("Rocks")]
        [XmlArrayItem("Rock")]
        public List<string> Rocks { get; set; }

        [XmlElement("Road")]
        public string Road { get; set; }

        [XmlIgnore]
        public int NumberOfTiles
        {
            get
            {
                return BaseTexture.BlockCount +
                       OverlayTextures.Sum(t => t.Texture.BlockCount) +
                       CliffTexture.BlockCount +
                       CliffOverlayTextures.Sum(t => t.Texture.BlockCount);
            }
        }

        public Scenery()
        {
            OverlayTextures = new List<TextureData>();
            CliffOverlayTextures = new List<TextureData>();
            Trees = new List<string>();
            Rocks = new List<string>();
        }

        public void AddTexturesTo(TileData tileData)
        {
            tileData.AddTexture(BaseTexture);
            foreach (var texture in OverlayTextures)
		        tileData.AddTexture(texture.Texture);

            tileData.AddTexture(CliffTexture);
            foreach (var texture in CliffOverlayTextures)
                tileData.AddTexture(texture.Texture);
        }
    }
}
