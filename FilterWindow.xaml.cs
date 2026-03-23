/*
 * SPDX-FileCopyrightText: 2026 Jackie <jackie.github@outlook.com>
 * SPDX-License-Identifier: GPL-3.0
 */

using System.Windows;

namespace PicViewer
{
    public partial class FilterWindow : Window
    {
        public FilterWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            ChkJpg.IsChecked = AppSettings.ShowJpg;
            ChkPng.IsChecked = AppSettings.ShowPng;
            ChkBmp.IsChecked = AppSettings.ShowBmp;
            ChkGif.IsChecked = AppSettings.ShowGif;
            ChkTiff.IsChecked = AppSettings.ShowTiff;
            ChkIco.IsChecked = AppSettings.ShowIco;
            ChkSvg.IsChecked = AppSettings.ShowSvg;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            // Write the selections on the screen back to global settings
            AppSettings.ShowJpg = ChkJpg.IsChecked ?? false;
            AppSettings.ShowPng = ChkPng.IsChecked ?? false;
            AppSettings.ShowBmp = ChkBmp.IsChecked ?? false;
            AppSettings.ShowGif = ChkGif.IsChecked ?? false;
            AppSettings.ShowTiff = ChkTiff.IsChecked ?? false;
            AppSettings.ShowIco = ChkIco.IsChecked ?? false;
            AppSettings.ShowSvg = ChkSvg.IsChecked ?? false;

            AppSettings.Save();

            this.DialogResult = true; // Return success
            this.Close();
        }
    }
}
