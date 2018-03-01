#region License

// ObjectFactory.cs
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
using Genesis.Core;
using Genesis.Roads;
using Genesis.Tiles;

namespace Genesis.Objects
{
    public class ObjectFactory
    {
        private const float endpointConnectionRange = 20f;

        private int objectId = 0;
        private int waypointId = 1;

        public ScriptObject CreateObject(string type, float x, float y, float z, float rotation, String uniqueId = null, Player owner = null)
        {
            var obj = new ScriptObject()
            {
                Type = type,
                Rotation = rotation,
                X = x,
                Y = y,
                Z = z
            };

            string originalOwner = "team";
            if (owner != null)
                originalOwner += owner.Properties["playerName"].Value;

            obj.SetProperty("objectInitialHealth", PropertyType.Integer, 100);
            obj.SetProperty("objectEnabled", PropertyType.Boolean, true);
            obj.SetProperty("objectIndestructable", PropertyType.Boolean, false);
            obj.SetProperty("objectUnsellable", PropertyType.Boolean, false);
            obj.SetProperty("objectPowered", PropertyType.Boolean, true);
            obj.SetProperty("objectRecruitableAI", PropertyType.Boolean, true);
            obj.SetProperty("objectTargetable", PropertyType.Boolean, false);
            obj.SetProperty("originalOwner", PropertyType.OneByteString, originalOwner);
            obj.SetProperty("uniqueID", PropertyType.OneByteString, uniqueId ?? objectId.ToString());

            objectId++;

            return obj;
        }

        public ScriptObject CreateWaypoint(float x, float y, string name = null)
        {
            var waypoint = CreateObject(ScriptObject.WaypointObjectType, x, y, 0f, 0f);
            if (name == null)
                name = "Waypoint " + waypointId;
            waypoint.Properties["uniqueID"].Value = objectId.ToString();
            waypoint.SetProperty("waypointID", PropertyType.Integer, waypointId);
            waypoint.SetProperty("waypointName", PropertyType.OneByteString, name);

            waypointId++;
            objectId++;

            return waypoint;
        }

        public Area CreateArea(string name, params Point[] points)
        {
            var area = new Area()
            {
                Id = objectId++,
                Name = name,
                IsWater = false,
                IsRiver = false,
                RiverStart = 0
            };

            foreach (var point in points)
                area.Points.Add(new int[] { (int)point.X, (int)point.Y, 0 });

            return area;
        }

        public void AddTemplateToMap(ObjectTemplate template, Map map, MapInfo info, TileInfo blockedTileInfo, Point position, float? rotation, string roadType)
        {
            if (template.IsInstantiated)
                throw new InvalidOperationException("A template can only be added to a map once. Consider cloning it before adding it to the map.");

            Point origin = template.CalculateOrigin();

            foreach (var obj in template.Objects)
            {
                obj.Properties["uniqueID"].Value = objectId.ToString();
                Point point = new Point(position.X + obj.X - origin.X, position.Y + obj.Y - origin.Y);
                if (rotation.HasValue)
                {
                    point = point.Rotate(position, rotation.Value);
                    obj.Rotation += rotation.Value;
                }

                obj.X = point.X;
                obj.Y = point.Y;

                map.Objects.Add(obj);
                objectId++;
            }

            AddFlattenedArea(template, info);

            AddTextures(template, map, info, blockedTileInfo, position, rotation, origin);

            AddRoadsFromTemplateToMap(template, map, info, roadType);

            template.IsInstantiated = true;
        }

