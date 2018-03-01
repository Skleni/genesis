#region License

// ResourceGenerator.cs
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Core;
using Genesis.Math;
using Genesis.Objects;
using Genesis.Tiles;
using Genesis.Tools;

namespace Genesis.Processors.CWC
{
    public class ResourceGenerator : IMapProcessor
    {
        class ResourceTypeInfo
        {
            public ResourceTypeInfo(string name, TileInfo nearInfo, TileInfo farInfo)
            {
                Count = 0;
                Name = name;
                NearInfo = nearInfo;
                FarInfo = farInfo;
            }

            public string Name { get; private set; }
            public TileInfo NearInfo { get; private set; }
            public TileInfo FarInfo { get; private set; }
            public int Count { get; set; }
        }

        class ResourceType
        {
            private const string objectName = "objectName";

            public ResourceTypeInfo Info { get; private set; }

            public ObjectTemplate[] Templates { get; private set; }

            public ResourceType(ResourceTypeInfo info, params ObjectTemplate[] templates)
            {
                Info = info;
                Templates = templates;
            }

            public ScriptObject GetResource(ObjectTemplate template, string typeFilter = null)
            {
                var resource = template.Objects.FirstOrDefault(o => Info.Name.Equals(o.GetPropertyValue<string>(objectName)) && (typeFilter == null || o.Type.ToLower().Contains(typeFilter.ToLower())));
                return resource;
            }
        }

        class VillageTemplate
        {
            public ObjectTemplate Template { get; private set; }

            public ScriptObject SmallFlag { get; set; }
            public ScriptObject LargeFlag { get; set; }
            public ScriptObject SmallFuelDepot { get; set; }
            public ScriptObject LargeFuelDepot { get; set; }

            public bool HasSmallFlag { get { return SmallFlag != null; } }
            public bool HasLargeFlag { get { return LargeFlag != null; } }
            public bool HasSmallFuelDepot { get { return SmallFuelDepot != null; } }
            public bool HasLargeFuelDepot { get { return LargeFuelDepot != null; } }

            public bool IsInstantiated { get; set; }

            public VillageTemplate(ObjectTemplate template)
            {
                Template = template;
                IsInstantiated = false;
            }
        }

        private const float minVillageDistance = 200f;

        private Map map;
        private MapInfo info;
        private TileGraph tiles;
        private Random random;

        private Player resourcesPlayer;
        private ResourceType smallFlag;
        private ResourceType largeFlag;
        private ResourceType smallFuelDepot;
        private ResourceType largeFuelDepot;

        private List<VillageTemplate> villages;
        private ReadOnlyCollection<Line> axesOfSymmetry;

        private List<Point> villagePositions;

        public string Description
        {
            get
            {
                return "Generating resources";
            }
        }

        public bool UpdatePreview
        {
            get { return true; }
        }

        public void Process(Map map, MapInfo info)
        {
            InitializeTypes();

            this.map = map;
            this.info = info;
            this.tiles = new TileGraph(info.Tiles, TileInfo.Base | TileInfo.FuelDepotFar | TileInfo.FlagFar);

            this.resourcesPlayer = map.Players.FirstOrDefault(p => p.Properties["playerName"].Value.Equals("Resources"));

            this.axesOfSymmetry = new SymmetryAnalyzer().Analyze(map, info.StartingPositions);
            this.villagePositions = new List<Point>();

            this.random = new Random(info.MapId);

            // There are always at least two axes which should intersect at the map center.
            Point center = axesOfSymmetry[0].Intersect(axesOfSymmetry[1]);

            // Generate a village in the center.
            if (random.Next(3) == 0)
            {
                GenerateVillage(center, villages[random.Next(villages.Count)]);
            }

            // Generate two villages along the primary axis.
            if (random.Next(3) == 0)
            {
                Point[] positions = new Point[2];
                
                var direction = axesOfSymmetry[0].IsVertical ? new Point(0, 1) : new Point(1, axesOfSymmetry[0].Slope).Normalize();
                var maxLength = System.Math.Min(center.X, center.Y);

                do
                {
                    float distance = (float)random.NextDouble() * maxLength;

                    positions[0] = center + direction * distance;
                    positions[1] = center - direction * distance;
                }
                while (!CheckVillagePositions(positions) && !info.IsCancellationRequested);

                GenerateVillages(positions);
            }

            // Generate a village for every player.
            if (random.Next(3) == 0)
            {
                Point[] positions = null;

                int count = 0;
                do
                {
                    positions = GetRandomMirroredPositions();
                    count++;
                }
                while (!CheckVillagePositions(positions) && !info.IsCancellationRequested);

                if (count < 100)
                    GenerateVillages(positions);
            }

            while (smallFlag.Info.Count < 2 * info.NumberOfPlayers && !info.IsCancellationRequested)
                GenerateResource(random.NextDouble() < 0.5 ? smallFlag : largeFlag);

            while (smallFuelDepot.Info.Count < 2 * info.NumberOfPlayers && !info.IsCancellationRequested)
                GenerateResource(random.NextDouble() < 0.5 ? smallFuelDepot : largeFuelDepot);
        }

