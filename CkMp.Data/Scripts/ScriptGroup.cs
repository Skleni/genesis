#region License

// ScriptGroup.cs
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

using System.Collections.Generic;
using System.Linq;

namespace CkMp.Data.Scripts
{
    public class ScriptGroup : ScriptBase
    {
        private IList<Script> scripts;
        public override IList<Script> Scripts { get { return scripts; } }

        public ScriptGroup()
        {
            scripts = new List<Script>();
        }

        public override int GetSize(bool includingHeader)
        {
            return base.GetSize(includingHeader)
                 + Scripts.Sum(s => s.GetSize(true));
        }
    }
}
