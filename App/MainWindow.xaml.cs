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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;

using Microsoft.Win32;

namespace Lmpessoa.MakeIcon {

   internal partial class MainWindow : Window {

      public static readonly RoutedCommand SaveAsCommand = new RoutedCommand();
      public static readonly RoutedCommand SelectNextCommand = new RoutedCommand();
      public static readonly RoutedCommand ClearSelectionCommand = new RoutedCommand();
      public static readonly RoutedCommand RemoveImageCommand = new RoutedCommand();

      private static readonly Brush SELECTED = new SolidColorBrush(SystemColors.HighlightColor);

      private readonly Dictionary<int, Button> images;
      private readonly Icon icon = new Icon();

      private Button sel = null;

      public MainWindow() {
         InitializeComponent();
         WindowChrome.SetWindowChrome(this, new WindowChrome() {
            UseAeroCaptionButtons = false,
            CaptionHeight = 0,
         });
         Theme.Restore();
         images = new Dictionary<int, Button> {
            { 256, Icon256 }, { 128, Icon128 }, { 64, Icon64 }, { 48, Icon48 }, { 32, Icon32 }, { 16, Icon16 },
         };
      }

      private void Close_Click(object sender, RoutedEventArgs e)
         => Close();

      private void Minimize_Click(object sender, RoutedEventArgs e)
         => WindowState = WindowState == WindowState.Minimized ? WindowState.Normal : WindowState.Minimized;

      private void ChangeTheme_Click(object sender, RoutedEventArgs e)
         => Theme.Switch();

      private void TitlePanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
         if (e.ButtonState == MouseButtonState.Pressed) {
            DragMove();
            e.Handled = true;
         }
      }

      private void Help_MouseEnter(object sender, MouseEventArgs e)
         => HelpTip.Visibility = Visibility.Visible;

      private void Help_MouseLeave(object sender, MouseEventArgs e)
         => HelpTip.Visibility = Visibility.Hidden;

      private void ImageSelect(object sender, RoutedEventArgs e) {
         if (sender is Button btn) {
            if (sel != null) {
               if (sel.Content == null) {
                  sel.SetResourceReference(Button.BorderBrushProperty, "IconBorder");
               } else {
                  sel.BorderBrush = null;
               }
            }
            if (sel != btn) {
               sel = btn;
               sel.BorderBrush = SELECTED;
            } else {
               sel = null;
            }
            e.Handled = true;
         }
      }

      private void Label_Click(object sender, MouseButtonEventArgs e) {
         if (sender is Label label && label.Tag is Button btn) {
            ImageSelect(btn, e);
         }
      }

      private static string[] GetDropFiles(DragEventArgs e)
         => e.Data.GetData("FileDrop", true) is string[] args
            ? args.Where(s => s.ToLower().EndsWith(".png")).ToArray()
            : Array.Empty<string>();

      private void Window_DragOver(object sender, DragEventArgs e) {
         e.Effects = GetDropFiles(e).Length > 0 ? DragDropEffects.Copy : DragDropEffects.None;
         e.Handled = true;
      }

      private void Window_DropFiles(object sender, DragEventArgs e) {
         if (GetDropFiles(e) is string[] args) {
            System.Drawing.Bitmap png;
            foreach (string file in args) {
               try {
                  png = new System.Drawing.Bitmap(file);
                  icon.Add(png);
                  Button btn = images[png.Width];
                  btn.BorderBrush = null;
                  btn.Content = new Image() {
                     Source = new BitmapImage(new Uri(file)),
                  };
               } catch (Exception e1) {
                  MessageBox.Show(e1.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
               }
            }
            e.Handled = true;
         }
      }

      private void SelectNext(object sender, RoutedEventArgs e) {
         if (sel == null || sel == Icon16) {
            ImageSelect(Icon256, e);
         } else {
            List<int> iconSizes = images.Keys.ToList();
            int index = iconSizes.IndexOf((int) sel.Width);
            ImageSelect(images[iconSizes[index + 1]], e);
         }
      }

      private void ClearSelection(object sender, RoutedEventArgs e) {
         ImageSelect(sel, e);
         e.Handled = true;
      }

      private void RemoveSelection(object sender, RoutedEventArgs e) {
         if (sel != null) {
            icon.Remove((int) sel.Width);
            sel.Content = null;
            e.Handled = true;
         }
      }

      private void SaveAs_Click(object sender, RoutedEventArgs e) {
         if (icon.Count > 0) {
            SaveFileDialog save = new SaveFileDialog {
               Filter = "Icon file|*.ico",
               Title = "Choose where to save the icon"
            };
            if (save.ShowDialog() == true) {
               icon.SaveTo(save.FileName);
            }
         }
         e.Handled = true;
      }
   }
}
