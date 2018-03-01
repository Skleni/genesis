#region License

// SkirmishAIGenerator.cs
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Algorithms;
using Genesis.Core;
using Genesis.Math;
using Genesis.Tiles;

namespace Genesis.Processors.CWC
{
	public class SkirmishAIGenerator : IMapProcessor
	{
        private const string objectName = "objectName";

		private const float innerPerimeterOffset = 100f;
		private const float outerPerimeterOffset = 200f;
		private const float resourceAreaRadius = 200f;
		private const float angleBetweenPaths = 70f;

		private const TileInfo invalidTiles = TileInfo.Structure | TileInfo.Cliff;

		private Map map;
		private MapInfo info;

		private float mapWidth, mapHeight;
		private Point center;
		private Point[] corners;
		private List<Area> innerPerimeters;
		private List<Area> outerPerimeters;
		private List<ScriptObject> resources;

		private SnailShell<Coordinates> snailShell;

		private float pathLength;

		private int flags, fuelDepots;

        public string Description
        {
            get
            {
                return "Generating skirmish AI";
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

			Initialize();

			ProcessResources(map, info); 

			for (int i = 1; i <= info.NumberOfPlayers; i++)
			{
				Point startingPosition = map.CoordinatesToPosition(info.StartingPositions[i - 1]);

				AddPerimeter("InnerPerimeter" + i, startingPosition, info.BaseRadius * Map.TileWidth + innerPerimeterOffset, innerPerimeters);
				AddPerimeter("OuterPerimeter" + i, startingPosition, info.BaseRadius * Map.TileWidth + outerPerimeterOffset, outerPerimeters);

				AddPaths(startingPosition, i);
			}

			map.Objects.Add(info.ObjectFactory.CreateWaypoint(center.X, center.Y, "MapCenter"));

			map.Areas.Add(
				info.ObjectFactory.CreateArea(
					"SkirmishWorld",
					corners));

			AddCombatZone();           

			map.Players.Add(new Player("SkirmishRussia", false, "SkirmishRussia", "FactionRussia", string.Empty, string.Empty, 0x00, 0xFF, 0x00, 0x00));
			map.Players.Add(new Player("SkirmishUSA", false, "SkirmishUSA", "FactionUSA", string.Empty, string.Empty, 0x00, 0x00, 0x00, 0xFF));        

			new ScriptImporter() { ScriptFile = "Scripts\\USA Skirmish Scripts.scb", PlayerName = "SkirmishUSA" }.Process(map, info);
			new ScriptImporter() { ScriptFile = "Scripts\\Russia Skirmish Scripts.scb", PlayerName = "SkirmishRussia" }.Process(map, info);
		}

		private void Initialize()
		{
			flags = 0;
			fuelDepots = 0;

			resources = new List<ScriptObject>();

			innerPerimeters = new List<Area>(info.NumberOfPlayers);
			outerPerimeters = new List<Area>(info.NumberOfPlayers);

			mapWidth = map.HeightMap.MapWidth * Map.TileWidth;
			mapHeight = map.HeightMap.MapHeight * Map.TileWidth;

			center = new Point(map.HeightMap.MapWidth * Map.TileWidth / 2, map.HeightMap.MapHeight * Map.TileWidth / 2);
			corners = new Point[]
			{
				new Point(0, 0),
				new Point(mapWidth, 0),
				new Point(mapWidth, mapHeight),
				new Point(0, mapHeight)
			};

			pathLength = 2 * info.BaseRadius * Map.TileWidth;

			snailShell = new SnailShell<Coordinates>((x, y) => new Coordinates(x, y));
		}

		private void ProcessResources(Map map, MapInfo info)
		{
			int count = map.Objects.Count;
			for (int i = 0; i < count; i++)
			{
				var resource = map.Objects[i];
				if (resource.IsFlag())
				{
					flags++;
					resource.SetProperty(objectName, PropertyType.OneByteString, "flag" + flags.ToString("00"));
					ProcessResource(resource);
				}
                else if (resource.IsFuelDepot())
				{
					fuelDepots++;
					resource.SetProperty(objectName, PropertyType.OneByteString, "fuel" + fuelDepots.ToString("00"));
					ProcessResource(resource);
				}
			}
		}

		private void ProcessResource(ScriptObject resource)
		{
			resources.Add(resource);

			string name = resource.GetPropertyValue<string>(objectName);
			map.Objects.Add(info.ObjectFactory.CreateWaypoint(resource.X, resource.Y, name + "_wp"));

			Point position = resource.GetPosition();
			map.Areas.Add(info.ObjectFactory.CreateArea(
				name + "_area",
				position + new Point(-resourceAreaRadius, -resourceAreaRadius),
				position + new Point(-resourceAreaRadius, +resourceAreaRadius),
				position + new Point(+resourceAreaRadius, +resourceAreaRadius),
				position + new Point(+resourceAreaRadius, -resourceAreaRadius)));
		}

		private void AddPerimeter(string name, Point startingPosition, float radius, List<Area> areas)
		{
			var perimeter = info.ObjectFactory.CreateArea(
				name,
				new Point(System.Math.Max(0, startingPosition.X - radius), System.Math.Max(0, startingPosition.Y - radius)),
				new Point(System.Math.Max(0, startingPosition.X - radius), System.Math.Min(mapHeight, startingPosition.Y + radius)),
				new Point(System.Math.Min(mapWidth, startingPosition.X + radius), System.Math.Min(mapHeight, startingPosition.Y + radius)),
				new Point(System.Math.Min(mapWidth, startingPosition.X + radius), System.Math.Max(0, startingPosition.Y - radius)));

			areas.Add(perimeter);
			map.Areas.Add(perimeter);
		}

		private void AddPaths(Point startingPosition, int player)
		{
			Point towardsCenter = (center - startingPosition).ScaleTo(pathLength);
			
			Point centerStart = startingPosition + towardsCenter;
			map.AddWaypath(
				"Center" + player,
				info.ObjectFactory.CreateWaypoint(centerStart.X, centerStart.Y),
				info.ObjectFactory.CreateWaypoint(startingPosition.X, startingPosition.Y));

			Point flankStart = centerStart.Rotate(startingPosition, Angle.ToRadians(angleBetweenPaths));
			map.AddWaypath(
				"Flank" + player,
				info.ObjectFactory.CreateWaypoint(flankStart.X, flankStart.Y),
				info.ObjectFactory.CreateWaypoint(startingPosition.X, startingPosition.Y));

			Point backDoorStart = centerStart.Rotate(startingPosition, Angle.ToRadians(-angleBetweenPaths));
			map.AddWaypath(
				"BackDoor" + player,
				info.ObjectFactory.CreateWaypoint(backDoorStart.X, backDoorStart.Y),
				info.ObjectFactory.CreateWaypoint(startingPosition.X, startingPosition.Y));

			AddPatrolPath(startingPosition, flankStart, "player" + player + "_patrol1");
			AddPatrolPath(startingPosition, backDoorStart, "player" + player + "_patrol2");
		}

		private void AddPatrolPath(Point startingPosition, Point origin, string name)
		{
			Point towardsStartingPosition = (startingPosition - origin).Normalize();

			var orderedResources = resources.OrderBy(r => (new Point(r.X, r.Y) - origin).VectorLengthSquared());

			Point p1 = origin + towardsStartingPosition * pathLength / 2;
			Point p2 = p1 + (orderedResources.ElementAt(0).GetPosition() - p1).Truncate(pathLength / 2);
			Point p3 = p1 + (orderedResources.ElementAt(1).GetPosition() - p1).Truncate(pathLength / 2);

			var waypoint = CreateWaypoint(p1);
			map.AddWaypath(
				name,
				waypoint,
				CreateWaypoint(p2),
				CreateWaypoint(p3),
				waypoint);
		}

		private ScriptObject CreateWaypoint(Point position)
		{
			var coordinates = map.PositionToCoordinates(position);
			var enumerator = snailShell.GetCoordinates(coordinates.X, coordinates.Y).GetEnumerator();
			while ((info.Tiles[coordinates.X, coordinates.Y] & invalidTiles) != TileInfo.Free)
			{
				enumerator.MoveNext();
				coordinates = enumerator.Current;
			}

			position = map.CoordinatesToPosition(coordinates);
			return info.ObjectFactory.CreateWaypoint(position.X, position.Y);
		}

		private void AddCombatZone()
		{
			List<IEnumerable<Point>>[] nearestPerimetersPerCorner = new List<IEnumerable<Point>>[corners.Length];
			for (int i = 0; i < nearestPerimetersPerCorner.Length; i++)
			{
				nearestPerimetersPerCorner[i] = new List<IEnumerable<Point>>();
			}

			for (int player = 0; player < outerPerimeters.Count; player++)
			{
				var points = outerPerimeters[player].Points.Select(array => new Point(array[0], array[1])).ToArray();
				var nearest = (from point in Enumerable.Range(0, points.Length)
							   from corner in Enumerable.Range(0, corners.Length)
							   let distance = (points[point] - corners[corner]).VectorLengthSquared()
							   orderby distance
							   select new { Corner = corner, Point = point }).First();

				nearestPerimetersPerCorner[nearest.Corner].Add(
					points.Skip(nearest.Point).Take(points.Length - nearest.Point).Concat(points.Take(nearest.Point)));
			}

			List<Point> combatZone = new List<Point>();
			for (int corner = 0; corner < corners.Length; corner++)
			{
				combatZone.Add(corners[corner]);
				foreach (var perimeter in nearestPerimetersPerCorner[corner])
				{
					foreach (var point in perimeter)
					{
						combatZone.Add(point);
					}
					combatZone.Add(perimeter.First());
					combatZone.Add(corners[corner]);
				}
			}

			map.Areas.Add(info.ObjectFactory.CreateArea("CombatZone", combatZone.ToArray()));
		}
	}   
}
