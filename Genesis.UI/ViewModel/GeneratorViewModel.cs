#region License

// GeneratorViewModel.cs
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Linq;
using Genesis.Settings;

namespace Genesis.UI.ViewModel
{
    class GeneratorViewModel : ViewModel
    {
        public event EventHandler Generating;
        public event EventHandler Generated;

        private Dispatcher dispatcher;

        private StringBuilder messages;
        private CancellationTokenSource cancellation;

        public string Messages { get { return messages.ToString(); } }
        public string LogPath { get; set; }
        public string OutputPath { get; set; }
        public string MapNameTemplate { get; set; }
        public MapGenerator Generator { get; private set; }
        public PreviewViewModel Preview { get; private set; }
        public SettingsViewModel Settings { get; private set; }

        public Command NewMapCommand { get; private set; }
        public Command GenerateCommand { get; private set; }
        public Command SaveCommand { get; private set; }
        public Command CancelCommand { get; private set; }

        private XDocument log;

        private bool showSaveInfo;
        public bool ShowSaveInfo
        {
            get { return this.showSaveInfo; }

            set
            {
                if (value != this.showSaveInfo)
                {
                    showSaveInfo = value;
                    this.OnPropertyChanged(() => this.ShowSaveInfo);
                }
            }
        }

        private string saveInfo;
        public string SaveInfo
        {
            get { return this.saveInfo; }

            set
            {
                if (value != this.saveInfo)
                {
                    saveInfo = value;
                    this.OnPropertyChanged(() => this.SaveInfo);
                }
            }
        }

        public GeneratorViewModel(MapGenerator generator, MapLayout[] layouts, Scenery[] sceneries, string logPath = null)
        {
            dispatcher = Dispatcher.CurrentDispatcher;

            LogPath = logPath;

            messages = new StringBuilder();

            Generator = generator;
            Generator.Log += (s, e) =>
            {
                messages.Append(e.Message);
                messages.Append("...");

                if (e.Message == "Generating rocks")
                    messages.Append(" (it does indeed ;)");

                messages.AppendLine();

                OnPropertyChanged("Messages");
            };

            Generator.PreviewUpdated += (s, e) =>
            {
                dispatcher.Invoke(new Action(() => Preview.UpdatePreview(Generator.Map, Generator.PreviewGenerator.BmpPreview)));
            };
            
            Preview = new PreviewViewModel();
            Settings = new SettingsViewModel(layouts, sceneries, Preview);

            if (File.Exists(LogPath))
            {
                log = XDocument.Load(LogPath);
                try
                {
                    var lastMap = log.Root.Descendants("Map").Last();
                    if (lastMap != null)
                    {
                        Generator.MapId = int.Parse(lastMap.Attribute("Id").Value);
                        Settings.Width = int.Parse(lastMap.Attribute("Width").Value);
                        Settings.Height = int.Parse(lastMap.Attribute("Height").Value);
                        Settings.MapLayout = Settings.MapLayouts.First(l => l.Name == lastMap.Attribute("Layout").Value);
                        Settings.Scenery = Settings.Sceneries.First(t => t.Name == lastMap.Attribute("Scenery").Value);
                        Settings.CliffFrequency = Settings.Frequencies.First(f => f.ToString() == lastMap.Attribute("Mountains").Value);
                        Settings.TreeFrequency = Settings.Frequencies.First(f => f.ToString() == lastMap.Attribute("Trees").Value);
                        Settings.TimeOfDay = Settings.TimesOfDay.First(t => t.ToString() == lastMap.Attribute("TimeOfDay").Value);
                        Settings.IsModified = false;
                    }
                }
                catch (Exception)
                {
                }
            }
            else
            {
                log = new XDocument(new XElement("Log"));
            }

            Settings.MapId = Generator.MapId;

            NewMapCommand = new Command(NewMap, CanGenerateNew);
            GenerateCommand = new Command(Generate, CanGenerate);
            SaveCommand = new Command(Save, CanSave);
            CancelCommand = new Command(Cancel, CanCancel);
        }

        private bool CanGenerateNew(object parameter)
        {
            return !Generator.IsGenerating && Settings.IsValid;
        }

