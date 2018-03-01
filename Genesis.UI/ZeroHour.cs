#region License

// ZeroHour.cs
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
using System.IO;
using Microsoft.Win32;

namespace Genesis.UI
{
    public static class ZeroHour
    {
        private static string mapsDirectory;

        public static string MapsDirectory
        {
            get
            {
                if (mapsDirectory == null)
                {
                    string data = (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Electronic Arts\EA Games\Command and Conquer Generals Zero Hour", "UserDataLeafName", null);
                    if (data != null)
                        mapsDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), data, "Maps" + Path.DirectorySeparatorChar);
                }

                return mapsDirectory;
            }
        }
    }
}
