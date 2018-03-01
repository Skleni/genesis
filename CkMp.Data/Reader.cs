#region License

// Reader.cs
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
using System.Diagnostics;
using System.IO;
using System.Text;
using CkMp.Data.Enumerations;
using CkMp.Data.Lighting;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using CkMp.Data.Scripts;

namespace CkMp.Data
{
    public class Reader
    {
        private int indention;
        private BinaryReader reader;

        public String[] Strings { get; protected set; }
        public Map.Map Map { get; protected set; }

        private void InitializeVariables()
        {
            indention = 0;
            Map = new Map.Map();
        }   

        public void ReadFile(String fileName)
        {
            ReadStream(File.OpenRead(fileName));
        }

        public void ReadStream(Stream stream)
        {
            InitializeVariables();

            reader = new BinaryReader(stream);

            ReadCkMp();
            ReadStrings();

            while (reader.BaseStream.Position < reader.BaseStream.Length)
                ReadSection();

            reader.Close();
        }

        private void ReadCkMp()
        {
            Assert(reader.ReadChar() == 'C');
            Assert(reader.ReadChar() == 'k');
            Assert(reader.ReadChar() == 'M');
            Assert(reader.ReadChar() == 'p');
            Write("File header", "CkMp");
        }

        private void ReadStrings()
        {
            Write("Dictionary");
            indention++;

            int length = reader.ReadInt32();
            Strings = new String[length + 1];
            for (int i = 0; i < length; i++)
            {
                string s = ReadOneByteString(false);
                int index = reader.ReadInt32();
                Strings[index] = s;
                Write(index.ToString(), s);
            }
            indention--;
        }        

        private void ReadSection()
        {
            var header = ReadHeader();

            switch (Strings[header.Index])
            {
                case "HeightMapData":
                    Assert(header.Magic == 4);
                    ReadHeightMap();
                    break;
                case "BlendTileData":
                    Assert(header.Magic == 8);
                    ReadBlendTileData();
                    break;
                case "SidesList":
                    ReadSidesList();
                    break;
                case "GlobalLighting":
                    Assert(header.Magic == 3);
                    ReadGlobalLighting();
                    break;
                case "WorldInfo":
                    Assert(header.Magic == 1);
                    ReadWorldInfo();
                    break;
                case "PlayerScriptsList":
                    Assert(header.Magic == 5);
                    ReadPlayerScriptsList(header.Length);
                    break;
                case "ScriptsPlayers":
                    Assert(header.Magic == 2);
                    ReadScriptsPlayers();
                    break;
                case "ObjectsList":
                    Assert(header.Magic == 3);
                    ReadObjectsList(header.Length);
                    break;
                case "PolygonTriggers":
                    Assert(header.Magic == 3 || header.Magic == 4);
                    ReadPolygoneTriggers();
                    break;
                case "ScriptTeams":
                    Assert(header.Magic == 1);
                    ReadScriptTeams(header.Length);
                    break;
                case "WaypointsList":
                    Assert(header.Magic == 1);
                    ReadWaypointsList();
                    break;
            }
        }

        private void Assert(bool condition)
        {
#if DEBUG
            Debug.Assert(condition);
#else
            if (!condition)
                throw new InvalidDataException();
#endif
        }

        #region map

        private void ReadHeightMap() 
        {
            Write("HeightMap");
            int totalWidth = reader.ReadInt32();
            int totalHeight = reader.ReadInt32();
            int border = reader.ReadInt32();

            int magic = reader.ReadInt32();
            Assert(magic == 1);

            int width = reader.ReadInt32();
            int height = reader.ReadInt32();

            // Can be different when map is resized manually:

            //if (border > 0)
            //{
            //    Assert(totalWidth == width + 2 * border);
            //    Assert(totalHeight == height + 2 * border);
            //}
            //else
            //{
            //    Assert(totalWidth == width + 1);
            //    Assert(totalHeight == height + 1);
            //}
            
            Map.HeightMap = new HeightMap(totalWidth, totalHeight, border);

            int count = reader.ReadInt32();
            Assert(count == totalWidth * totalHeight);

            for (int i = 0; i < count; i++)
                Map.HeightMap[i] = reader.ReadByte();
        }

