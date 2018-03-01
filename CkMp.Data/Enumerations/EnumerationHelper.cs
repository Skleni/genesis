#region License

// EnumerationHelper.cs
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
using CkMp.Data.Scripts;

namespace CkMp.Data.Enumerations
{
    public static class EnumerationHelper
    {
        private static IDictionary<ParameterType, Type> types;

        public static Type GetEnumerationType(ParameterType type)
        {
            if (types == null)
            {
                types = new Dictionary<ParameterType, Type>();
                types[ParameterType.ComparisonOperator] = typeof(ComparisonOperator);
                types[ParameterType.Surface] = typeof(Surface);
                types[ParameterType.ShakeIntensity] = typeof(ShakeIntensity);
                types[ParameterType.Mood] = typeof(Mood);
                types[ParameterType.EvacuationSide] = typeof(EvacuationSide);
                types[ParameterType.RadarEvent] = typeof(RadarEvent);
                types[ParameterType.Buildability] = typeof(Buildability);
                types[ParameterType.Boundary] = typeof(Boundary);
                types[ParameterType.KindOf] = typeof(Kind);
            }
            return types[type];
        }

        public static object GetEnumerationValue(short value, ParameterType type)
        {
            return Enum.ToObject(GetEnumerationType(type), value);
        }
    }
}
