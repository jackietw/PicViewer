/*
 * SPDX-FileCopyrightText: 2026 Jackie <jackie.github@outlook.com>
 * SPDX-License-Identifier: GPL-3.0
 */

using System;
using System.IO;

namespace PicViewer
{
    // Global static settings to keep track of current format filters and preferences
    public static class AppSettings
    {
        public static bool ShowJpg = true;
        public static bool ShowPng = true;
        public static bool ShowBmp = true;
        public static bool ShowGif = true;
        public static bool ShowTiff = true;
        public static bool ShowIco = true;
        public static bool ShowSvg = true; // Enabled by default
        
        public static string LanguageFile = "en_US.ini";

        private static readonly string SettingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.ini");

        public static void Load()
        {
            if (!File.Exists(SettingsFilePath)) return;

            try
            {
                string[] lines = File.ReadAllLines(SettingsFilePath);
                foreach (string line in lines)
                {
                    string l = line.Trim();
                    if (string.IsNullOrEmpty(l) || l.StartsWith(";") || l.StartsWith("#")) continue;

                    string[] parts = l.Split(new char[] { '=' }, 2);
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (key == "ShowJpg") ShowJpg = bool.Parse(value);
                        else if (key == "ShowPng") ShowPng = bool.Parse(value);
                        else if (key == "ShowBmp") ShowBmp = bool.Parse(value);
                        else if (key == "ShowGif") ShowGif = bool.Parse(value);
                        else if (key == "ShowTiff") ShowTiff = bool.Parse(value);
                        else if (key == "ShowIco") ShowIco = bool.Parse(value);
                        else if (key == "ShowSvg") ShowSvg = bool.Parse(value);
                        else if (key == "LanguageFile") LanguageFile = value;
                    }
                }
            }
            catch { }
        }

        public static void Save()
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(SettingsFilePath, false, System.Text.Encoding.UTF8))
                {
                    sw.WriteLine("; PicViewer Global Settings");
                    sw.WriteLine($"LanguageFile={LanguageFile}");
                    sw.WriteLine($"ShowJpg={ShowJpg}");
                    sw.WriteLine($"ShowPng={ShowPng}");
                    sw.WriteLine($"ShowBmp={ShowBmp}");
                    sw.WriteLine($"ShowGif={ShowGif}");
                    sw.WriteLine($"ShowTiff={ShowTiff}");
                    sw.WriteLine($"ShowIco={ShowIco}");
                    sw.WriteLine($"ShowSvg={ShowSvg}");
                }
            }
            catch { }
        }
    }
}