        private void GenerateVillages(Point[] positions)
        {
            VillageTemplate village = GetVillageTemplate(this.villages);
            var villageClass = from v in villages
                               where v.HasSmallFlag == village.HasSmallFlag &&
                                     v.HasLargeFlag == village.HasLargeFlag &&
                                     v.HasSmallFuelDepot == village.HasSmallFuelDepot &&
                                     v.HasLargeFuelDepot == village.HasLargeFuelDepot
                               select v;

            foreach (var position in positions)
            {
                village = GetVillageTemplate(villageClass);
                GenerateVillage(position, village);
            }
        }

        private int GetProbability(VillageTemplate template)
        {
            var villagesOfSameType = from v in villages
                                     where v.Template.Map.GetPropertyValue<string>("mapName") ==
                                           template.Template.Map.GetPropertyValue<string>("mapName")
                                     select v;
            if (template.IsInstantiated || villagesOfSameType.Any(v => v.IsInstantiated))
            {
                return int.MaxValue;
            }
            else
            {
                return random.Next();
            }
        }

        private VillageTemplate GetVillageTemplate(IEnumerable<VillageTemplate> villages)
        {
            return villages.OrderBy(v => GetProbability(v)).First();
        }

        private void GenerateVillage(Point position, VillageTemplate village)
        {
            var template = village.Template.Clone();

            AddTemplateToMap(template, position);
            villagePositions.Add(position);

            if (village.HasSmallFlag)
                AddResource(smallFlag, template);
            if (village.HasLargeFlag)
                AddResource(largeFlag, template);
            if (village.HasSmallFuelDepot)
                AddResource(smallFuelDepot, template);
            if (village.HasLargeFuelDepot)
                AddResource(largeFuelDepot, template);

            village.IsInstantiated = true;
        }

        private bool CheckVillagePositions(Point[] positions)
        {
            bool inStartingRange = positions.Any(p => info.IsWithinStartingRange(map.PositionToCoordinates(p), info.BaseRadius * 1.5));

            bool tooNearToBorder = positions.Any(p => p.X < minVillageDistance && p.Y < minVillageDistance);

            bool tooNearToVillage =   (from p1 in positions
                                       from p2 in villagePositions
                                       let distance = (p1 - p2).VectorLengthSquared()
                                       where distance > 0 && distance < 4 * minVillageDistance * minVillageDistance
                                       select distance).Any();

            bool tooNearToEachOther = (from p1 in positions
                                       from p2 in positions
                                       let distance = (p1 - p2).VectorLengthSquared()
                                       where distance > 0 && distance < 4 * minVillageDistance * minVillageDistance
                                       select distance).Any();
            
            return !inStartingRange && !tooNearToBorder && !tooNearToVillage && !tooNearToEachOther;
        }

        private void InitializeTypes()
        {
            var flagInfo = new ResourceTypeInfo("flag", TileInfo.FlagNear, TileInfo.FlagFar);
            var fuelDepotInfo = new ResourceTypeInfo("fuel", TileInfo.FuelDepotNear, TileInfo.FuelDepotFar);

            smallFlag = new ResourceType(flagInfo, Directory.GetFiles("Templates/SmallFlag").Select(ObjectTemplate.ReadFromFile).ToArray());
            largeFlag = new ResourceType(flagInfo, Directory.GetFiles("Templates/LargeFlag").Select(ObjectTemplate.ReadFromFile).ToArray());

            smallFuelDepot = new ResourceType(fuelDepotInfo, Directory.GetFiles("Templates/SmallFuelDepot").Select(ObjectTemplate.ReadFromFile).ToArray());
            largeFuelDepot = new ResourceType(fuelDepotInfo, Directory.GetFiles("Templates/LargeFuelDepot").Select(ObjectTemplate.ReadFromFile).ToArray());

            villages = Directory.GetFiles("Templates/Village").Select(ObjectTemplate.ReadFromFile).Select(CreateVillageTemplate).ToList();
        }

