#region License

// Player.cs
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

namespace CkMp.Data.Objects
{
    public class Player : PropertyObject
    {
        public String Name { get; set; }

        public Player()
        {
        }

        public Player(string name, bool isHuman, string displayName, string faction, string allies, string enemies)
        {
            SetProperty("playerName", PropertyType.OneByteString, name);
            SetProperty("playerIsHuman", PropertyType.Boolean, isHuman);
            SetProperty("playerDisplayName", PropertyType.TwoByteString, displayName);
            SetProperty("playerFaction", PropertyType.OneByteString, faction);
            SetProperty("playerAllies", PropertyType.OneByteString, allies);
            SetProperty("playerEnemies", PropertyType.OneByteString, enemies);
        }

        public Player(string name, bool isHuman, string displayName, string faction, string allies, string enemies, byte a, byte r, byte g, byte b) :
            this(name, isHuman, displayName, faction, allies, enemies)
        {
            SetProperty("playerColor", PropertyType.Integer, a << 24 | r << 16 | g << 8 | b);
        }

        public int GetSize(bool includingName, bool includingProperties)
        {
            int size = 0;
            if (includingName)
                size += 2 + Name.Length;
            if (includingProperties)
                size += GetSize();

            return size;
        }
    }
}
