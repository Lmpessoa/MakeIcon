/*
 * Copyright (c) 2021 Leonardo Pessoa
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