        private VillageTemplate CreateVillageTemplate(ObjectTemplate template)
        {
            VillageTemplate village = new VillageTemplate(template);
            village.SmallFlag = smallFlag.GetResource(template, "Small");
            village.LargeFlag = largeFlag.GetResource(template, "Large");
            village.SmallFuelDepot = smallFuelDepot.GetResource(template, "Small") ?? smallFuelDepot.GetResource(template, "Medium");
            village.LargeFuelDepot = largeFuelDepot.GetResource(template, "Large");

            return village;
        }

        private Point[] GetRandomMirroredPositions()
        {
            int minDistance = info.ResourceRadiusNear * (int)Map.TileWidth;

            // generate as many positions as there are axes of symmetry
            Point[] positions = new Point[axesOfSymmetry.Count];

            bool validPositions = false;
            while (!validPositions && !info.IsCancellationRequested)
            {
                // Generate a random position.
                positions[0] = new Point(
                    random.Next(minDistance, (map.HeightMap.MapWidth * (int)Map.TileWidth) - minDistance),
                    random.Next(minDistance, (map.HeightMap.MapHeight * (int)Map.TileWidth) - minDistance));

                validPositions = IsPositionValid(positions[0], TileInfo.FlagFar | TileInfo.FuelDepotFar | TileInfo.Structure);

                // Calculate the other positions by mirroring the original one first on the primary axis
                // and then on each of the other axes.
                for (int i = 1; validPositions && i < axesOfSymmetry.Count; i++)
                {
                    positions[i] = axesOfSymmetry[0].Mirror(positions[0]);
                    positions[i] = axesOfSymmetry[i].Mirror(positions[i]);

                    validPositions = IsPositionValid(positions[i], TileInfo.FlagFar | TileInfo.FuelDepotFar | TileInfo.Structure) &&
                                    (positions[i] - positions[0]).VectorLengthSquared() > (Map.TileWidth * 15) * (Map.TileWidth * 15);
                }
            }

            return positions;
        }

        private bool IsPositionValid(Point position, TileInfo tileInfo)
        {
            if (position.X < 0 || position.Y < 0)
            {
                return false;
            }

            var coordinates = map.PositionToCoordinates(position);
            return (info.Tiles[coordinates.X, coordinates.Y] & tileInfo) == TileInfo.Free && !info.IsWithinStartingRange(coordinates, info.BaseRadius);
        }

        private void GenerateResource(ResourceType type)
        {
            var positions = GetRandomMirroredPositions();

            foreach (var position in positions)
            {
                AddResource(type, position);
            }
        }

        private void AddResource(ResourceType type, Point position)
        {
            var template = type.Templates[random.Next(type.Templates.Length)].Clone();

            AddTemplateToMap(template, position);
            AddResource(type, template);
        }

        private void AddTemplateToMap(ObjectTemplate template, Point position)
        {
            info.ObjectFactory.AddTemplateToMap(template, map, info, TileInfo.Structure, position, (float)(random.NextDouble() * System.Math.PI * 2), info.Settings.Scenery.Road);
        }

        private void AddResource(ResourceType type, ObjectTemplate template)
        {
            type.Info.Count++;
            var resource = type.GetResource(template);

            Point position = new Point(resource.X, resource.Y);

            UpdateTileInfo(position, info.ResourceRadiusNear, type.Info.NearInfo);
            UpdateTileInfo(position, info.ResourceRadiusFar, type.Info.FarInfo);            
        }

        private void UpdateTileInfo(Point position, int radius, TileInfo tileInfo)
        {
            var coordinates = map.PositionToCoordinates(position);
            float squaredRadius = radius * radius * Map.TileWidth * Map.TileWidth;
            for (int x = coordinates.X - radius; x <= coordinates.X + radius; x++)
            {
                for (int y = coordinates.Y - radius; y <= coordinates.Y + radius; y++)
                {
                    if (x >= 0 && x < info.Tiles.Width && y >= 0 && y < info.Tiles.Height)
                    {
                        var vector = position - map.CoordinatesToPosition(new Coordinates(x, y));
                        if (vector.VectorLengthSquared() <= squaredRadius)
                            info.Tiles[x, y] |= tileInfo;
                    }
                }
            }
        }
    }
}