        private void ReadBlendTileData()
        {
            Write("BlendTileData");
            Map.Tiles = new TileData(Map.HeightMap.Width, Map.HeightMap.Height);
            
            int count = reader.ReadInt32();
            Assert(count == Map.HeightMap.Width * Map.HeightMap.Height);

            for (int i = 0; i < count; i++)
                Map.Tiles[i].BaseTexture = reader.ReadInt16();

            for (int i = 0; i < count; i++)
                Map.Tiles[i].BlendTexture1 = reader.ReadInt16();

            for (int i = 0; i < count; i++)
                Map.Tiles[i].BlendTexture2 = reader.ReadInt16();

            for (int i = 0; i < count; i++)
                Map.Tiles[i].BlendTexture3 = reader.ReadInt16();

            ReadImpassability();

            Map.Tiles.NumberOfBaseTiles = reader.ReadInt32();
            Map.Tiles.NumberOfBlendTiles = reader.ReadInt32();
            
            int magic = reader.ReadInt32();
            Assert(magic == 1);

            ReadTextures();
            ReadBlendTiles();
        }

        private void ReadImpassability()
        {
            // For every set of 8 horizontal tiles the impassability is encoded in a byte,
            // starting with a new byte in every new row.

            byte impassability = 0;
            for (int i = 0; i < Map.Tiles.Height; i++)
            {
                for (int j = 0; j < Map.Tiles.Width; j++)
                {
                    if (j % 8 == 0)
                        impassability = reader.ReadByte();
                    
                    Map.Tiles[j, i].Impassable = ((impassability >> (j % 8)) & 1) == 1;
                }
            }
        }

        private void ReadTextures()
        {
            int count = reader.ReadInt32();
            Map.Tiles.Textures = new List<Texture>(count);

            for (int i = 0; i < count; i++)
            {
                var texture = new Texture();
                texture.BlockStartIndex = reader.ReadInt32();
                texture.BlockCount = reader.ReadInt32();
                texture.BlocksPerRow = reader.ReadInt32();
                Assert(texture.BlockCount == texture.BlocksPerRow * texture.BlocksPerRow);
                texture.Magic = reader.ReadInt32();
                Assert(texture.Magic == 0);

                texture.Name = ReadOneByteString(true);
                
                Map.Tiles.Textures.Add(texture);
            }

            ReadNullBytes(8);
        }

        private void ReadBlendTiles()
        {
            Map.Tiles.BlendTiles = new List<BlendTile>(Map.Tiles.NumberOfBlendTiles);
            
            //Start at 1! NumberOfBlendTiles is always 1 more than the actual number (?).
            for (int i = 1; i < Map.Tiles.NumberOfBlendTiles; i++)
            {
                var blendTile = new BlendTile();
                blendTile.TileIndex = reader.ReadInt16();
                blendTile.BlendType = (BlendType)reader.ReadInt64();
                long blendData = reader.ReadInt64();
                Assert(blendData == BlendTile.BlendData);

                Map.Tiles.BlendTiles.Add(blendTile);
            }
        }

        private void ReadWorldInfo() 
        {
            Write("WorldInfo");
            Map.Properties = ReadProperties();
        }

        #endregion

        #region Lighting

        private void ReadGlobalLighting()
        {
            Write("GlobalLighting");
            Map.GlobalLightOptions = new GlobalLightOptions();

            Map.GlobalLightOptions.TimeOfDay = (TimeOfDay)reader.ReadByte();
            ReadNullBytes(3);

            Map.GlobalLightOptions.Morning = ReadLightOptions();
            Map.GlobalLightOptions.Afternoon = ReadLightOptions();
            Map.GlobalLightOptions.Evening = ReadLightOptions();
            Map.GlobalLightOptions.Night = ReadLightOptions();

            Assert(reader.ReadByte() == 160);
            Assert(reader.ReadByte() == 160);
            Assert(reader.ReadByte() == 160);
            Assert(reader.ReadByte() == 127);
        }

