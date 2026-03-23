/*
 * SPDX-FileCopyrightText: 2026 Jackie <jackie.github@outlook.com>
 * SPDX-License-Identifier: GPL-3.0
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace PicViewer
{
    public class LanguageInfo
    {
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
    }

    public static class LanguageManager
    {
        private static readonly string LanguagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages");

        // Dictionary containing all default English strings
        private static readonly Dictionary<string, string> DefaultEnglishStrings = new Dictionary<string, string>
        {
            { "LanguageName", "English" },
            { "Menu_File", "File(_F)" },
            { "Menu_Exit", "Exit(_X)" },
            { "Menu_View", "View(_V)" },
            { "Menu_Language", "Language(_L)" },
            { "Menu_FilterSettings", "File Format Filter Settings..." },
            { "Menu_ActualSize", "Actual Size(_1)" },
            { "Menu_ZoomIn", "Zoom In(_+)" },
            { "Menu_ZoomOut", "Zoom Out(_-)" },
            { "Menu_Help", "Help(_H)" },
            { "Menu_About", "About(_A)" },
            { "StatusBar_ZoomRatio", "Zoom Ratio: " },
            { "Tree_QuickAccess", "Quick Access" },
            { "Tree_Desktop", "Desktop" },
            { "Tree_Downloads", "Downloads" },
            { "Tree_Pictures", "Pictures" },
            { "Tree_Documents", "Documents" },
            { "Tree_Videos", "Videos" },
            { "Tree_LocalDisk", "Local Disk" },
            { "Msg_CannotLoadImage", "Cannot load image: " },
            { "Msg_Error", "Error" },
            { "Title_FilterWindow", "Supported File Format Filter" },
            { "Filter_Prompt", "Please select formats to display in folder:" },
            { "Btn_AssociateFiles", "Associate Files" },
            { "Btn_RemoveAssociation", "Remove Assoc." },
            { "Msg_AssocSuccess", "File associations set successfully!" },
            { "Msg_RemoveAssocSuccess", "File associations removed successfully!" },
            { "Btn_ApplyAndSave", "Apply and Save" },
            { "Title_AboutWindow", "About PicViewer" },
            { "About_Version", "Version: " },
            { "ToolTip_DoubleClickHash", "Double click to show Commit Hash" },
            { "Btn_Ok", "OK" }
        };

        public static void InitializeDefaultLanguage()
        {
            // Apply default English strings first
            foreach (var kvp in DefaultEnglishStrings)
            {
                Application.Current.Resources[kvp.Key] = kvp.Value;
            }

            // Ensure Languages directory exists
            if (!Directory.Exists(LanguagesDir))
            {
                try
                {
                    Directory.CreateDirectory(LanguagesDir);
                }
                catch { return; }
            }

            // Create default en_US.ini if not exists
            string defaultIniPath = Path.Combine(LanguagesDir, "en_US.ini");
            if (!File.Exists(defaultIniPath))
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(defaultIniPath, false, System.Text.Encoding.UTF8))
                    {
                        sw.WriteLine("; PicViewer English Language File");
                        sw.WriteLine("; File format: ISO639-1_ISO3166.ini");
                        foreach (var kvp in DefaultEnglishStrings)
                        {
                            sw.WriteLine($"{kvp.Key}={kvp.Value}");
                        }
                    }
                }
                catch { }
            }
        }

        public static List<LanguageInfo> GetAvailableLanguages()
        {
            List<LanguageInfo> list = new List<LanguageInfo>();
            if (!Directory.Exists(LanguagesDir)) return list;

            Regex regex = new Regex(@"^[a-z]{2}_[A-Z]{2}\.ini$");

            string[] files = Directory.GetFiles(LanguagesDir, "*.ini");
            foreach (string file in files)
            {
                string fileName = Path.GetFileName(file);
                if (regex.IsMatch(fileName))
                {
                    string displayName = fileName;
                    // Try to read LanguageName from file
                    try
                    {
                        string[] lines = File.ReadAllLines(file);
                        foreach (string line in lines)
                        {
                            if (line.StartsWith("LanguageName="))
                            {
                                displayName = line.Substring("LanguageName=".Length).Trim();
                                break;
                            }
                        }
                    }
                    catch { }

                    list.Add(new LanguageInfo
                    {
                        FilePath = file,
                        FileName = fileName,
                        DisplayName = displayName
                    });
                }
            }

            return list;
        }

        public static void ApplyLanguage(string filePath)
        {
            if (!File.Exists(filePath)) return;

            try
            {
                // Reset to default first to ensure missing keys fallback to English
                foreach (var kvp in DefaultEnglishStrings)
                {
                    Application.Current.Resources[kvp.Key] = kvp.Value;
                }

                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith(";") || trimmed.StartsWith("#"))
                        continue;

                    int equalsIndex = trimmed.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = trimmed.Substring(0, equalsIndex).Trim();
                        string value = trimmed.Substring(equalsIndex + 1).Trim();
                        Application.Current.Resources[key] = value;
                    }
                }
            }
            catch { }
        }
    }
}
