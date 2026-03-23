/*
 * SPDX-FileCopyrightText: 2026 Jackie <jackie.github@outlook.com>
 * SPDX-License-Identifier: GPL-3.0
 */

using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace PicViewer
{
    public partial class AboutWindow : Window
    {
        private string _displayVersion = "unknown";
        private string _commitHash = "N/A";
        private bool _showingHash = false;

        public AboutWindow()
        {
            InitializeComponent();
            LoadVersionInfo();
        }

        private void LoadVersionInfo()
        {
            string? rawVersion = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            
            if (!string.IsNullOrEmpty(rawVersion) && rawVersion.Contains('+'))
            {
                string[] parts = rawVersion.Split('+');
                _displayVersion = parts[0];
                _commitHash = parts[1];
            }
            else
            {
                _displayVersion = rawVersion ?? "unknown version";
            }

            VersionTextBlock.Text = _displayVersion;
        }

        private void VersionTextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Determine if double-clicked
            if (e.ClickCount == 2)
            {
                _showingHash = !_showingHash;
                if (!_showingHash)
                {
                    //VersionText.Visibility = Visibility.Visible;
                    VersionText.Text = "Version: ";
                }
                else
                {
                    //VersionText.Visibility = Visibility.Hidden;
                    VersionText.Text = "Hash: ";
                }
                VersionTextBlock.Text = _showingHash ? _commitHash : _displayVersion;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
