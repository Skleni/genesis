#region License

// PreviewViewModel.cs
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

using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CkMp.Data.Map;
using CkMp.Data.Objects;

namespace Genesis.UI.ViewModel
{
    public class PreviewViewModel : ViewModel
    {
        private WriteableBitmap image;
        public WriteableBitmap Image
        {
            get { return image; }
            set
            {
                if (value != image)
                {
                    image = value;
                    OnPropertyChanged(() => Image);
                }
            }
        }

        public ObservableCollection<OverlayViewModel> StartingPositions { get; private set; }
        public ObservableCollection<OverlayViewModel> Resources { get; private set; }

        public PreviewViewModel()
        {
            StartingPositions = new ObservableCollection<OverlayViewModel>();
            Resources = new ObservableCollection<OverlayViewModel>();
        }

        public void Reset(int width, int height)
        {
            if (width > 0 && height > 0)
            {
                Image = new WriteableBitmap(width, height, 96, 96, PixelFormats.Gray8, BitmapPalettes.Gray256);
            }

            StartingPositions.Clear();
            Resources.Clear();
        }

        public void UpdatePreview(Map map, byte[] imageData)
        {
            int width = map.HeightMap.MapWidth;
            int height = map.HeightMap.MapHeight;
            double scale = System.Math.Max(width, height) / 300.0;

            Reset(width, height);
            Image.WritePixels(new Int32Rect(0, 0, width, height), imageData, width, 0);

            // copy to array so that the generation can go on in the background
            var startingPositions = map.Objects.ToArray().Where(o => o.IsWaypoint && Regex.IsMatch(o.GetPropertyValue<string>("waypointName"), "Player_[1-8]_Start"));
            var resources = map.Objects.ToArray().Where(o => o.IsResource());

            foreach (var startingPosition in startingPositions)
            {
                StartingPositions.Add(new OverlayViewModel(startingPosition.X / Map.TileWidth, height - startingPosition.Y / Map.TileWidth, scale));
            }

            foreach (var resource in resources)
            {
                Resources.Add(new OverlayViewModel(resource.X / Map.TileWidth, height - resource.Y / Map.TileWidth, scale));
            }
        }
    }
}