        private bool CanGenerate(object parameter)
        {
            return !Generator.IsGenerating && Settings.IsModified && Settings.IsValid;
        }

        private bool CanCancel(object parameter)
        {
            return Generator.IsGenerating && !cancellation.IsCancellationRequested;
        }

        private bool CanSave(object parameter)
        {
            return !string.IsNullOrEmpty(OutputPath) && !Generator.IsGenerating && !cancellation.IsCancellationRequested;
        }

        private void NewMap(object parameter)
        {
            Settings.MapId = Generator.NewMap();
            GenerateMap();
        }

        private void Generate(object parameter)
        {
            Generator.MapId = Settings.MapId;
            GenerateMap();
        }

        private void Save(object parameter)
        {
            string name = string.Format(MapNameTemplate.Replace("#", "{0}"), Generator.MapId);
            
            Generator.SaveMap(name, OutputPath);
            SaveInfo = Path.Combine(OutputPath, name);
            ShowSaveInfo = true;
        }

        private void GenerateMap()
        {
            ShowSaveInfo = false;
            this.Settings.IsModified = false;

            messages.Clear();
            OnPropertyChanged("Messages");

            log.Root.Add(
                new XElement("Map",
                    new XAttribute("Date", DateTime.Now.ToString()),
                    new XAttribute("Id", Settings.MapId),
                    new XAttribute("Width", Settings.Width),
                    new XAttribute("Height", Settings.Height),
                    new XAttribute("Layout", Settings.MapLayout.Name),
                    new XAttribute("Scenery", Settings.Scenery.Name),
                    new XAttribute("Mountains", Settings.CliffFrequency),
                    new XAttribute("Trees", Settings.TreeFrequency),
                    new XAttribute("TimeOfDay", Settings.TimeOfDay)));

            var watch = new Stopwatch();
            watch.Start();

            if (Generating != null)
                Generating(this, EventArgs.Empty);

            if (cancellation != null)
            {
                cancellation.Dispose();
            }
            cancellation = new CancellationTokenSource();

            Task.Factory.StartNew(() => 
                {
                    Generator.Settings = new MapSettings(Settings.MapLayout, Settings.Scenery, Settings.CliffFrequency, Settings.TreeFrequency, Settings.TimeOfDay);
                    Generator.GenerateMap(Settings.TotalWidth, Settings.TotalHeight, Settings.Border, Settings.NumberOfPlayers, cancellation.Token);
                },
                cancellation.Token)
                
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        watch.Stop();
                        var exception = t.Exception.InnerExceptions.First();
                        log.Root.Add(CreateExceptionElement(exception));

                        messages.AppendLine();
                        messages.AppendLine("An error occured:");
                        messages.AppendLine(exception.ToString());
                        OnPropertyChanged("Messages");
                    }
                    else
                    {
                        watch.Stop();

                        string message = cancellation.IsCancellationRequested ?
                            "Map generation cancelled" :
                            string.Format("Map generated in {0} ms!", watch.ElapsedMilliseconds);

                        messages.AppendLine();
                        messages.AppendLine(message);
                        OnPropertyChanged("Messages");
                    }

                    dispatcher.Invoke(new Action(() =>
                        {
                            if (Generated != null)
                                Generated(this, EventArgs.Empty);

                            log.Save(LogPath);
                        }));
                });
            
        }

        private void Cancel(object parameter)
        {
            if (cancellation != null)
            {
                cancellation.Cancel();
            }
        }

        private XElement CreateExceptionElement(Exception exception)
        {
            XElement element = new XElement(exception.GetType().ToString());

            if (exception.Message != null)
            {
                element.Add(new XElement("Message", exception.Message));
            }

            if (exception.Data != null && exception.Data.Count > 0)
            {
                element.Add(new XElement("Data",
                    exception.Data.OfType<DictionaryEntry>()
                                  .Select(entry => new XElement(entry.Key.ToString(), entry.Value == null ? null : entry.Value.ToString()))
                ));
            }

            if (exception.InnerException != null)
            {
                element.Add(CreateExceptionElement(exception.InnerException));
            }

            return element;
        }
    }
}
