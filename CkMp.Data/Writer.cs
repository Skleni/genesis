#region License

// Writer.cs
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
using System.Linq;
using System.Text;
using CkMp.Data.Lighting;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using CkMp.Data.Scripts;

namespace CkMp.Data
{
	public class Writer
	{
		private BinaryWriter writer;

		public IList<String> Strings { get; private set; }
		public Map.Map Map { get; set; }

		public Writer()
		{
			InitializeVariables();
		}

		private void InitializeVariables()
		{
			Strings = new List<String>() { null };
		}   

		public void WriteFile(string fileName)
		{
			string directory = Path.GetDirectoryName(fileName);
			if (!Directory.Exists(directory))
				Directory.CreateDirectory(directory);

			WriteStream(File.Open(fileName, FileMode.Create));
		}

		public void WriteStream(Stream stream)
		{
			checked
			{
                // write into a temporary memory stream (the header can only be written after the data)
                using (Stream dataStream = new MemoryStream())
                {
                    writer = new BinaryWriter(dataStream);

                    if (Map != null)
                    {
                        WriteHeightMap();
                        WriteBlendTileData();
                        WriteWorldInfo();
                        WriteSidesList();
                    }
                    else
                        WriteScriptsPlayers(true);

                    WriteObjectsList();

                    if (Map == null)
                        WriteScriptTeams();

                    WritePolygoneTriggers();
                    if (Map != null)
                        WriteGlobalLighting();
                    WriteWaypointsList();

                    // write the header into the real stream
                    writer = new BinaryWriter(stream);
                    WriteCkMp();
                    WriteStrings();

                    // copy the data from the temporary stream
                    dataStream.Seek(0, SeekOrigin.Begin);
                    dataStream.CopyTo(stream);
                    writer.Close();
                }  
			}
		}

		private void WriteCkMp()
		{
			writer.Write('C');
			writer.Write('k');
			writer.Write('M');
			writer.Write('p');
		}

		private void WriteStrings()
		{
			writer.Write(Strings.Count - 1);
			for (int i = Strings.Count - 1; i > 0; i--)
			{
				WriteOneByteString(Strings[i], false);
				writer.Write(i);
			}
		}

		#region Map

		private void WriteHeightMap() 
		{
			var header = new Header()
			{
				Index = GetIndex("HeightMapData"),
				Magic = 4,
				Length = Map.HeightMap.GetSize(false)
			};

			WriteHeader(header);

			writer.Write(Map.HeightMap.Width );
			writer.Write(Map.HeightMap.Height);
			writer.Write(Map.HeightMap.Border);

			int magic = 1;
			writer.Write(magic);

			writer.Write(Map.HeightMap.MapWidth);
			writer.Write(Map.HeightMap.MapHeight);

			writer.Write(Map.HeightMap.Count);

			for (int i = 0; i < Map.HeightMap.Count; i++)
				writer.Write(Map.HeightMap[i]);
		}

		private void WriteBlendTileData()
		{
			var header = new Header()
			{
				Index = GetIndex("BlendTileData"),
				Magic = 8,
				Length = Map.Tiles.GetSize(false)
			};

			WriteHeader(header);

			int count = Map.Tiles.Width * Map.Tiles.Height;
			writer.Write(count);

			for (int i = 0; i < count; i++)
				writer.Write(Map.Tiles[i].BaseTexture);

			for (int i = 0; i < count; i++)
				writer.Write(Map.Tiles[i].BlendTexture1);

			for (int i = 0; i < count; i++)
				writer.Write(Map.Tiles[i].BlendTexture2);

			for (int i = 0; i < count; i++)
				writer.Write(Map.Tiles[i].BlendTexture3);

			WriteImpassability();

			writer.Write(Map.Tiles.NumberOfBaseTiles);
			writer.Write(Map.Tiles.NumberOfBlendTiles);
			
			int magic = 1;
			writer.Write(magic);

			WriteTextures();
			WriteBlendTiles();
		}

		private void WriteImpassability()
		{
			// For every set of 8 horizontal tiles the impassability is encoded in a byte,
			// starting with a new byte in every new row.

			for (int i = 0; i < Map.Tiles.Height; i++)
			{
				byte impassability = 0;
				for (int j = 0; j < Map.Tiles.Width; j++)
				{
					impassability = (byte)((impassability >> 1) | (Map.Tiles[j, i].Impassable ? 128 : 0));

					if (j % 8 == 7)
					{
						writer.Write(impassability);
						impassability = 0;
					}
				}
				if (Map.Tiles.Width % 8 != 0)
					writer.Write(impassability);
			}
		}

