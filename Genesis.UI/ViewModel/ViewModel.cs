#region License

// ViewModel.cs
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
using System.ComponentModel;
using System.Linq.Expressions;

namespace Genesis.UI.ViewModel
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected void OnPropertyChanged<TProperty>(Expression<Func<TProperty>> property)
        {
            OnPropertyChanged(GetPropertyName(property));
        }

        protected string GetPropertyName<TProperty>(Expression<Func<TProperty>> property)
        {
            var expression = property.Body as MemberExpression;
            if (expression == null)
            {
                throw new ArgumentException("expression");
            }

            return expression.Member.Name;
        }
    }
}
