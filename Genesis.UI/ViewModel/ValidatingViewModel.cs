#region License

// ValidatingViewModel.cs
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
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Genesis.UI.ViewModel
{
    public abstract class ValidatingViewModel : ViewModel, IDataErrorInfo
    {
        private IDictionary<string, string> errors = new Dictionary<string, string>();

        protected void AddError<TProperty>(Expression<Func<TProperty>> property, string error)
        {
            errors[GetPropertyName(property)] = error;
            OnPropertyChanged(() => IsValid);
        }

        protected void ClearError<TProperty>(Expression<Func<TProperty>> property)
        {
            errors.Remove(GetPropertyName(property));
            OnPropertyChanged(() => IsValid);
        }

        public string Error
        {
            get 
            {
                // KeyValuePair is a struct, so the "Default" branch will return a KeyValuePair with a null value.
                return errors.FirstOrDefault().Value;
            }
        }

        public string this[string propertyName]
        {
            get
            {
                string error;
                errors.TryGetValue(propertyName, out error);
                return error;
            }
        }

        public bool IsValid
        {
            get { return !errors.Any(); }
        }
    }
}
