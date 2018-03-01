#region License

// DebugProcessor.cs
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
using CkMp.Data.Map;
using Genesis.Core;

namespace Genesis.Processors
{
    public class DebugProcessor : IMapProcessor
    {
        public Func<Map, MapInfo, int, int, bool> Condition { get; set; }

        public DebugProcessor(Func<Map, MapInfo, int, int, bool> condition)
        {
            Condition = condition;
        }

        public string Description
        {
            get
            {
                return null;
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            if (Condition != null)
            {
                for (int i = 0; i < map.Tiles.Width; i++)
                {
                    for (int j = 0; j < map.Tiles.Height; j++)
                    {
                        map.Tiles[i, j].Impassable = Condition(map, info, i, j);
                    }
                }
            }
        }
    }
}
