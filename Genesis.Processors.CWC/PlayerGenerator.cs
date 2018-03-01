#region License

// PlayerGenerator.cs
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

using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Core;

namespace Genesis.Processors.CWC
{
    public class PlayerGenerator : IMapProcessor
    {
        public string Description
        {
            get
            {
                return "Generating players";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            map.Players.Add(new Player(string.Empty, false, "Neutral", string.Empty, string.Empty, string.Empty));
            map.Players.Add(new Player("Resources", false, "Resources", "FactionCivilian", string.Empty, string.Empty, 0x00, 0x59, 0x59, 0x59));
            map.Players.Add(new Player("PlyrCivilian", false, "Civilian", "FactionCivilian", string.Empty, string.Empty, 0x00, 0xFF, 0xFF, 0xFF));
        }
    }
}
