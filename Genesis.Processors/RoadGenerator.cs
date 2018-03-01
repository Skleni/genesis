#region License

// RoadGenerator.cs
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
using Genesis.Algorithms;
using Genesis.Core;
using Genesis.Math;
using Genesis.Roads;
using Genesis.Tiles;

namespace Genesis.Processors
{
    public class RoadGenerator : IMapProcessor
    {
        private const float outOfMapDestinationDistance = 3; // in tiles
        private const float tightCurveAngle = (float)System.Math.PI / 3;
        private const float tightCurveDistance = 55f;
        private const float minAngleBetweenCrossroads = 35f;

        private Map map;
        private MapInfo info;
        private Random random;
        private TileGraph tileGraph;
        private List<RoadGraphEdge> roadsLeadingOutOfMap;

        public int MinimumRoadsLeadingOutOfMap { get; set; }
        public float MinimumDistanceBetweenRoadsLeadingOutOfMap { get; set; }

        public RoadGenerator()
        {
            MinimumRoadsLeadingOutOfMap = 4;
            MinimumDistanceBetweenRoadsLeadingOutOfMap = 500f;
        }

        public string Description
        {
            get
            {
                return "Generating roads";
            }
        }

        public bool UpdatePreview
        {
            get { return false; }
        }

        public void Process(Map map, MapInfo info)
        {
            this.map = map;
            this.info = info;
            this.tileGraph = new TileGraph(info.Tiles, TileInfo.Structure | TileInfo.Cliff);
            this.roadsLeadingOutOfMap = new List<RoadGraphEdge>();

            this.random = new Random(info.MapId);

            AddRoadsBetweenNodes();
            AddRoadsLeadingOutOfMapForDanglingNodes();
            AddAdditionalRoadsLeadingOutOfMap();

            CheckTightCurves();

            AddOffsets();
        }

