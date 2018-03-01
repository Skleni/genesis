#region License

// ParameterType.cs
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

namespace CkMp.Data.Scripts
{
    [Serializable]
    public enum ParameterType : int
    {
        Integer = 0,
        Float = 1,
        Script = 2,
        Team = 3,
        Counter = 4,
        Flag = 5,
        ComparisonOperator = 6,
        Waypoint = 7,
        Boolean = 8,
        Area = 9,
        String = 10,
        Player = 11,
        Sound = 12,
        Subroutine = 13,
        Unit = 14,
        UnitType = 15,
        Location = 16,
        Angle = 17,
        State = 18,
        Relation = 19,
        Mood = 20,
        Speech = 21,
        Music = 22,
        Video = 23,
        Path = 24,
        LocalizedString = 25,
        Bridge = 26,
        KindOf = 27,
        AttackPrioritySet = 28,
        RadarEvent = 29,
        SpecialPower = 30,
        Science = 31,
        Upgrade = 32,
        
        Boundary = 34,
        Buildability = 35,
        Surface = 36,
        ShakeIntensity = 37,
        CommandButton = 38,
        Font = 39,
        ObjectStatus = 40,
        Ability = 41,
        SkirmishApproachPath = 42,
        Color = 43,
        Emoticon = 44,
        ObjectFlag = 45,
        Faction = 46,

        RevealName = 48,
        ScienceAvailability = 49,
        EvacuationSide = 50,
        Percent = 51
    }
}
