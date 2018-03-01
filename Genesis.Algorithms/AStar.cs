#region License

// AStar.cs
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

namespace Genesis.Algorithms
{
    /// <summary>
    /// A-Star implementation independent of any special graph representation.
    /// </summary>
    /// <typeparam name="TNode">Some type identifying a node in the graph.</typeparam>
    public class AStar<TNode>: IComparer<AStar<TNode>.SearchNode>
        where TNode: IEquatable<TNode>
    {

        #region SearchNode

        private class SearchNode
        {
            public TNode Node { get; set; }
            public double CostFromSource { get; set; }
            public double CostToDestination { get; set; }
            public double TotalCost { get { return CostFromSource + CostToDestination; } }
            public SearchNode Parent { get; set; }
        }

        int IComparer<AStar<TNode>.SearchNode>.Compare(AStar<TNode>.SearchNode x, AStar<TNode>.SearchNode y)
        {
            return x.TotalCost.CompareTo(y.TotalCost);
        }

        #endregion

        private ICollection<SearchNode> openList;
        private ICollection<SearchNode> closedList;

        private Func<TNode, IEnumerable<TNode>> getNeighbors;
        private Func<TNode, TNode, double> getDistance;
        private Func<TNode, TNode, double> estimateDistance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AStar&lt;TNode&gt;"/> class.
        /// </summary>
        /// <param name="getNeighbors">A function returning all neighbors for a given node.</param>
        /// <param name="getDistance">A function returning the distance between two connected nodes.</param>
        /// <param name="estimateDistance">A function estimating the distance between any two nodes in the graph. The result should be as close to the real distance as possible without ever overestimating it.</param>
        public AStar(Func<TNode, IEnumerable<TNode>> getNeighbors, Func<TNode, TNode, double> getDistance, Func<TNode, TNode, double> estimateDistance)
	    {
            this.getNeighbors = getNeighbors;
            this.getDistance = getDistance;
            this.estimateDistance = estimateDistance;
	    }

        /// <summary>
        /// Finds the shortest path (if any exist) between two nodes in a graph.
        /// </summary>
        /// <param name="start">The start node.</param>
        /// <param name="destination">The destination node.</param>
        /// <param name="length">The length of the path.</param>
        /// <returns>The nodes in the order they need to be visited to get from the start node to the destination node.</returns>
        public IEnumerable<TNode> FindShortestPath(TNode start, TNode destination, out double length)
        {
            openList = new SortedSet<SearchNode>(this);
            closedList = new SortedSet<SearchNode>(this);

            SearchNode node = new SearchNode() { Node = start, CostFromSource = 0, CostToDestination = estimateDistance(start, destination) };
            openList.Add(node);

            while (openList.Count > 0)
            {
                node = openList.First();
                openList.Remove(node);

                if (node.Node.Equals(destination))
                {
                    length = node.CostFromSource;
                    LinkedList<TNode> path = new LinkedList<TNode>();
                    while (node != null)
                    {
                        path.AddFirst(node.Node);
                        node = node.Parent;
                    }
                    return path;
                }
                
                foreach (TNode neighbor in getNeighbors(node.Node))
                {
                    double costFromSource = node.CostFromSource + getDistance(node.Node, neighbor);
                    double costToDestination = estimateDistance(neighbor, destination);
                    double totalCost = costFromSource + costToDestination;

                    SearchNode existing = openList.FirstOrDefault(n => n.Node.Equals(neighbor));
                    if (existing != null)
                    {
                        if (totalCost < existing.TotalCost)
                        {
                            openList.Remove(existing);
                            existing.CostFromSource = costFromSource;
                            existing.CostToDestination = costToDestination;
                            existing.Parent = node;
                            openList.Add(existing);
                        }
                    }
                    else
                    {
                        existing = closedList.FirstOrDefault(n => n.Node.Equals(neighbor));
                        if (existing != null)
                        {
                            if (totalCost < existing.TotalCost)
                            {
                                existing.CostFromSource = costFromSource;
                                existing.CostToDestination = costToDestination;
                                existing.Parent = node;
                                UpdateChildren(existing);
                            }
                        }
                        else
                        {
                            openList.Add(new SearchNode() { Node = neighbor, Parent = node, CostFromSource = costFromSource, CostToDestination = costToDestination });
                        }
                    }
                }
                closedList.Add(node);
            }

            length = -1;
            return null;
        }

        private void UpdateChildren(SearchNode node)
        {
            foreach (TNode neighbor in getNeighbors(node.Node))
            {
                SearchNode neighborNode = closedList.FirstOrDefault(n => n.Node.Equals(neighbor));
                if (neighborNode != null)
                {
                    double newCost = node.CostFromSource + estimateDistance(node.Node, neighbor);
                    if (newCost < neighborNode.CostFromSource)
                    {
                        neighborNode.CostFromSource = newCost;
                        neighborNode.Parent = node;
                        UpdateChildren(neighborNode);
                    }
                }
            }
        }
    }
}
