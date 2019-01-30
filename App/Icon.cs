/*
 * Copyright (c) 2019 Leonardo Pessoa
 * https://lmpessoa.com
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Lmpessoa.MakeIcon {

   class Icon {

      private List<Bitmap> images = new List<Bitmap>();

      internal void Add(Bitmap image) {
         if (image.Width != image.Height) {
            throw new Exception("Icon images must have same width and height");
         }
         if (image.Width > 256) {
            throw new Exception($"Image size is not supported in ICO format: {image.Width}x{image.Height}");
         }
         if (image.PixelFormat != PixelFormat.Format32bppArgb) {
            throw new Exception($"Unsupported format for icon: {image.PixelFormat.ToString()}");
         }
         foreach (Bitmap bmp in images) {
            if (bmp.Width == image.Width) {
               throw new Exception($"This icon already has an image {image.Width}x{image.Height}");
            }
         }
         images.Add(image);
      }

      internal void SaveTo(string filename) {
         Bitmap[] images = GetSortedImages();
         byte[][] data = GetImageBytes(images);
         FileStream file = File.OpenWrite(filename);
         file.SetLength(0);
         using (BinaryWriter icon = new BinaryWriter(file)) {
            icon.Write((UInt32) 0x00010000);
            icon.Write((UInt16) images.Length);
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
         List<Bitmap> images = new List<Bitmap>(this.images);
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
         switch (format) {
            case PixelFormat.Format1bppIndexed:
               return 1;
            case PixelFormat.Format4bppIndexed:
               return 4;
            case PixelFormat.Format8bppIndexed:
               return 8;
            case PixelFormat.Format24bppRgb:
               return 24;
            case PixelFormat.Format32bppArgb:
               return 32;
         }
         return 0;
      }
   }
}
