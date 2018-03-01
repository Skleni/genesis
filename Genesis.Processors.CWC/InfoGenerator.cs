#region License

// InfoGenerator.cs
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
using System.Reflection;
using CkMp.Data.Map;
using CkMp.Data.Scripts;
using Genesis.Core;

namespace Genesis.Processors.CWC
{
    public class InfoGenerator : IMapProcessor
    {
        private const string format = @"This map was generated on {0} by the CWC Map Generator {1}.

Map Id: {2}
Layout: {3}
Scenery: {4}
Mountains: {5}
Trees: {6}";

        public string Description
        {
            get
            {
                return "Adding map generation info";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            var infoScript = new Script()
            {
                Comment = string.Format(
                    format,
                    DateTime.Now.ToString(),
                    version,
                    info.MapId,
                    info.Settings.Layout.Name,
                    info.Settings.Scenery.Name,
                    info.Settings.NumberOfCliffs,
                    info.Settings.NumberOfTrees),
                IsActive = false,
                IsSubroutine = false,
                Deactivate = true,
                Easy = true,
                Normal = true,
                Hard = true,
                Interval = 0,
                Name = "Map Generation Info"
            };

            IList<ScriptBase> neutralScripts = map.PlayerScripts.FirstOrDefault();
            if (neutralScripts == null)
            {
                neutralScripts = new List<ScriptBase>();
                map.PlayerScripts.Add(neutralScripts);

                neutralScripts.Add(infoScript);
            }
        }
    }
}
