#region License

// Map.cs
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
using CkMp.Data.Lighting;
using CkMp.Data.Objects;
using CkMp.Data.Scripts;

namespace CkMp.Data.Map
{
    public class Map : PropertyObject
    {
        public const float TileWidth = 10f;

        public GlobalLightOptions GlobalLightOptions { get; set; }

        public HeightMap HeightMap { get; set; }
        public TileData Tiles { get; set; }
        public IList<ScriptObject> Objects { get; private set; }
        public IList<Player> Players { get; private set; }
        public IList<Tuple<int, int>> Paths { get; private set; }
        public IList<Team> Teams { get; private set; }
        public IList<Area> Areas { get; private set; }
        public IList<IList<ScriptBase>> PlayerScripts { get; private set; }
        
        public Map()
        {
            Objects = new List<ScriptObject>();
            Players = new List<Player>();
            Paths = new List<Tuple<int, int>>();
            Teams = new List<Team>();
            Areas = new List<Area>();
            PlayerScripts = new List<IList<ScriptBase>>();
        }

        public Map(int width, int height, int border) : this()
        {
            HeightMap = new HeightMap(width, height, border);
            Tiles = new TileData(width, height);    
        }
    }
}
