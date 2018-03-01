#region License

// ScriptImporter.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using CkMp.Data;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using CkMp.Data.Scripts;
using Genesis.Core;

namespace Genesis.Processors
{
    public class ScriptImporter : IMapProcessor
    {
        public string ScriptFile { get; set; }
        public string PlayerName { get; set; }

        public string Description
        {
            get
            {
                return "Importing script file: " + Path.GetFileName(ScriptFile);
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            var reader = new Reader();
            reader.ReadFile(ScriptFile);

            for (int i = 0; i < reader.Map.Players.Count; i++)
			{
                Player player = null;
                string name = GetName(reader.Map.Players[i]);

                if (string.IsNullOrEmpty(name) || name == "**SELECTION**")
                {
                    if (string.IsNullOrEmpty(PlayerName))
                    {
                        player = map.Players.LastOrDefault(IsCivilianPlayer);
                        if (player == null)
                            throw new InvalidOperationException("No player specified and no civilian player found.");
                    }
                    else
                    {
                        name = PlayerName;
                    }
                }
                
                if (player == null)
                {
                    player = map.Players.FirstOrDefault(p => GetName(p) == name);
                    if (player == null)
                    {
                        player = reader.Map.Players[i];
                        map.Players.Add(player);
                    }
                }

                int index = map.Players.IndexOf(player);

                while (map.PlayerScripts.Count <= index)
                    map.PlayerScripts.Add(new List<ScriptBase>());

                foreach (var script in reader.Map.PlayerScripts[i])
                    map.PlayerScripts[index].Add(script);

                foreach (var team in reader.Map.Teams)
                    map.Teams.Add(team);
			}
        }

        private string GetName(Player player)
        {
            Property name;
            if (player.Properties.TryGetValue("playerName", out name))
                return name.Value as string;
            else
                return player.Name;
        }

        private bool IsCivilianPlayer(Player player)
        {
            Property faction;
            player.Properties.TryGetValue("playerFaction", out faction);
            return faction.Value.Equals("FactionCivilian");
        }
    }
}
