#region License

// OptionsViewModel.cs
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

using System.IO;
using System.Xml.Linq;
namespace Genesis.UI.ViewModel
{               
    class OptionsViewModel : ValidatingViewModel
    {
        private static char[] invalidCharacters = Path.GetInvalidPathChars();

        private string mapNameTemplate;

        public string MapNameTemplate
        {
            get { return this.mapNameTemplate; }

            set
            {
                if (value != this.mapNameTemplate)
                {
                    mapNameTemplate = value;

                    if (mapNameTemplate.IndexOfAny(invalidCharacters) >= 0)
                    {
                        AddError(() => MapNameTemplate, "The name can only contain valid path characters.");
                    }
                    else
                    {
                        ClearError(() => MapNameTemplate);
                    }

                    this.OnPropertyChanged(() => MapNameTemplate);
                }
            }
        }

        private string outputDirectory;

        public string OutputDirectory
        {
            get { return this.outputDirectory; }

            set
            {
                if (value != this.outputDirectory)
                {
                    outputDirectory = value;

                    if (outputDirectory.IndexOfAny(invalidCharacters) >= 0)
                    {
                        AddError(() => OutputDirectory, "The directory can only contain valid path characters.");
                    }
                    else
                    {
                        ClearError(() => OutputDirectory);
                    }

                    this.OnPropertyChanged(() => OutputDirectory);
                }
            }
        }

        private bool addObjectsToPreview;

        public bool AddObjectsToPreview
        {
            get { return this.addObjectsToPreview; }

            set
            {
                if (value != this.addObjectsToPreview)
                {
                    addObjectsToPreview = value;
                    this.OnPropertyChanged(() => AddObjectsToPreview);
                }
            }
        }

        public OptionsViewModel()
        {
            MapNameTemplate = "#";
            AddObjectsToPreview = true;
        }

        public void ReadXml(Stream stream)
        {
            var document = XDocument.Load(stream);

            MapNameTemplate = GetValue(document.Root, GetPropertyName(() => MapNameTemplate));
            OutputDirectory = GetValue(document.Root, GetPropertyName(() => OutputDirectory));

            bool.TryParse(GetValue(document.Root, GetPropertyName(() => AddObjectsToPreview)), out addObjectsToPreview);
        }

        private string GetValue(XElement parent, string elementName)
        {
            var element = parent.Element(elementName);
            if (element == null)
                return null;
            else
                return element.Value;
        }

        public void SaveXml(Stream stream)
        {
            new XDocument(
                new XElement("Options",
                    new XElement(GetPropertyName(() => MapNameTemplate), MapNameTemplate),
                    new XElement(GetPropertyName(() => AddObjectsToPreview), AddObjectsToPreview),
                    string.IsNullOrEmpty(OutputDirectory) ? null: new XElement(GetPropertyName(() => OutputDirectory), OutputDirectory))).Save(stream);
        }
    }
}
