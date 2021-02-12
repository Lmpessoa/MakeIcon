/*
 * Copyright (c) 2019 Leonardo Pessoa
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace Lmpessoa.MakeIcon {

   internal sealed class Icon {

      private static readonly int[] validSizes = new int[] { 16, 32, 48, 64, 128, 256 };
      private readonly Dictionary<int, Bitmap> images = new Dictionary<int, Bitmap>();

      internal void Add(Bitmap image) {
         if (image.Width != image.Height) {
            throw new BadImageFormatException("Icon images must have same width and height");
         }
         if (!validSizes.Contains(image.Width)) {
            throw new BadImageFormatException($"Image size is not supported in ICO format: {image.Width}x{image.Height}");
         }
         if (image.PixelFormat != PixelFormat.Format32bppArgb) {
            throw new BadImageFormatException($"Unsupported format for icon: {image.PixelFormat}");
         }
         images[image.Width] = image;
      }

      internal void Remove(int size) {
         if (images.ContainsKey(size)) {
            images.Remove(size);
         }
      }

      internal int Count => images.Count;

      internal Bitmap this[int size] => images[size];

      internal void SaveTo(string filename) {
         Bitmap[] images = GetSortedImages();
         byte[][] data = GetImageBytes(images);
         FileStream file = File.OpenWrite(filename);
         file.SetLength(0);
         using (BinaryWriter icon = new BinaryWriter(file)) {
            icon.Write((uint) 0x00010000);
            icon.Write((ushort) images.Length);
            int offset = (images.Length * 16) + 6;
            for (int i = 0; i < images.Length; ++i) {
               Bitmap image = images[i];
               icon.Write((byte) (image.Width == 256 ? 0 : image.Width));
               icon.Write((byte) (image.Height == 256 ? 0 : image.Height));
               icon.Write((ushort) 0);
               icon.Write((ushort) 1);
               icon.Write((ushort) 32);
               icon.Write(data[i].Length);
               icon.Write(offset);
               offset += data[i].Length;
            }
            for (int i = 0; i < data.Length; ++i) {
               icon.Write(data[i]);
            }
         }
      }

      private Bitmap[] GetSortedImages() {
         List<Bitmap> images = new List<Bitmap>(this.images.Values);
         images.Sort((b1, b2) => b2.Width - b1.Width);
         return images.ToArray();
      }

      private byte[][] GetImageBytes(Bitmap[] images) {
         byte[][] result = new byte[images.Length][];
         for (int i = 0; i < images.Length; ++i) {
            Bitmap image = images[i];
            using (MemoryStream stream = new MemoryStream()) {
               if (image.Width == 256) {
                  image.Save(stream, ImageFormat.Png);
               } else {
                  BinaryWriter ico = new BinaryWriter(stream);
                  ico.Write((uint) 0x28);
                  ico.Write((uint) image.Width);
                  ico.Write((uint) image.Height * 2);
                  ico.Write((ushort) 1);
                  ico.Write((ushort) 32);
                  ico.Write(new byte[24]);
                  for (int y = image.Height - 1; y >= 0; --y) {
                     for (int x = 0; x < image.Width; ++x) {
                        Color pixel = image.GetPixel(x, y);
                        if (pixel.ToArgb() == 0xFFFFFF) {
                           pixel = Color.FromArgb(0);
                        }
                        ico.Write(pixel.B);
                        ico.Write(pixel.G);
                        ico.Write(pixel.R);
                        ico.Write(pixel.A);
                     }
                  }
                  for (int y = image.Height - 1; y >= 0; --y) {
                     uint dta = 0, mask = 0x80000000;
                     for (int x = 0; x < image.Width; ++x) {
                        Color pixel = image.GetPixel(x, y);
                        if (pixel.A < 50) {
                           dta |= mask;
                        }
                        mask = mask >> 1;
                        if (mask == 0 || x == image.Width - 1) {
                           dta = (dta & 0xFF000000) >> 24 |
                                 (dta & 0x00FF0000) >> 8 |
                                 (dta & 0x0000FF00) << 8 |
                                 (dta & 0x000000FF) << 24;
                           ico.Write(dta);
                           mask = 0x80000000;
                           dta = 0;
                        }
                     }
                  }
               }
               result[i] = stream.ToArray();
            }
         }
         return result;
      }

      private byte BitsPerPixel(PixelFormat format) {
         return format switch {
            PixelFormat.Format1bppIndexed => 1,
            PixelFormat.Format4bppIndexed => 4,
            PixelFormat.Format8bppIndexed => 8,
            PixelFormat.Format24bppRgb => 24,
            PixelFormat.Format32bppArgb => 32,
            _ => 0,
         };
      }
   }
}
