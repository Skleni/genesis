﻿#region License

// Tile.cs
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

namespace CkMp.Data.Map
{
    public class Tile
    {
        public short BaseTexture { get; set; }
        public short BlendTexture1 { get; set; }
        public short BlendTexture2 { get; set; }
        public short BlendTexture3 { get; set; }
        public bool Impassable { get; set; }
    }
}
