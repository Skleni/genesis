#region License

// Property.cs
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
    public class Property
    {
        public String Name { get; set; }
        public PropertyType Type { get; set; }
        public Object Value { get; set; }

        public override string ToString()
        {
            return String.Format("{0} ({1}): {2}", Name, Type, Value);
        }

        public int GetSize()
        {
            int valueSize = 0;
            switch(Type)
            {
                case PropertyType.Boolean:
                    valueSize = 1;
                    break;
                case PropertyType.Integer:
                case PropertyType.Float:
                    valueSize = 4;
                    break;
                case PropertyType.OneByteString:
                    valueSize = 2 + ((String)Value).Length;
                    break;
                case PropertyType.TwoByteString:
                    valueSize = 2 + 2 * ((String)Value).Length;
                    break;
            }

            return 1 //type
                 + 2 //name index
                 + 1 //null
                 + valueSize;
        }
    }
}