		private void WriteTextures()
		{
			writer.Write(Map.Tiles.Textures.Count);
			
			foreach(var texture in Map.Tiles.Textures)
			{
				writer.Write(texture.BlockStartIndex);
				writer.Write(texture.BlockCount);
				writer.Write(texture.BlocksPerRow);
				Debug.Assert(texture.Magic == 0);
				writer.Write(texture.Magic);
				WriteOneByteString(texture.Name, true);
			}

			WriteNullBytes(8);
		}

		private void WriteBlendTiles()
		{            
			foreach (var blendTile in Map.Tiles.BlendTiles)
			{
				writer.Write(blendTile.TileIndex);
				writer.Write((long)blendTile.BlendType);
				writer.Write(BlendTile.BlendData);
			}
		}

		private void WriteWorldInfo() 
		{
			var header = new Header()
			{
				Index = GetIndex("WorldInfo"),
				Magic = 1,
				Length = Map.GetSize()
			};

			WriteHeader(header);
			WriteProperties(Map.Properties);
		}

		#endregion

        #region Lighting

        private void WriteGlobalLighting()
        {
            var header = new Header()
            {
                Index = GetIndex("GlobalLighting"),
                Magic = 3,
                Length = (Map.GlobalLightOptions == null ? 0 : Map.GlobalLightOptions.GetSize(false))
            };

            WriteHeader(header);
            if (Map.GlobalLightOptions != null)
            {
                writer.Write((byte)Map.GlobalLightOptions.TimeOfDay);
                WriteNullBytes(3);
                WriteLightOptions(Map.GlobalLightOptions.Morning);
                WriteLightOptions(Map.GlobalLightOptions.Afternoon);
                WriteLightOptions(Map.GlobalLightOptions.Evening);
                WriteLightOptions(Map.GlobalLightOptions.Night);
                writer.Write((byte)160);
                writer.Write((byte)160);
                writer.Write((byte)160);
                writer.Write((byte)127);
            }
        }

        private void WriteLightOptions(LightOptions lightOptions)
        {
            WriteColor(lightOptions.ObjectAmbientColor);
            WriteLight(lightOptions.ObjectsSun);
            WriteColor(lightOptions.TerrainAmbientColor);
            WriteLight(lightOptions.TerrainSun);
            WriteNullBytes(12);
            WriteLight(lightOptions.ObjectsAccent1);
            WriteNullBytes(12);
            WriteLight(lightOptions.ObjectsAccent2);
            WriteNullBytes(12);
            WriteLight(lightOptions.TerrainAccent1);
            WriteNullBytes(12);
            WriteLight(lightOptions.TerrainAccent2);
        }

        private void WriteLight(Light light)
        {
            WriteColor(light.Color);
            WriteVector(light.Vector);
        }

        private void WriteColor(Color color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
        }

