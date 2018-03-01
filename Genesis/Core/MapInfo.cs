#region License

// MapInfo.cs
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
using System.Threading;
using CkMp.Data.Map;
using Genesis.Math;
using Genesis.Objects;
using Genesis.Roads;
using Genesis.Settings;
using Genesis.Tiles;

namespace Genesis.Core
{
    public class MapInfo
    {
        private CancellationToken token;

        public int MapId { get; private set; }

        public Coordinates[] StartingPositions { get; private set; }

        public int NumberOfPlayers { get { return StartingPositions.Length; } }

        public MapSettings Settings { get; private set; }

        public Frequency NumberOfCliffs { get; private set; }
        public Frequency NumberOfTrees { get; private set; }

        public int BaseRadius { get; set; }
        public int ResourceRadiusNear { get; set; }
        public int ResourceRadiusFar { get; set; }

        public Grid<TileInfo> Tiles { get; private set; }
        public Grid<Texture> TextureData { get; private set; }

        public RoadGraph Roads { get; private set; }
        public ICollection<Circle> FlattenedAreas { get; set; }

        public ObjectFactory ObjectFactory { get; private set; }

        public ICollection<string> MapIniTemplates { get; private set; }

        public bool IsCancellationRequested
        {
            get { return token != null && token.IsCancellationRequested; }
        }

        public MapInfo(int mapId, int width, int height, MapSettings settings, int numberOfPlayers, CancellationToken cancellationToken)
        {
            token = cancellationToken;

            MapId = mapId;
            Settings = settings;

            ObjectFactory = new ObjectFactory();

            Tiles = new Grid<TileInfo>(width, height);
            TextureData = new Grid<Texture>(width + 1, height + 1);

            StartingPositions = new Coordinates[numberOfPlayers];
            Roads = new RoadGraph();
            FlattenedAreas = new List<Circle>();

            BaseRadius = 40;
            ResourceRadiusNear = 5;
            ResourceRadiusFar = 40;

            MapIniTemplates = new List<string>();
        }

        public bool IsWithinStartingRange(Coordinates coordinates, double distance)
        {
            double squaredDistance = distance * distance;
            foreach (Coordinates startingPosition in StartingPositions)
            {
                var vector = coordinates - startingPosition;
                int lengthSquared = vector.X * vector.X + vector.Y * vector.Y;
                if (lengthSquared < squaredDistance)
                    return true;
            }

            return false;
        }
    }
}
