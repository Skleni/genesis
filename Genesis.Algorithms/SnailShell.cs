#region License

// SnailShell.cs
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

namespace Genesis.Algorithms
{
    public class SnailShell<TCoordinates>
    {
        private Func<int, int, TCoordinates> getCoordinates;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnailShell&lt;TCoordinates&gt;"/> class.
        /// </summary>
        /// <param name="getCoordinates">A function converting an X and Y coordinate into the result type.</param>
        public SnailShell(Func<int, int, TCoordinates> getCoordinates)
        {
            this.getCoordinates = getCoordinates;
        }

        /// <summary>
        /// Traverses a 2D grid like a snail shell.
        /// </summary>
        /// <param name="x">The X coordinate of the start point.</param>
        /// <param name="y">The Y coordinate of the start point</param>
        /// <returns></returns>
        public IEnumerable<TCoordinates> GetCoordinates(int x, int y)
        {
            int distanceToGo = 1;
            bool down = true;

            yield return getCoordinates(x, y);
                
            while (true)
            {
                for (int i = 0; i < distanceToGo; i++)
                {
                    if (down)
                        y++;
                    else
                        y--;
                    
                    yield return getCoordinates(x, y);
                }
                for (int i = 0; i < distanceToGo; i++)
                {
                    if (down)
                        x++;
                    else
                        x--;

                    yield return getCoordinates(x, y);
                }

                distanceToGo++;
                down = !down;
            }
        }
    }
}
