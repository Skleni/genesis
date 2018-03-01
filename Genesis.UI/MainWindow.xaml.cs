#region License

// MainWindow.xaml.cs
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using Genesis;
using Genesis.Settings;
using Genesis.UI;
using Genesis.UI.ViewModel;
using Genesis.Xaml;
using Microsoft.Win32;

namespace CkMp.Generator.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string logFile = "log.xml";

        private GeneratorViewModel generator;
        private OptionsViewModel options;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;

            foreach (var dll in Directory.GetFiles("Processors", "*.dll"))
            {
                try 
                {
                    Assembly.LoadFrom(dll);
                }
                catch { }
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var serializer = new XmlSerializer(typeof(MapLayout[]), new XmlRootAttribute("MapLayouts"));
            var layouts = serializer.Deserialize(File.OpenRead("MapLayouts.xml")) as MapLayout[];

            serializer = new XmlSerializer(typeof(Scenery));
            var sceneries = Directory.GetFiles("Sceneries", "*.xml").Select(f => 
                {
                    try
                    {
                        return (Scenery)serializer.Deserialize(File.OpenRead(f));
                    }
                    catch
                    {
                        return default(Scenery);
                    }
                });

            var processors = MapProcessors.FromXaml(File.OpenRead("Genesis.xaml"));

            generator = new GeneratorViewModel(new MapGenerator(processors), layouts, sceneries.ToArray(), logFile);

            options = new OptionsViewModel();
            try
            {
                using (var stream = File.OpenRead("Options.xml"))
                {
                    options.ReadXml(stream);
                }
            }
            catch { }

            generator.MapNameTemplate = options.MapNameTemplate;
            generator.Generator.PreviewGenerator.AddObjects = options.AddObjectsToPreview;
            generator.Generating += (s, a) => content.Cursor = Cursors.Wait;
            generator.Generated += (s, a) =>
            {
                content.Cursor = Cursors.Arrow;
                log.Focus();
                CommandManager.InvalidateRequerySuggested();
            };

            DataContext = generator;

            // We need to wait until the window is fully initialized before we can show a MessageBox.
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (string.IsNullOrEmpty(options.OutputDirectory))
                {
                    if (ZeroHour.MapsDirectory == null)
                    {
                        MessageBox.Show("The location of your Zero Hour data folder could not be determined.\nYou need to set the path manually in the options to be able to save maps.", "Zero Hour Data Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                        EditOptions();
                    }
                    else
                    {
                        generator.OutputPath = ZeroHour.MapsDirectory;
                    }
                }
                else
                {
                    generator.OutputPath = options.OutputDirectory;
                }

                generator.GenerateCommand.Execute(null);
            }));
        }

        private void ScrollToEnd(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            log.ScrollToEnd();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void EditOptions(object sender, RoutedEventArgs e)
        {
            EditOptions();
        }

        private void EditOptions()
        {
            string outputDirectory = options.OutputDirectory;

            var result = new Options() { Owner = this, DataContext = options }.ShowDialog();
            try
            {
                if (result == true)
                {
                    generator.MapNameTemplate = options.MapNameTemplate;
                    generator.OutputPath = string.IsNullOrEmpty(options.OutputDirectory) ? ZeroHour.MapsDirectory : options.OutputDirectory;
                    generator.Generator.PreviewGenerator.AddObjects = options.AddObjectsToPreview;
                    using (var stream = File.Open("Options.xml", FileMode.Create))
                    {
                        options.SaveXml(stream); 
                    }

                    generator.Settings.IsModified = true;
                }
                else
                {
                    options.MapNameTemplate = generator.MapNameTemplate;
                    options.OutputDirectory = outputDirectory;
                }
            }
            catch { }
        }

        private void ShowAboutBox(object sender, RoutedEventArgs e)
        {
            new About() { Owner = this }.ShowDialog();
        }

        private void OpenPath(object sender, RoutedEventArgs e)
        {
            Process.Start(path.Text);
        }
    }
}
