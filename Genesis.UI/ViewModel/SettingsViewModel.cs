#region License

// SettingsViewModel.cs
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CkMp.Data.Enumerations;
using Genesis.Settings;

namespace Genesis.UI.ViewModel
{
    public class SettingsViewModel : ValidatingViewModel
    {
        private PreviewViewModel preview;

        public ObservableCollection<Scenery> Sceneries { get; private set; }
        public ObservableCollection<MapLayout> MapLayouts { get; private set; }
        public ObservableCollection<Frequency> Frequencies { get; private set; }
        public ObservableCollection<TimeOfDay> TimesOfDay { get; private set; }
        public ObservableCollection<int> Players { get; private set; }

        private bool isModified;
        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (value != isModified)
                {
                    isModified = value;
                    OnPropertyChanged(() => IsModified);
                }
            }
        }

        private int numberOfPlayers;
        public int NumberOfPlayers
        {
            get { return numberOfPlayers; }
            set
            {
                if (value != numberOfPlayers)
                {
                    numberOfPlayers = value;
                    OnPropertyChanged(() => NumberOfPlayers);
                    ValidateSize();
                }
            }
        }

        private int width;
        public int Width
        {
            get { return width; }
            set
            {
                if (value != width)
                {
                    width = value;
                    OnPropertyChanged(() => Width);
                    OnPropertyChanged(() => TotalWidth);
                    ValidateSize();
                }
            }
        }

        private int height;
        public int Height
        {
            get { return height; }
            set
            {
                if (value != height)
                {
                    height = value;
                    OnPropertyChanged(() => Height);
                    OnPropertyChanged(() => TotalHeight);
                    ValidateSize();
                }
            }
        }
        
        private int border;
        public int Border
        {
            get { return border; }
            set
            {
                if (value != border)
                {
                    border = value;
                    OnPropertyChanged(() => Border);
                    OnPropertyChanged(() => TotalWidth);
                    OnPropertyChanged(() => TotalHeight);
                }
            }
        }

        private Scenery scenery;
        public Scenery Scenery
        {
            get { return scenery; }
            set
            {
                if (value != scenery)
                {
                    scenery = value;
                    OnPropertyChanged(() => Scenery);
                }
            }
        }

        private MapLayout mapLayout;
        public MapLayout MapLayout
        {
            get { return mapLayout; }
            set
            {
                if (value != mapLayout)
                {
                    mapLayout = value;
                    OnPropertyChanged(() => MapLayout);
                }
            }
        }

        private int mapId;
        public int MapId
        {
            get { return mapId; }
            set
            {
                if (value != mapId)
                {
                    mapId = value;
                    OnPropertyChanged(() => MapId);
                }
            }
        }

        private Frequency treeFrequency;
        public Frequency TreeFrequency
        {
            get { return treeFrequency; }
            set
            {
                if (value != treeFrequency)
                {
                    treeFrequency = value;
                    OnPropertyChanged(() => TreeFrequency);
                }
            }
        }

        private Frequency cliffFrequency;
        public Frequency CliffFrequency
        {
            get { return cliffFrequency; }
            set
            {
                if (value != cliffFrequency)
                {
                    cliffFrequency = value;
                    OnPropertyChanged(() => CliffFrequency);
                }
            }
        }

        private TimeOfDay timeOfDay;
        public TimeOfDay TimeOfDay
        {
            get { return timeOfDay; }
            set
            {
                if (value != timeOfDay)
                {
                    timeOfDay = value;
                    OnPropertyChanged(() => TimeOfDay);
                }
            }
        }

        public int TotalWidth
        {
            get { return Width + 2 * Border; }
        }

        public int TotalHeight
        {
            get { return Height + 2 * Border; }
        }

        public SettingsViewModel(IEnumerable<MapLayout> mapLayouts, IEnumerable<Scenery> textureSets, PreviewViewModel preview)
        {
            this.preview = preview;
            MapLayouts = new ObservableCollection<MapLayout>(mapLayouts);
            MapLayout = MapLayouts.FirstOrDefault();

            Sceneries = new ObservableCollection<Scenery>(textureSets);
            Scenery = Sceneries.FirstOrDefault();

            Frequencies = new ObservableCollection<Frequency>((Frequency[])Enum.GetValues(typeof(Frequency)));
            TreeFrequency = Frequency.Medium;
            CliffFrequency = Frequency.Medium;

            TimesOfDay = new ObservableCollection<TimeOfDay>((TimeOfDay[])Enum.GetValues(typeof(TimeOfDay)));
            TimeOfDay = TimeOfDay.Afternoon;

            Players = new ObservableCollection<int>() { 2, 4 };
            NumberOfPlayers = 2;

            Width = 250;
            Height = 250;
            Border = 30;

            IsModified = false;
        }

        private void ValidateSize()
        {
            switch (NumberOfPlayers)
            {
                case 2:
                    ValidateSize(150);
                    break;
                case 4:
                    ValidateSize(300);
                    break;
                default:
                    throw new InvalidOperationException("Invalid number of players.");
            }
        }

        private void ValidateSize(int size)
        {
            if (Width < size)
                AddError(() => Width, string.Format("The minimum map width for {0} players is {1}.", NumberOfPlayers, size));
            else
                ClearError(() => Width);
            OnPropertyChanged(() => Width);

            if (Height < size)
                AddError(() => Height, string.Format("The minimum map height for {0} players is {1}.", NumberOfPlayers, size));
            else
                ClearError(() => Height);
            OnPropertyChanged(() => Height);
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName != "IsModified")
            {
                IsModified = true;
            }
        }
    }
}
