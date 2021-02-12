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
using System.Windows;

using Microsoft.Win32;

namespace Lmpessoa.MakeIcon {

   internal static class Theme {

      private static readonly RegistryKey registry = Registry.CurrentUser.CreateSubKey(@"Software").CreateSubKey("Lmpessoa").CreateSubKey("MakeIcon", writable: true);
      private static bool isDark = true;

      internal static void Restore() {
         isDark = (registry.GetValue("Theme")?.ToString() ?? "Dark") != "Dark";
         Switch();
      }

      internal static void Switch() {
         isDark = !isDark;
         Application.Current.Resources.MergedDictionaries.Clear();
         Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() {
            Source = new Uri($"Resources/Themes/{(isDark ? "Dark" : "Light")}.xaml", UriKind.Relative),
         });
         registry.SetValue("Theme", isDark ? "Dark" : "Light");
      }
   }
}