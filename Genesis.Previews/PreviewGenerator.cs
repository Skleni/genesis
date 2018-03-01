#region License

// PreviewGenerator.cs
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
using System.Drawing;
using System.IO;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Core;
using Genesis.Tiles;

namespace Genesis.Previews
{
    public class PreviewGenerator : IMapProcessor
    {
        public PreviewGenerator()
        {
            AddObjects = true;
        }

        public string Description
        {
            get
            {
                return "Generating preview";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public bool AddObjects { get; set; }

        public readonly object Lock = new object();

        public byte[] TgaPreview { get; private set; }
        public byte[] BmpPreview { get; private set; }

        public void Process(Map map, MapInfo info)
        {
            Grid<byte> heightMap = new Grid<byte>(map.HeightMap.MapWidth, map.HeightMap.MapHeight);

            if (AddObjects)
            {
                foreach (var o in map.Objects)
                {
                    if (!o.IsWaypoint && o.RoadOptions == RoadOptions.None)
                    {
                        var coordinates = map.PositionToCoordinates(o.GetPosition());

                        if (info.Tiles[coordinates.X, coordinates.Y].HasFlag(TileInfo.Structure))
                            heightMap[coordinates.X - map.HeightMap.Border, coordinates.Y - map.HeightMap.Border] = 255;
                    }
                }
            }
            
            using (MemoryStream tga = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(tga);
                writer.Write((byte)0); // Id Length
                writer.Write((byte)0); // Cmap Type
                writer.Write((byte)10); // Image Type (TGA_RLERGB 10)

                writer.Write((UInt16)0); // Cmap Index
                writer.Write((UInt16)0); // Cmap Length
                writer.Write((byte)0); // Cmap Entry Size

                writer.Write((UInt16)0); // X Origin
                writer.Write((UInt16)0); // Y Origin
                writer.Write(Convert.ToUInt16(map.HeightMap.MapWidth)); // Image Width
                writer.Write(Convert.ToUInt16(map.HeightMap.MapHeight)); // Image Height
                writer.Write((byte)32); // Pixel Depth
                writer.Write((byte)0); // Imag Desc

                List<System.Drawing.Color> tgaPacket = new List<System.Drawing.Color>();

                for (int j = 0; j < heightMap.Height; j++) 
                {
                    for (int i = 0; i < heightMap.Width; i++)
                    {
                        byte height = map.HeightMap[i + map.HeightMap.Border, j + map.HeightMap.Border];

                        if (height > heightMap[i, j])
                            heightMap[i, j] = height;

                        Color pixel = Color.FromArgb(height, height, height);
                        tgaPacket.Add(pixel);
                        if (tgaPacket.Count == 128)
                            WriteTgaPacket(writer, tgaPacket);
                    }
                }
                WriteTgaPacket(writer, tgaPacket);

                lock (Lock)
                {
                    BmpPreview = heightMap.ToArray();
                    TgaPreview = tga.ToArray();
                }
            }
        }

        private void WriteTgaPacket(BinaryWriter writer, List<Color> packet)
        {
            if (packet.Count > 0)
            {
                writer.Write((byte)(packet.Count - 1));
                foreach (System.Drawing.Color pixel in packet)
                {
                    writer.Write((byte)pixel.B);
                    writer.Write((byte)pixel.G);
                    writer.Write((byte)pixel.R);
                    writer.Write((byte)pixel.A);
                }

                packet.Clear();
            }
        }
    }
}