        private LightOptions ReadLightOptions()
        {
            var options = new LightOptions();
            options.ObjectAmbientColor = ReadColor();
            options.ObjectsSun = ReadLight();
            options.TerrainAmbientColor = ReadColor();
            options.TerrainSun = ReadLight();
            ReadNullBytes(12);
            options.ObjectsAccent1 = ReadLight();
            ReadNullBytes(12);
            options.ObjectsAccent2 = ReadLight();
            ReadNullBytes(12);
            options.TerrainAccent1 = ReadLight();
            ReadNullBytes(12);
            options.TerrainAccent2 = ReadLight();

            return options;
        }

        private Light ReadLight()
        {
            return new Light()
            {
                Color = ReadColor(),
                Vector = ReadVector()
            };
        }

        private Color ReadColor()
        {
            return new Color()
            {
                R = reader.ReadSingle(),
                G = reader.ReadSingle(),
                B = reader.ReadSingle()
            };
        }

        private Vector ReadVector()
        {
            return new Vector()
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle()
            };
        }

        #endregion

        #region scripts

        private void ReadPlayerScriptsList(int length)
        {
            Write("PlayerScriptList");
            indention++;
            long start = reader.BaseStream.Position;
            while (reader.BaseStream.Position < start + length)
                Map.PlayerScripts.Add(ReadScriptList());
            indention--;
        }

        private List<ScriptBase> ReadScriptList()
        {
            Write("ScriptList");
            var list = new List<ScriptBase>();

            var header = ReadHeader();
            Assert(header.Magic == 1);
            
            indention++;
            long start = reader.BaseStream.Position;
            while (reader.BaseStream.Position < start + header.Length)
            {
                var elementHeader = ReadHeader();
                switch (Strings[elementHeader.Index])
                {
                    case "ScriptGroup":
                        list.Add(ReadScriptGroup(elementHeader.Length));
                        break;
                    case "Script":
                        list.Add(ReadScript(elementHeader.Length));
                        break;
                }
             
            }
            indention--;
            return list;
        }

        private ScriptGroup ReadScriptGroup(int length)
        {
            var group = new ScriptGroup();

            long start = reader.BaseStream.Position;
            group.Name = ReadString();
            Write("ScriptGroup", group.Name);
            group.IsActive = reader.ReadBoolean();
            group.IsSubroutine = reader.ReadBoolean();
            
            indention++;
            while (reader.BaseStream.Position < start + length)
            {
                var header = ReadHeader();
                Assert(header.Magic == 2);
                group.Scripts.Add(ReadScript(header.Length));
            }
            indention--;

            return group;
        }

        private Script ReadScript(int length)
        {
            var script = new Script();

            long start = reader.BaseStream.Position;

            script.Name = ReadString();
            Write("Script", script.Name);
            script.Comment = ReadString();
            script.ConditionComment = ReadString();
            script.ActionComment = ReadString();
            script.IsActive = reader.ReadBoolean();
            script.Deactivate = reader.ReadBoolean();
            script.Easy = reader.ReadBoolean();
            script.Normal = reader.ReadBoolean();
            script.Hard = reader.ReadBoolean();
            script.IsSubroutine = reader.ReadBoolean();
            script.Interval = reader.ReadInt32();

            indention++;
            while (reader.BaseStream.Position < start + length)
            {
                var elementHeader = ReadHeader();

                switch (Strings[elementHeader.Index])
                {
                    case "OrCondition":
                        Assert(elementHeader.Magic == 1);
                        script.OrConditions.Add(ReadOrCondition(elementHeader.Length));
                        break;
                    case "ScriptAction":
                        Assert(elementHeader.Magic == 2);
                        script.ActionsIfTrue.Add(ReadScriptElement());
                        break;
                    case "ScriptActionFalse":
                        Assert(elementHeader.Magic == 2);
                        script.ActionsIfFalse.Add(ReadScriptElement());
                        break;
                }
            }
            indention--;

            return script;
        }

        private OrCondition ReadOrCondition(int length)
        {
            Write("OrCondition");
            var condition = new OrCondition();

            indention++;
            long start = reader.BaseStream.Position;
            while (reader.BaseStream.Position < start + length)
            {
                var header = ReadHeader();
                Assert(header.Magic == 4);
                condition.Conditions.Add(ReadScriptElement());
            }
            indention--;

            return condition;
        }

