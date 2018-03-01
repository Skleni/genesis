#region License

// RoadGraphEdge.cs
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

namespace Genesis.Roads
{
    public class RoadGraphEdge
    {
        public RoadGraphNode Node1 { get; private set; }
        public RoadGraphNode Node2 { get; private set; }
        public float Length { get; private set; }

        public RoadGraphEdge(RoadGraphNode node1, RoadGraphNode node2, float length)
        {
            Node1 = node1;
            Node2 = node2;
            Length = length;
        }
    }
}
