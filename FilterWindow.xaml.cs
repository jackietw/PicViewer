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
            ChkHeic.IsChecked = AppSettings.ShowHeic;
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
            AppSettings.ShowHeic = ChkHeic.IsChecked ?? false;

            AppSettings.Save();

            this.DialogResult = true; // Return success
            this.Close();
        }

        [System.Runtime.InteropServices.DllImport("shell32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        private static extern void SHChangeNotify(uint wEventId, uint uFlags, System.IntPtr dwItem1, System.IntPtr dwItem2);

        private void BtnAssociate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                if (string.IsNullOrEmpty(exePath)) return;

                string progId = "PicViewer.Image";

                // Register ProgID
                using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{progId}"))
                {
                    key.SetValue("", "Image File");
                    using (var iconKey = key.CreateSubKey("DefaultIcon"))
                        iconKey.SetValue("", $"\"{exePath}\",0");
                    using (var cmdKey = key.CreateSubKey(@"shell\open\command"))
                        cmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
                }

                // Register extensions
                string[] exts = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".ico", ".svg", ".heic" };
                foreach (var ext in exts)
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ext}"))
                    {
                        key.SetValue("", progId);
                    }

                    // Add app to the 'Open With' dialog in modern Windows (Since Windows 10 ignores direct HKCU/.jpg defaults)
                    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\Classes\{ext}\OpenWithProgids"))
                    {
                        key.SetValue(progId, string.Empty);
                    }
                }

                // Notify Explorer (SHCNE_ASSOCCHANGED = 0x08000000)
                SHChangeNotify(0x08000000, 0x0000, System.IntPtr.Zero, System.IntPtr.Zero);

                string successMsg = (string)Application.Current.Resources["Msg_AssocSuccess"] ?? "File associations set successfully!";
                successMsg += "\n\n[Windows 10/11 Note]: Microsoft protects default apps.\nPlease right-click any image file -> 'Open with' -> 'Choose another app', select PicViewer, and check 'Always use this app'.";
                MessageBox.Show(successMsg, "File Association", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to set file associations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnRemoveAssociate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string progId = "PicViewer.Image";
                string[] exts = { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tif", ".tiff", ".ico", ".svg", ".heic" };

                // Remove extensions
                foreach (var ext in exts)
                {
                    try { Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{ext}\OpenWithProgids", false); } catch { }
                    try
                    {
                        using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\Classes\{ext}", true))
                        {
                            if (key != null && key.GetValue("") as string == progId)
                            {
                                key.DeleteValue("", false);
                            }
                        }
                    } 
                    catch { }
                }

                // Remove ProgID
                try { Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree($@"Software\Classes\{progId}", false); } catch { }

                SHChangeNotify(0x08000000, 0x0000, System.IntPtr.Zero, System.IntPtr.Zero);

                MessageBox.Show((string)Application.Current.Resources["Msg_RemoveAssocSuccess"] ?? "File associations removed successfully!", 
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Failed to remove file associations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