        private void WriteVector(Vector vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        #endregion

        #region scripts

        private void WritePlayerScriptsList()
		{
			var header = new Header()
			{
				Index = GetIndex("PlayerScriptsList"),
				Magic = 5,
				Length = Map.PlayerScripts.Sum(p => 10 + p.Sum(s => s.GetSize(true)))
			};

			WriteHeader(header);

			foreach (var scriptList in Map.PlayerScripts)
				WriteScriptList(scriptList);
		}

		private void WriteScriptList(IList<ScriptBase> scriptList)
		{
			var header = new Header()
			{
				Index = GetIndex("ScriptList"),
				Magic = 1,
				Length = scriptList.Sum(s => s.GetSize(true))
			};
			
			WriteHeader(header);

			foreach (var scriptElement in scriptList)
			{
				if (scriptElement is ScriptGroup)
					WriteScriptGroup((ScriptGroup)scriptElement);
				else
					WriteScript((Script)scriptElement);

			}
		}

		private void WriteScriptGroup(ScriptGroup group)
		{
			var header = new Header()
			{
				Index = GetIndex("ScriptGroup"),
				Magic = 2,
				Length = group.GetSize(false)
			};

			WriteHeader(header);

			WriteString(group.Name);
			writer.Write(group.IsActive);
			writer.Write(group.IsSubroutine);
			
			foreach (var script in group.Scripts)
				WriteScript(script);
		}

		private void WriteScript(Script script)
		{
			var header = new Header()
			{
				Index = GetIndex("Script"),
				Magic = 2,
				Length = script.GetSize(false)
			};
			
			WriteHeader(header);

			WriteString(script.Name);
			WriteString(script.Comment);
			WriteString(script.ConditionComment);
			WriteString(script.ActionComment);
			writer.Write(script.IsActive);
			writer.Write(script.Deactivate);
			writer.Write(script.Easy);
			writer.Write(script.Normal);
			writer.Write(script.Hard);
			writer.Write(script.IsSubroutine);
			writer.Write(script.Interval);

			foreach (var orCondition in script.OrConditions)
				WriteOrCondition(orCondition);

			foreach (var action in script.ActionsIfTrue)
				WriteScriptElement(action, GetIndex("ScriptAction"), 2);

			foreach (var action in script.ActionsIfFalse)
				WriteScriptElement(action, GetIndex("ScriptActionFalse"), 2);	
		}

		private void WriteOrCondition(OrCondition orCondition)
		{
			var header = new Header()
			{
				Index = GetIndex("OrCondition"),
				Magic = 1,
				Length = orCondition.GetSize(false)
			};

			WriteHeader(header);

			foreach (var condition in orCondition.Conditions)
				WriteScriptElement(condition, GetIndex("Condition"), 4);
		}

		private void WriteScriptElement(ScriptElement scriptElement, int index, short magic)
		{
			var header = new Header()
			{
				Index = index,
				Magic = magic,
				Length = scriptElement.GetSize(false)
			};

			WriteHeader(header);
			writer.Write(scriptElement.Id);
			writer.Write((byte)3);
			
			writer.Write((short)GetIndex(scriptElement.Name));
			WriteNullBytes(1);
			
			writer.Write(scriptElement.Parameters.Count);
			foreach (var parameter in scriptElement.Parameters)
				WriteParameter(parameter);
		}

		private void WriteParameter(Parameter parameter)
		{
			writer.Write((int)parameter.Type);

			switch (parameter.Type)
			{
				case ParameterType.Boolean:
					writer.Write((bool)parameter.Value);
					WriteNullBytes(9);
					break;
				case ParameterType.Integer:
					writer.Write((int)parameter.Value);
					WriteNullBytes(6);
					break;
				case ParameterType.ComparisonOperator:
				case ParameterType.Surface:
				case ParameterType.ShakeIntensity:
				case ParameterType.Mood:
				case ParameterType.EvacuationSide:
				case ParameterType.RadarEvent:
				case ParameterType.Buildability:
				case ParameterType.Boundary:
					writer.Write((int)(short)parameter.Value);
					WriteNullBytes(6);
					break;
				case ParameterType.Float:
				case ParameterType.Angle:
				case ParameterType.Percent:
					WriteNullBytes(4);
					writer.Write((float)parameter.Value);
					WriteNullBytes(2);
					break;
				case ParameterType.Location:
					float[] floats = (float[])parameter.Value;
					writer.Write(floats[0]);
					writer.Write(floats[1]);
					writer.Write(floats[2]);
					break;
				case ParameterType.Color:
					byte[] bytes = (byte[])parameter.Value;
					writer.Write(bytes[0]);
					writer.Write(bytes[1]);
					writer.Write(bytes[2]);
					writer.Write(bytes[3]);
					WriteNullBytes(6);
					break;
				case ParameterType.KindOf:
					writer.Write((int)(short)parameter.Value);
					WriteNullBytes(4);
					WriteString(parameter.Value.ToString());
					break;
				default:
					WriteNullBytes(8);
					WriteString((string)parameter.Value);
					break;
			}
		}

		#endregion

		#region players

		private void WriteScriptsPlayers(bool propertiesIncluded)
		{
			var header = new Header()
			{
				Index = GetIndex("ScriptsPlayers"),
				Magic = 2,
				Length = 1
					   + 3
					   + 4
					   + Map.Players.Sum(p => p.GetSize(true, propertiesIncluded))
			};

			WriteHeader(header);

			writer.Write(propertiesIncluded);
			WriteNullBytes(3);

			writer.Write(Map.Players.Count);
			foreach (var player in Map.Players)
				WritePlayer(player, true, propertiesIncluded);
		}

		private void WriteSidesList()
		{
			var header = new Header() 
			{
				Index = GetIndex("SidesList"),
				Magic = 3, //TODO
				Length = 4
					   + Map.Players.Sum(p => p.GetSize(false, true) + 4)
					   + 4
                       + Map.Teams.Sum(t => t.GetSize())
					   + 10
                       + Map.PlayerScripts.Sum(p => 10 + p.Sum(s => s.GetSize(true)))
			};

			WriteHeader(header);

			writer.Write(Map.Players.Count);
			foreach (var player in Map.Players)
			{
				WritePlayer(player, false, true);
				WriteNullBytes(4);
			}

            writer.Write(Map.Teams.Count);
            foreach (var team in Map.Teams)
				WriteTeam(team);

			WritePlayerScriptsList();
		}

		private void WritePlayer(Player player, bool writeName, bool writeProperties)
		{
			if (writeName)
				WriteString(player.Name);

			if (writeProperties)
				WriteProperties(player.Properties);
		}

		#endregion

		#region objects

		private void WriteObjectsList()
		{
			var header = new Header()
			{
				Index = GetIndex("ObjectsList"),
				Magic = 3,
				Length = Map.Objects.Sum(o => o.GetSize(true))
			};

			WriteHeader(header);

			foreach (var scriptObject in Map.Objects)
				WriteObject(scriptObject);
		}

		private void WriteObject(ScriptObject scriptObject)
		{
			var header = new Header()
			{
				Index = GetIndex("Object"),
				Magic = 3,
				Length = scriptObject.GetSize(false)
			};

			WriteHeader(header);
			
			writer.Write(scriptObject.X);
            writer.Write(scriptObject.Y);
			writer.Write(scriptObject.Z);
			writer.Write(scriptObject.Rotation);

			writer.Write((int)scriptObject.RoadOptions);
			WriteString(scriptObject.Type);
			
			WriteProperties(scriptObject.Properties);
		}

		private void WriteProperties(IDictionary<String, Property> properties)
		{
			writer.Write((short)properties.Count);
		 
			foreach (var property in properties)
				WriteProperty(property.Value);
		}

		private void WriteProperty(Property property)
		{
			writer.Write((byte)property.Type);
			writer.Write((short)GetIndex(property.Name));
			WriteNullBytes(1);
			
			switch (property.Type)
			{
				case PropertyType.Boolean:
					writer.Write((bool)property.Value);
					break;
				case PropertyType.Integer:
					writer.Write((int)property.Value);
					break;
				case PropertyType.Float:
					writer.Write((float)property.Value);
					break;
				case PropertyType.OneByteString:
					WriteOneByteString((String)property.Value, true);
					break;
				case PropertyType.TwoByteString:
					WriteTwoByteString((String)property.Value);
					break;
				default:
					Debugger.Break();
					break;
			}
		}

		#endregion

		#region areas

		private void WritePolygoneTriggers()
		{
			var header = new Header()
			{
				Index = GetIndex("PolygonTriggers"),
				Magic = (short)(Map == null ? 3 : 4),
                Length = 4 + Map.Areas.Sum(a => a.GetSize())
			};

			WriteHeader(header);

            writer.Write(Map.Areas.Count);
            foreach (var area in Map.Areas)
				WriteArea(area);
		}

		private void WriteArea(Area area)
		{
			WriteString(area.Name);
			WriteNullBytes(2);
			writer.Write(area.Id);
			writer.Write(area.IsWater);
			writer.Write(area.IsRiver);
			writer.Write(area.RiverStart);

			writer.Write(area.Points.Count);
			foreach (var point in area.Points)
			{
				writer.Write(point[0]);
				writer.Write(point[1]);
				writer.Write(point[2]);
			}
		}

		#endregion

		#region teams

		private void WriteScriptTeams()
		{
			var header = new Header()
			{
				Index = GetIndex("ScriptTeams"),
				Magic = 1,
                Length = Map.Teams.Sum(d => d.GetSize())
			};

			WriteHeader(header);

            foreach (var team in Map.Teams)
				WriteTeam(team);
		}

		private void WriteTeam(Team team)
		{
			WriteProperties(team.Properties);
		}

		#endregion

		#region waypaths

		private void WriteWaypointsList()
		{
			var header = new Header()
			{
				Index = GetIndex("WaypointsList"),
				Magic = 1,
				Length = 4 + Map.Paths.Count * (4 + 4)
			};

			WriteHeader(header);

			writer.Write(Map.Paths.Count);
			foreach (var path in Map.Paths)
			{
				writer.Write(path.Item1);
				writer.Write(path.Item2);
			}
		}

		#endregion

		#region helper functions

		private int GetIndex(String value)
		{
			int index = Strings.IndexOf(value);
			if (index < 0)
			{
				Strings.Add(value);
				index = Strings.Count - 1;
			}

			return index;
		}

		private void WriteOneByteString(String value, bool twoByteLength)
		{
			if (twoByteLength)
				writer.Write((short)value.Length);
			else
				writer.Write((byte)value.Length);

			foreach (char c in value)
				writer.Write((byte)c);
		}

		private void WriteString(String value)
		{
			WriteOneByteString(value, true);
		}

		private void WriteTwoByteString(String value)
		{
			writer.Write((short)value.Length);
			byte[] bytes = Encoding.Unicode.GetBytes(value);
			writer.Write(bytes);
		}

		private void WriteNullBytes(byte count)
		{
			for (byte b = 0; b < count; b++)
				writer.Write((byte)0);
		}

		private void WriteHeader(Header header)
		{
			writer.Write(header.Index);
			writer.Write(header.Magic);
			writer.Write(header.Length);
		}

		#endregion
	}
}
