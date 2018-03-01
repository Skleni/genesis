#region License

// Script.cs
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
    public class Script : ScriptBase
    {
        private static IList<Script> emptyList = new List<Script>();

        public Script()
        {
            Comment = string.Empty;
            ConditionComment = string.Empty;
            ActionComment = string.Empty;

            OrConditions = new List<OrCondition>();
            ActionsIfTrue = new List<ScriptElement>();
            ActionsIfFalse = new List<ScriptElement>();
        }

        public string Comment { get; set; }
        public string ConditionComment { get; set; }
        public string ActionComment { get; set; }
        public bool Deactivate { get; set; }
        public bool Easy { get; set; }
        public bool Normal { get; set; }
        public bool Hard { get; set; }
        public int Interval { get; set; }

        public IList<OrCondition> OrConditions { get; private set; }
        public IList<ScriptElement> ActionsIfTrue { get; private set; }
        public IList<ScriptElement> ActionsIfFalse { get; private set; }

        public override IList<Script> Scripts { get { return emptyList; } }

        public override int GetSize(bool includingHeader)
        {
            return base.GetSize(includingHeader)
                 + 2
                 + Comment.Length
                 + 2
                 + ConditionComment.Length
                 + 2
                 + ActionComment.Length
                 + 4 * 1
                 + 4
                 + OrConditions.Sum(o => o.GetSize(true))
                 + ActionsIfTrue.Sum(a => a.GetSize(true))
                 + ActionsIfFalse.Sum(a => a.GetSize(true));
        }
    }
}
