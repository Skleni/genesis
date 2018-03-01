#region License

// IMapProcessor.cs
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
using System.Threading;
using CkMp.Data.Map;

namespace Genesis.Core
{
    public interface IMapProcessor
    {
        /// <summary>
        /// Gets a string indicating what this processor does.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a value indicating whether the preview should be updated after executing this processor.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the preview should be updated, <c>false</c> otherwise.</returns>
        /// </value>
        bool UpdatePreview { get; }

        /// <summary>
        /// Applies this processor on a map.
        /// </summary>
        /// <param name="map">The map.</param>
        /// <param name="info">The map info.</param>
        void Process(Map map, MapInfo info);
    }
}