        private ScriptElement ReadScriptElement()
        {
            var element = new ScriptElement();
            element.Id = reader.ReadInt32();
            Assert(reader.ReadByte() == 3);
            short index = reader.ReadInt16();
            ReadNullBytes(1);
            element.Name = Strings[index];
            Write(element.Name);
            int parameters = reader.ReadInt32();
            indention++;
            for (int i = 0; i < parameters; i++)
                element.Parameters.Add(ReadParameter());
            indention--;
            return element;
        }

        private Parameter ReadParameter()
        {
            var parameter = new Parameter();
            parameter.Type = (ParameterType)reader.ReadInt32();

            switch (parameter.Type)
            {
                case ParameterType.Boolean:
                    parameter.Value = reader.ReadBoolean();
                    ReadNullBytes(9);
                    break;
                case ParameterType.Integer:
                    parameter.Value = reader.ReadInt32();
                    ReadNullBytes(6);
                    break;
                case ParameterType.Float:
                case ParameterType.Angle:
                case ParameterType.Percent:
                    ReadNullBytes(4);
                    parameter.Value = reader.ReadSingle();
                    ReadNullBytes(2);
                    break;
                case ParameterType.Location:
                    parameter.Value = new float[]
                    {
                        reader.ReadSingle(),
                        reader.ReadSingle(),
                        reader.ReadSingle()
                    };
                    break;
                case ParameterType.Color:
                    parameter.Value = new byte[]
                    {
                        reader.ReadByte(), //B
                        reader.ReadByte(), //G
                        reader.ReadByte(), //R
                        reader.ReadByte()  //A
                    };
                    ReadNullBytes(6);
                    break;
                case ParameterType.ComparisonOperator:
                case ParameterType.Surface:
                case ParameterType.ShakeIntensity:
                case ParameterType.Mood:
                case ParameterType.EvacuationSide:
                case ParameterType.RadarEvent:
                case ParameterType.Buildability:
                case ParameterType.Boundary:
                    parameter.Value = EnumerationHelper.GetEnumerationValue((short)reader.ReadInt32(), parameter.Type);
                    ReadNullBytes(6);
                    break;
                case ParameterType.KindOf:
                    parameter.Value = EnumerationHelper.GetEnumerationValue((short)reader.ReadInt32(), parameter.Type);
                    ReadNullBytes(4);
                    var kindOfAsString = ReadString();
                    Assert(kindOfAsString == parameter.Value.ToString());
                    break;
                default:
                    ReadNullBytes(8);
                    parameter.Value = ReadString();
                    break;
            }
            Write(parameter.Type.ToString(), parameter.Value.ToString());
            return parameter;
        }

        #endregion

        #region players

        private void ReadScriptsPlayers()
        {
            Write("ScriptsPlayers");
            bool propertiesIncluded = reader.ReadBoolean();
            ReadNullBytes(3);
            int length = reader.ReadInt32();
            indention++;
            for (int i = 0; i < length; i++)
            {
                var player = ReadPlayer(true, propertiesIncluded);
                Map.Players.Add(player);
            }

            indention--;
        }

