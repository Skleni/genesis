#region License

// Grid.cs
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

namespace CkMp.Data.Map
{
    public class Grid<T>
    {
        protected T[] data;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Count { get { return Width * Height; } }

        public T this[int x, int y]
        {
            get { return data[Index(x, y)]; }
            set { data[Index(x, y)] = value; }
        }

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            data = new T[width * height];
        }

        public int Index(int x, int y)
        {
            return y * Width + x;
        }

        public T[] ToArray()
        {
            return data;
        }

        public void WrapCoordinates(ref int x, ref int y)
        {
            x = (x + Width) % Width;
            y = (y + Height) % Height;
        }

        public bool CheckCoordinates(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
