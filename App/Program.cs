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
using System.Drawing;
using System.Windows.Forms;

namespace Lmpessoa.MakeIcon {

   static class Program {

      [STAThread]
      static void Main() {
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);

         OpenFileDialog open = new OpenFileDialog {
            Filter = "PNG Image|*.png",
            Multiselect = true,
            Title = "Select images to compose the icon"
         };
         do {
            try {
               if (open.ShowDialog() != DialogResult.OK) {
                  return;
               }
               Icon ico = new Icon();
               foreach (string filename in open.FileNames) {
                  ico.Add(new Bitmap(filename));
               }
               SaveFileDialog save = new SaveFileDialog {
                  Filter = "Icon file|*.ico",
                  Title = "Choose where to save the icon"
               };
               if (save.ShowDialog() != DialogResult.OK) {
                  return;
               }
               ico.SaveTo(save.FileName);
            } catch (Exception e) {
               MessageBox.Show(e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
         } while (true);
      }
   }
}
