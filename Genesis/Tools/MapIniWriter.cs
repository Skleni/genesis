#region License

// MapIniWriter.cs
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
using System.IO;
using System.Linq;
using System.Text;

namespace Genesis.Tools
{
    public class MapIniWriter
    {
        public void Write(string path, IEnumerable<string> mapIniTemplates)
        {
            string contents = string.Join(Environment.NewLine + Environment.NewLine, mapIniTemplates.Select(t => File.ReadAllText(t)));
            File.WriteAllText(path, contents, Encoding.Default);
        }
    }
}
