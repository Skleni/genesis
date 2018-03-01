#region License

// Parameter.cs
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
    public class Parameter
    {
        public ParameterType Type { get; set; }
        public Object Value { get; set; }

        public int GetSize()
        {
            int valueSize;
            switch (Type)
            {
                case ParameterType.Boolean:
                case ParameterType.Integer:
                case ParameterType.ComparisonOperator:
                case ParameterType.Surface:
                case ParameterType.ShakeIntensity:
                case ParameterType.Mood:
                case ParameterType.EvacuationSide:
                case ParameterType.RadarEvent:
                case ParameterType.Buildability:
                case ParameterType.Boundary:
                case ParameterType.Float:
                case ParameterType.Angle:
                case ParameterType.Percent:
                case ParameterType.Color:
                    valueSize = 10;
                    break;
                case ParameterType.Location:
                    valueSize = 12;
                    break;
                default:
                    valueSize = 8 //null 
                              + 2 //length
                              + Value.ToString().Length;
                    break;
            }

            return 4 //type
                 + valueSize;
        }
    }
}
