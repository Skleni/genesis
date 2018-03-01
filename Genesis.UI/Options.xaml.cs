#region License

// Options.xaml.cs
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

using System.Windows;
using System.Windows.Forms;
using Genesis.UI;
using Genesis.UI.ViewModel;

namespace CkMp.Generator.UI
{
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();
        }

        private void Close(object sender, RoutedEventArgs e)
        {
            DialogResult = sender == ok;
            this.Close();
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            var viewModel = ((OptionsViewModel)DataContext);

            var dialog = new FolderBrowserDialog()
            {
                SelectedPath = string.IsNullOrEmpty(viewModel.OutputDirectory) ? ZeroHour.MapsDirectory : viewModel.OutputDirectory,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                viewModel.OutputDirectory = dialog.SelectedPath;
            }
        }
    }
}