        private void ReadSidesList()
        {
            Write("SidesList");
            
            indention++;
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var player = ReadPlayer(false, true);
                Map.Players.Add(player);
                ReadNullBytes(4);
            }

            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var team = ReadTeam();
                Map.Teams.Add(team);
            }
            indention--;

        }

        private Player ReadPlayer(bool readName, bool readProperties)
        {
            var player = new Player();
            if (readName)
                player.Name = ReadString();
            Write("Player", player.Name);
            if (readProperties)
                player.Properties = ReadProperties();
            return player;
        }

        #endregion

        #region objects

        private void ReadObjectsList(int length)
        {
            Write("ObjectsList");
            indention++;
            long start = reader.BaseStream.Position;
            while (reader.BaseStream.Position < start + length)
                Map.Objects.Add(ReadObject());
            indention--;
        }

        private ScriptObject ReadObject()
        {
            var obj = new ScriptObject();

            var header = ReadHeader();
            Assert(header.Magic == 3);
            Assert(Strings[header.Index] == "Object");

            obj.X = reader.ReadSingle();
            obj.Y = reader.ReadSingle();
            obj.Z = reader.ReadSingle();
            obj.Rotation = reader.ReadSingle();

            obj.RoadOptions = (RoadOptions)reader.ReadInt32();
            obj.Type = ReadString();
            
            Write("Object", obj.Type);

            obj.Properties = ReadProperties();
            return obj;
        }

        private IDictionary<String, Property> ReadProperties()
        {
            IDictionary<String, Property> properties = new Dictionary<String, Property>();
            short count = reader.ReadInt16();
            indention++;
            for (int i = 0; i < count; i++)
            {
                var property = ReadProperty();
                properties[property.Name] = property;
            }
            indention--;
            return properties;
        }

        private Property ReadProperty()
        {
            var property = new Property();

            property.Type = (PropertyType)reader.ReadByte();
            short index = reader.ReadInt16();
            property.Name = Strings[index];
            ReadNullBytes(1);
            switch (property.Type)
            {
                case PropertyType.Boolean:
                    property.Value = reader.ReadBoolean();
                    break;
                case PropertyType.Integer:
                    property.Value = reader.ReadInt32();
                    break;
                case PropertyType.Float:
                    property.Value = reader.ReadSingle();
                    break;
                case PropertyType.OneByteString:
                    property.Value = ReadOneByteString(true);
                    break;
                case PropertyType.TwoByteString:
                    property.Value = ReadTwoByteString();
                    break;
                default:
                    Debugger.Break();
                    break;
            }
            Write(property.Name, property.Value.ToString());
            return property;
        }

        #endregion

        #region areas

        private void ReadPolygoneTriggers()
        {
            Write("PolygoneTriggers");
            int count = reader.ReadInt32();
            indention++;
            for (int i = 0; i < count; i++)
                Map.Areas.Add(ReadArea());
            indention--;
        }

        private Area ReadArea()
        {            
            Area area = new Area();
            area.Name = ReadString();
            Write(area.Name);
            ReadNullBytes(2);
            area.Id = reader.ReadInt32();
            area.IsWater = reader.ReadBoolean();
            area.IsRiver = reader.ReadBoolean();
            area.RiverStart = reader.ReadInt32();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                area.Points.Add(new int[] { reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32() });

            return area;
        }

        #endregion
        
        #region teams

        private void ReadScriptTeams(int length)
        {
            Write("ScriptTeams");
            long start = reader.BaseStream.Position;
            indention++;
            while (reader.BaseStream.Position < start + length)
                Map.Teams.Add(ReadTeam());
            indention--;
        }

        private Team ReadTeam()
        {
            Write("Team");
            var team = new Team();
            team.Properties = ReadProperties();
            return team;
        }

        #endregion

        #region waypaths

        private void ReadWaypointsList()
        {
            Write("Waypaths");
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
                Map.Paths.Add(new Tuple<int, int>(reader.ReadInt32(), reader.ReadInt32()));
        }

        #endregion

        #region helper functions

        private String ReadOneByteString(bool twoByteLength)
        {
            short length = twoByteLength ? reader.ReadInt16() : reader.ReadByte();
            char[] chars = new char[length];
            for (short s = 0; s < length; s++)
                chars[s] = reader.ReadChar();
            return new String(chars);
        }

        private String ReadString()
        {
            return ReadOneByteString(true);
        }

        private String ReadTwoByteString()
        {
            short length = reader.ReadInt16();
            byte[] bytes = reader.ReadBytes(length * 2);
            return Encoding.Unicode.GetString(bytes);
        }

        private void ReadNullBytes(byte count)
        {
            for (byte b = 0; b < count; b++)
                Assert(reader.ReadByte() == 0);
        }

        private Header ReadHeader()
        {
            return new Header() { Index = reader.ReadInt32(), Magic = reader.ReadInt16(), Length = reader.ReadInt32() };
        }

        private void Write(String message)
        {
            //Debug.Write(String.Empty.PadLeft(indention * 2));
            //Debug.WriteLine(message);
        }

        private void Write(String message, String value)
        {
            //Debug.Write(String.Empty.PadLeft(indention * 2));
            //Debug.WriteLine("{0}: {1}", message, value);
        }

        #endregion
    }
}
