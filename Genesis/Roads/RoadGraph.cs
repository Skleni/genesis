#region License

// RoadGraph.cs
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
using System.Linq;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Objects;

namespace Genesis.Roads
{
    public class RoadGraph
    {
        public LinkedList<RoadGraphNode> Nodes { get; private set; }
        public List<Tuple<ScriptObject, ScriptObject>> Segments { get; private set; }

        public RoadGraph()
        {
            this.Nodes = new LinkedList<RoadGraphNode>();
            this.Segments = new List<Tuple<ScriptObject, ScriptObject>>();
        }

        /// <summary>
        /// Connects the nodes at the first and last position, using the route given by the (optional) positions in between.
        /// If there is already a node at a given position, it is reused, otherwise new nodes are created.
        /// </summary>
        /// <param name="positions">The positions.</param>
        public void ConnectNodes(IEnumerable<Point> positions)
        {
            RoadGraphNode lastNode = null;
            foreach (var point in positions)
            {
                var node = FindNode(point);
                if (node == null)
                {
                    node = new RoadGraphNode(point, true, false);
                    Nodes.AddLast(node);
                }
                if (lastNode != null)
                    node.LinkTo(lastNode);
                lastNode = node;
            }
        }

        /// <summary>
        /// Adds a road using the route given by the positions to the map.
        /// </summary>
        /// <param name="positions">The positions.</param>
        /// <param name="map">The map.</param>
        /// <param name="factory">The object factory.</param>
        /// <param name="roadType">The type of the road.</param>
        /// <param name="connectNodes"><c>True</c> if the ConnectNodes function should be called.</param>
        public void AddRoad(IEnumerable<Point> positions, Map map, ObjectFactory factory, string roadType, bool connectNodes = true)
        {
            if (connectNodes)
                ConnectNodes(positions);

            Point start;
            Point end = positions.First();
            foreach (var point in positions.Skip(1))
            {
                start = end;
                end = point;

                var road1 = factory.CreateObject(roadType, start.X, start.Y, 0, 0);
                road1.RoadOptions = RoadOptions.Start;
                map.Objects.Add(road1);

                var road2 = factory.CreateObject(roadType, end.X, end.Y, 0, 0);
                road2.RoadOptions = RoadOptions.End;
                map.Objects.Add(road2);

                Segments.Add(new Tuple<ScriptObject, ScriptObject>(road1, road2));
            }
        }

        /// <summary>
        /// Gets a node at a given position.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <returns></returns>
        public RoadGraphNode FindNode(Point position)
        {
            return Nodes.FirstOrDefault(n => n.Position.X == position.X && n.Position.Y == position.Y);
        }

        /// <summary>
        /// Gets a road segment identified by the start and end points. The order of the points is not important (i.e., the method doesn't check if the RoadOption flags).
        /// </summary>
        /// <param name="start">The start point.</param>
        /// <param name="end">The end point.</param>
        public Tuple<ScriptObject, ScriptObject> FindRoadSegment(Point start, Point end)
        {
            for (int i = 0; i < Segments.Count; i++)
            {
                if ((Segments[i].Item1.X == start.X && Segments[i].Item1.Y == start.Y && Segments[i].Item2.X == end.X && Segments[i].Item2.Y == end.Y) ||
                    (Segments[i].Item2.X == start.X && Segments[i].Item2.Y == start.Y && Segments[i].Item1.X == end.X && Segments[i].Item1.Y == end.Y))
                {
                    return Segments[i];
                }
            }

            return null;
        }
    }
}
