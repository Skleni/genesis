#region License

// MapGenerator.cs
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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CkMp.Data;
using CkMp.Data.Enumerations;
using CkMp.Data.Lighting;
using CkMp.Data.Map;
using CkMp.Data.Objects;
using Genesis.Core;
using Genesis.Previews;
using Genesis.Settings;
using Genesis.Tools;

namespace Genesis
{
	public class MapGenerator
	{
		public event EventHandler<LogEventArgs> Log;

        public event EventHandler PreviewUpdated;

		private Random random = new Random();

		private MapInfo info;

        public MapSettings Settings { get; set; }

		public int MapId { get; set; }

        public Map Map { get; private set; }

        public bool IsGenerating { get; private set; }

		public PreviewGenerator PreviewGenerator { get; private set; }

        public IEnumerable<IMapProcessor> Processors { get; private set; }

        public MapGenerator(IEnumerable<IMapProcessor> processors)
		{
            Processors = processors;
			PreviewGenerator = new PreviewGenerator();
			NewMap();
		}

		public int NewMap()
		{
			return MapId = random.Next();
		}

		public void CreateMap(int width, int height, int border, int players, Weather weather, TimeOfDay time)
		{
			Map = new Map(width, height, border);
			Map.GlobalLightOptions = GlobalLightOptions.GetDefault();
			Map.GlobalLightOptions.TimeOfDay = time;

			Map.SetProperty("compression", PropertyType.Integer, 0);
			Map.SetProperty("weather", PropertyType.Integer, weather);

			for (int i = 0; i < Map.HeightMap.Width; i++)
			{
				for (int j = 0; j < Map.HeightMap.Height; j++)
				{
					Map.HeightMap[i, j] = Settings.Layout.Height;
				}
			}
		}

        public void GenerateMap(int width, int height, int border, int players)
        {
            GenerateMap(width, height, border, players, CancellationToken.None);
        }

		public void GenerateMap(int width, int height, int border, int players, CancellationToken token)
		{
			IsGenerating = true;

            try
            {
                CreateMap(width, height, border, players, Settings.Scenery.Weather, Settings.TimeOfDay);

                info = new MapInfo(MapId, Map.HeightMap.Width, Map.HeightMap.Height, Settings, players, token);

                int previewCount = 0;

                string message = null;
                foreach (var processor in Processors)
                {
                    if (token != null && token.IsCancellationRequested)
                    {
                        IsGenerating = false;
                        token.ThrowIfCancellationRequested();
                    }

                    message = processor.Description;
                    if (!string.IsNullOrWhiteSpace(message) && Log != null)
                        Log(processor, new LogEventArgs(message));

                    processor.Process(Map, info);

                    if (processor.UpdatePreview)
                    {
                        Task.Factory.StartNew(() =>
                        {
                            int count = Interlocked.Increment(ref previewCount);
                            PreviewGenerator.Process(Map, info);

                            if (count == previewCount && PreviewUpdated != null)
                            {
                                byte[] bmpPreview;
                                lock (PreviewGenerator.Lock)
                                {
                                    bmpPreview = PreviewGenerator.BmpPreview;
                                }
                                if (count == previewCount && PreviewUpdated != null)
                                    PreviewUpdated(this, new PreviewUpdatedEventArgs(bmpPreview));
                            }
                        });
                    }
                }
            }
            finally
            {
			    IsGenerating = false;
            }
		}

		public void SaveMap(string name, string path)
		{
			string directory = Path.Combine(path, name);
			Directory.CreateDirectory(directory);

			var writer = new Writer();
			writer.Map = Map;
			writer.WriteFile(Path.Combine(directory, name + ".map"));

			new MapIniWriter().Write(Path.Combine(directory, "map.ini"), info.MapIniTemplates);

			File.WriteAllBytes(Path.Combine(directory, name + ".tga"), PreviewGenerator.TgaPreview);
		}
	}
}
