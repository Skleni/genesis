#region License

// RoadGraphNode.cs
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
using System.Diagnostics;

namespace Genesis.Roads
{
    [DebuggerDisplay("({Position.X}, {Position.Y}), CanBeConnected: {CanBeConnected}, CreatedByTemplate: {CreatedByTemplate}")]
    public class RoadGraphNode : IEquatable<RoadGraphNode>
    {
        public Point Position { get; set; }
        public bool CanBeConnected { get; set; }
        public bool CreatedByTemplate { get; set; }
        public LinkedList<RoadGraphNode> Neighbors { get; private set; }

        public RoadGraphNode(Point position, bool canBeConnected, bool createdByTemplate)
        {
            this.Position = position;
            this.CanBeConnected = canBeConnected;
            this.Neighbors = new LinkedList<RoadGraphNode>();
            this.CreatedByTemplate = createdByTemplate;
        }

        public void LinkTo(RoadGraphNode node)
        {
            this.Neighbors.AddLast(node);
            node.Neighbors.AddLast(this);
        }

        public bool Equals(RoadGraphNode other)
        {
            return object.ReferenceEquals(this, other);
        }
    }
}
