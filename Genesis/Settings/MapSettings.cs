#region License

// MapSettings.cs
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

using CkMp.Data.Enumerations;

namespace Genesis.Settings
{
    public class MapSettings
    {
        public MapLayout Layout { get; set; }
        public Scenery Scenery { get; set; }
        public Frequency NumberOfCliffs { get; set; }
        public Frequency NumberOfTrees { get; set; }
        public TimeOfDay TimeOfDay { get; set; }

        public MapSettings(MapLayout layout, Scenery scenery, Frequency numberOfCliffs, Frequency numberOfTrees, TimeOfDay timeOfDay)
        {
            Layout = layout;
            Scenery = scenery;
            NumberOfCliffs = numberOfCliffs;
            NumberOfTrees = numberOfTrees;
            TimeOfDay = timeOfDay;
        }
    }
}