        private void CheckTightCurves()
        {
            foreach (var node in info.Roads.Nodes.Where(n => !n.CreatedByTemplate || n.CanBeConnected))
            {
                if (node.Neighbors.Count == 2)
                {
                    var neighbor1 = node.Neighbors.ElementAt(0);
                    var neighbor2 = node.Neighbors.ElementAt(1);

                    var vector1 = neighbor1.Position - node.Position;
                    var vector2 = neighbor2.Position - node.Position;
                    
                    var angle = Angle.Between(vector1, vector2);
                    if (angle > System.Math.PI / 2)
                        angle = (float)System.Math.PI - angle;
                    
                    if (angle > System.Math.PI / 5)
                    {
                        if (vector1.VectorLengthSquared() < tightCurveDistance * tightCurveDistance || vector2.VectorLengthSquared() < tightCurveDistance * tightCurveDistance)
                        {
                            var segment = info.Roads.FindRoadSegment(node.Position, neighbor1.Position);
                            segment.Item1.RoadOptions |= RoadOptions.TightCurve;
                            segment.Item2.RoadOptions |= RoadOptions.TightCurve;

                            segment = info.Roads.FindRoadSegment(node.Position, neighbor2.Position);
                            segment.Item1.RoadOptions |= RoadOptions.TightCurve;
                            segment.Item2.RoadOptions |= RoadOptions.TightCurve;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds small random offsets to roads with two neighbors (multiple exactly aligned roads sometimes lead to strange visual artifacts).
        /// </summary>
        private void AddOffsets()
        {
            var nodes = from n in info.Roads.Nodes
                        where //n.CanBeConnected &&
                              !n.CreatedByTemplate &&
                              n.Neighbors.Count == 2
                        select n;

            foreach (var node in nodes)
            {
                var roads = (from r in map.Objects
                            where r.RoadOptions != RoadOptions.None &&
                                  r.X == node.Position.X &&
                                  r.Y == node.Position.Y
                            select r).ToArray();
                
                node.Position = new Point(
                    node.Position.X + (float)random.NextDouble() * 2,
                    node.Position.Y + (float)random.NextDouble() * 2);

                foreach (var road in roads)
                {
                    road.X = node.Position.X;
                    road.Y = node.Position.Y;
                }
            }
        }

        private void AddRoadsBetweenNodes()
        {
            var aStar = new AStar<RoadGraphNode>(
                            n => n.Neighbors,
                            CalculateSquaredDistance,
                            CalculateSquaredDistance);

            bool changed = true;
            while (changed)
            {
                if (info.IsCancellationRequested)
                    return;

                changed = false;

                var edges = from n1 in info.Roads.Nodes
                            where n1.CanBeConnected
                            from n2 in info.Roads.Nodes.Except(n1.Neighbors)
                            where n2.CanBeConnected && n1 != n2 
                            let length = (n1.Position - n2.Position).VectorLength()
                            orderby length
                            select new RoadGraphEdge(n1, n2, length);

                foreach (var edge in edges)
                {
                    double length;
                    if (aStar.FindShortestPath(edge.Node1, edge.Node2, out length) == null || (length > edge.Length * 3f && edge.Length > 100))
                    {
                        if (TryAddEdge(edge))
                        {
                            changed = true;
                            break;
                        }
                    }
                }
            }
        }

        private void AddRoadsLeadingOutOfMapForDanglingNodes()
        {
            var danglingNodes = (from n in info.Roads.Nodes
                                 where n.CanBeConnected && n.Neighbors.Count == 1
                                 select n).ToArray();

            foreach (var node in danglingNodes)
            {
                if (info.IsCancellationRequested)
                    return;

                var direction = node.Position - node.Neighbors.First().Position;

                var maxAngle = System.Math.PI / 4;
                var possibleNewEdges = from n in info.Roads.Nodes
                                       where n.CanBeConnected
                                       let connection = n.Position - node.Position
                                       let angle = Angle.Between(direction, connection)
                                       where System.Math.Abs(angle) < maxAngle
                                       let length = connection.VectorLength()
                                       orderby length
                                       select new RoadGraphEdge(node, n, length);

                foreach (var edge in possibleNewEdges)
                {
                    if (TryAddEdge(edge))
                        break;
                }

                if (node.Neighbors.Count == 1)
                {
                    Point destination = node.Position;
                    do
                    {
                        destination += direction;
                    }
                    while (map.IsWithinMap(destination));

                    destination += direction; // just to be sure that the point is far enough outside   

                    if (!TryAddRoadLeadingOutOfMap(node, destination))
                    {
                        var toDestination = destination - node.Position;
                        for (int degrees = 5; degrees <= 45; degrees += 5)
                        {
                            if (TryAddRoadLeadingOutOfMap(node, node.Position + toDestination.Rotate(node.Position, Angle.ToRadians(degrees))))
                                break;
                            else
                            {
                                if (TryAddRoadLeadingOutOfMap(node, node.Position + toDestination.Rotate(node.Position, Angle.ToRadians(-degrees))))
                                    break;
                            }
                        }
                    }

                    // give up
                }
            }
        }

        private void AddAdditionalRoadsLeadingOutOfMap()
        {
            int count = -1;
            while (roadsLeadingOutOfMap.Count < MinimumRoadsLeadingOutOfMap &&
                   roadsLeadingOutOfMap.Count != count)
            {
                count = roadsLeadingOutOfMap.Count;
                var pointsNextToBorder = from n in info.Roads.Nodes
                                         where n.CanBeConnected &&
                                               n.Neighbors.Count == 2 &&
                                               map.IsWithinMap(n.Position) //&&
                                               //!roadsLeadingOutOfMap.Select(r => r.Node2.Position)
                                               //                     .Any(p => (p - n.Position).VectorLengthSquared() < MinimumDistanceBetweenRoadsLeadingOutOfMap * MinimumDistanceBetweenRoadsLeadingOutOfMap)
                                         let x = System.Math.Min(n.Position.X, map.HeightMap.MapWidth * Map.TileWidth - n.Position.X)
                                         let y = System.Math.Min(n.Position.Y, map.HeightMap.MapHeight * Map.TileWidth - n.Position.Y)
                                         orderby System.Math.Min(x, y)
                                         select new { X = x, Y = y, Node = n };

                foreach (var point in pointsNextToBorder)
                {
                    if (info.IsCancellationRequested)
                        return;

                    Point destination;
                    if (point.X < point.Y)
                    {
                        if (point.X < point.Node.Position.X)
                        {
                            destination = new Point((map.HeightMap.MapWidth + outOfMapDestinationDistance) * Map.TileWidth, point.Node.Position.Y);
                        }
                        else
                        {
                            destination = new Point(-outOfMapDestinationDistance * Map.TileWidth, point.Node.Position.Y);
                        }
                    }
                    else
                    {
                        if (point.Y < point.Node.Position.Y)
                        {
                            destination = new Point(point.Node.Position.X, (map.HeightMap.MapHeight + outOfMapDestinationDistance) * Map.TileWidth);
                        }
                        else
                        {
                            destination = new Point(point.Node.Position.X, -outOfMapDestinationDistance * Map.TileWidth);
                        }
                    }

                    var destinationNode = new RoadGraphNode(destination, false, false);
                    var edge = new RoadGraphEdge(point.Node, destinationNode, (destination - point.Node.Position).VectorLength());
                    if (TryAddRoadLeadingOutOfMap(point.Node, destination))
                    {
                        break;
                    }
                }
            }
        }

        private bool TryAddRoadLeadingOutOfMap(RoadGraphNode node, Point destination)
        {
            if (roadsLeadingOutOfMap.Any(r => (destination - r.Node2.Position).VectorLengthSquared() < MinimumDistanceBetweenRoadsLeadingOutOfMap * MinimumDistanceBetweenRoadsLeadingOutOfMap))
                return false;

            var destinationNode = new RoadGraphNode(destination, false, false);
            var edge = new RoadGraphEdge(node, destinationNode, (destination - node.Position).VectorLength());
            if (TryAddEdge(edge))
            {
                info.Roads.Nodes.AddLast(destinationNode);
                roadsLeadingOutOfMap.Add(edge);
                return true;
            }

            return false;
        }

        private bool TryAddEdge(RoadGraphEdge edge)
        {
            //int count = edge.Length < 150 * 150 ? 5 : 8;
            int count = (int)System.Math.Max((edge.Length / 100) + 1, 3);
                        
            ICurve curve = GetCurve(edge.Node1, edge.Node2);
                       
            var positions = curve.GetPoints(count);
            if (!CanAddIncomingRoad(edge.Node1, positions.ElementAt(1)) || !CanAddIncomingRoad(edge.Node2, positions.ElementAt(count - 2)))
                return false;

            return TryAddEdge(edge, positions);
        }

        private bool TryAddEdge(RoadGraphEdge edge, IEnumerable<Point> positions)
        {
            var tiles = tileGraph.GetTiles(map, positions);
            if (tiles != null) // path possible
            {
                info.Roads.AddRoad(positions, map, info.ObjectFactory, info.Settings.Scenery.Road);
                tileGraph.UpdateTiles(tiles, TileInfo.Road, 2);
                return true;
            }

            return false;
        }

        private bool CanAddIncomingRoad(RoadGraphNode node, Point position)
        {
            var incoming = node.Position - position;
            return node.Neighbors.All(n => Angle.Between(incoming, node.Position - n.Position) >= Angle.ToRadians(minAngleBetweenCrossroads));
        }

        private ICurve GetCurve(RoadGraphNode node1, RoadGraphNode node2)
        {
            ICurve curve;
            if (node1.Neighbors.Count == 1 && node2.Neighbors.Count == 1)
            {
                curve = new CubicBezierCurve(
                    node1.Position,
                    GetControlPoint(node1),
                    GetControlPoint(node2),
                    node2.Position);
            }
            else if (node1.Neighbors.Count == 1 && node2.Neighbors.Count > 1)
            {
                curve = new QuadraticBezierCurve(
                    node1.Position,
                    GetControlPoint(node1),
                    node2.Position);
            }
            else if (node2.Neighbors.Count == 1 && node1.Neighbors.Count > 1)
            {
                curve = new QuadraticBezierCurve(
                    node1.Position,
                    GetControlPoint(node2),
                    node2.Position);
            }
            else
            {
                curve = new LinearBezierCurve(node1.Position, node2.Position);
            }

            return curve;
        }

        private Point GetControlPoint(RoadGraphNode node)
        {
            const float minLength = 100;
            var incoming = (node.Position - node.Neighbors.First().Position);
            var lengthSquared = incoming.VectorLengthSquared();
            if (lengthSquared < minLength * minLength)
                incoming = incoming / (float)System.Math.Sqrt(lengthSquared) * minLength;
            return node.Position + incoming;
        }

        private double CalculateSquaredDistance(RoadGraphNode node1, RoadGraphNode node2)
        {
            return (node1.Position - node2.Position).VectorLength();
        }
    }
}