        private static void AddTextures(ObjectTemplate template, Map map, MapInfo info, TileInfo blockedTileInfo, Point position, float? rotation, Point origin)
        {
            float width = template.Tiles.Width * Map.TileWidth;
            float height = template.Tiles.Height * Map.TileWidth;

            Point[] corners = new Point[]
                {
                    new Point(position.X - origin.X, position.Y - origin.Y),
                    new Point(position.X - origin.X, position.Y - origin.Y + height),
                    new Point(position.X - origin.X + width, position.Y - origin.Y + height),
                    new Point(position.X - origin.X + width, position.Y - origin.Y)
                };

            if (rotation.HasValue)
            {
                for (int i = 0; i < corners.Length; i++)
                {
                    corners[i] = corners[i].Rotate(position, rotation.Value);
                }                
            }

            var bottomLeft = map.PositionToCoordinates(new Point(corners.Min(c => c.X), corners.Min(c => c.Y)));
            var topRight = map.PositionToCoordinates(new Point(corners.Max(c => c.X), corners.Max(c => c.Y)));

            for (int x = bottomLeft.X; x <= topRight.X; x++)
            {
                for (int y = bottomLeft.Y; y <= topRight.Y; y++)
                {
                    if (map.Tiles.CheckCoordinates(x, y))
                    {
                        Point positionInTemplate = map.CoordinatesToPosition(new Coordinates(x, y));

                        if (rotation.HasValue)
                        {
                            positionInTemplate = positionInTemplate.Rotate(position, -rotation.Value);
                        }

                        positionInTemplate = positionInTemplate - position + origin;
                        
                        var coordinates = template.Map.PositionToCoordinates(positionInTemplate);
                        if (template.Tiles.CheckCoordinates(coordinates.X, coordinates.Y))
                        {
                            if (template.Tiles[coordinates.X, coordinates.Y].Impassable)
                            {
                                info.Tiles[x, y] |= blockedTileInfo;
                            }

                            Texture texture = template.Tiles.GetTexture(coordinates.X, coordinates.Y);
                            if (texture.Name != ObjectTemplate.TextureTemplate)
                            {
                                texture = new Texture(texture.Name, texture.BlocksPerRow);
                                texture = map.Tiles.AddTexture(texture);
                                info.TextureData[x, y] = texture;
                            }
                        }
                    }
                }
            }
        }

        private static void AddFlattenedArea(ObjectTemplate template, MapInfo info)
        {
            Point origin = template.CalculateOrigin();

            float squaredRadius = template.Objects/*.Where(o => o.RoadOptions == RoadOptions.None)*/.Max(o => (new Point(o.X, o.Y) - origin).VectorLengthSquared());
            info.FlattenedAreas.Add(new Math.Circle(origin, System.Math.Max((float)System.Math.Sqrt(squaredRadius), Map.TileWidth * 5)));
        }

        private void AddRoadsFromTemplateToMap(ObjectTemplate template, Map map, MapInfo info, string roadType)
        {
            var tileGraph = new TileGraph(info.Tiles, TileInfo.Free);
            var roads = template.Objects.Where(o => o.RoadOptions != RoadOptions.None && o.Type == ObjectTemplate.RoadTemplate).ToArray();
            int count = info.Roads.Nodes.Count;

            ScriptObject road1, road2;

            for (int i = 0; i < roads.Length; i += 2)
            {
                road1 = roads[i];
                road2 = roads[i + 1];

                var start = info.Roads.FindNode(new Point(road1.X, road1.Y));
                if (start == null)
                {
                    start = new RoadGraphNode(new Point(road1.X, road1.Y), !road1.RoadOptions.HasFlag(RoadOptions.Join) && roads.Count(r2 => road1.X == r2.X && road1.Y == r2.Y) == 1, true);
                    info.Roads.Nodes.AddLast(start);
                }

                var end = info.Roads.FindNode(new Point(road2.X, road2.Y));
                if (end == null)
                {
                    end = new RoadGraphNode(new Point(road2.X, road2.Y), !road2.RoadOptions.HasFlag(RoadOptions.Join) && roads.Count(r2 => road2.X == r2.X && road2.Y == r2.Y) == 1, true);
                    info.Roads.Nodes.AddLast(end);
                }

                start.LinkTo(end);

                road1.Type = roadType;
                road2.Type = roadType;

                info.Roads.Segments.Add(new Tuple<ScriptObject, ScriptObject>(road1, road2));
                tileGraph.UpdateTiles(new[] { map.PositionToCoordinates(start.Position), map.PositionToCoordinates(end.Position) }, TileInfo.Road);
            }

            IEnumerable<RoadGraphNode> nodes = info.Roads.Nodes.Skip(count).ToArray();

            var endpoints = from r in nodes
                            where !r.CanBeConnected && r.Neighbors.Count == 1
                            select r;

            foreach (var endpoint in endpoints)
            {
                foreach (var node in nodes)
                {
                    if (node != endpoint && (node.Position - endpoint.Position).VectorLengthSquared() < endpointConnectionRange * endpointConnectionRange)
                        endpoint.LinkTo(node);
                }
            }
        }
    }
}
