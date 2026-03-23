/*
 * SPDX-FileCopyrightText: 2026 Jackie <jackie.github@outlook.com>
 * SPDX-License-Identifier: GPL-3.0
 */

using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Reflection;

namespace PicViewer
{
    public partial class MainWindow : Window
    {
        // Data source for binding the image list in the bottom ListView
        public ObservableCollection<ImageItem> ImageItems { get; set; }

        private Point _panStartPoint;
        private Point _scrollStartOffset;
        private System.Windows.Threading.DispatcherTimer _minimapHideTimer;
        private bool _isFullScreen = false;
        
        // Save constraints before switching to full screen
        private GridLength _savedLeftColWidth;
        private GridLength _savedBottomRowHeight;

        public MainWindow()
        {
            InitializeComponent();
            ImageItems = new ObservableCollection<ImageItem>();
            ThumbnailListView.ItemsSource = ImageItems; // Set data source

            // Initialize Minimap hide timer
            _minimapHideTimer = new System.Windows.Threading.DispatcherTimer();
            _minimapHideTimer.Interval = TimeSpan.FromSeconds(1);
            _minimapHideTimer.Tick += MinimapHideTimer_Tick;
        }

        private void MinimapHideTimer_Tick(object? sender, EventArgs e)
        {
            _minimapHideTimer.Stop();
            // Fade out animation
            System.Windows.Media.Animation.DoubleAnimation fadeOut = new System.Windows.Media.Animation.DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            MinimapBorder.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLanguages();
            LoadDrives(); // Scan system drives after window load
        }

        private void LoadLanguages()
        {
            var langs = LanguageManager.GetAvailableLanguages();
            foreach (var lang in langs)
            {
                MenuItem item = new MenuItem();
                item.Header = lang.DisplayName;
                item.Tag = lang.FilePath;
                item.Click += LanguageMenuItem_Click;
                MenuLanguageRoot.Items.Add(item);
            }
        }

        private void LanguageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item && item.Tag is string filePath)
            {
                LanguageManager.ApplyLanguage(filePath);
                
                // Save selected language setting
                AppSettings.LanguageFile = System.IO.Path.GetFileName(filePath);
                AppSettings.Save();
                
                // Refresh drive names if tree is loaded
                FolderTreeView.Items.Clear();
                LoadDrives();
            }
        }

        // Listen to keyboard events
        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // Exit full screen mode when Escape key is pressed
            if (e.Key == System.Windows.Input.Key.Escape && _isFullScreen)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
        }

        // Load system drives (C:, D:, etc.) and quick access folders into the tree directory
        private void LoadDrives()
        {
            // 1. Create 'Quick Access' root node
            TreeViewItem quickAccessNode = new TreeViewItem();
            quickAccessNode.Header = "⭐ " + (string)Application.Current.Resources["Tree_QuickAccess"];
            quickAccessNode.IsExpanded = true; // Expanded by default

            var specialFolders = new System.Collections.Generic.Dictionary<string, Environment.SpecialFolder>
            {
                { "Tree_Desktop", Environment.SpecialFolder.Desktop },
                { "Tree_Downloads", Environment.SpecialFolder.UserProfile }, // Handle Downloads specially
                { "Tree_Pictures", Environment.SpecialFolder.MyPictures },
                { "Tree_Documents", Environment.SpecialFolder.MyDocuments },
                { "Tree_Videos", Environment.SpecialFolder.MyVideos }
            };

            foreach (var kvp in specialFolders)
            {
                string path = (kvp.Key == "Tree_Downloads") 
                    ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
                    : Environment.GetFolderPath(kvp.Value);

                if (Directory.Exists(path))
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = "📁 " + (string)Application.Current.Resources[kvp.Key];
                    item.Tag = path;
                    item.Items.Add(null); // Allow expansion
                    item.Expanded += Folder_Expanded;
                    quickAccessNode.Items.Add(item);
                }
            }
            FolderTreeView.Items.Add(quickAccessNode);

            // 2. Create 'Local Disk' root node
            TreeViewItem pcNode = new TreeViewItem();
            pcNode.Header = "🖥️ " + (string)Application.Current.Resources["Tree_LocalDisk"];
            pcNode.IsExpanded = true;

            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = $"💿 {drive.Name} ({drive.VolumeLabel})";
                    item.Tag = drive.RootDirectory.FullName;
                    item.Items.Add(null); // Add an empty child node so the UI shows the '+' sign
                    item.Expanded += Folder_Expanded; // Register expanded event
                    pcNode.Items.Add(item);
                }
            }
            FolderTreeView.Items.Add(pcNode);
        }

        // Dynamically load the next level folders when the user clicks the '+' sign to expand (Lazy Loading)
        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is not TreeViewItem item) return;
            
            // If there is only one empty node (the null we just added), it means it hasn't been loaded yet
            if (item.Items.Count == 1 && item.Items[0] == null)
            {
                item.Items.Clear();
                try
                {
                    string? path = item.Tag?.ToString();
                    if (string.IsNullOrEmpty(path)) return;
                    foreach (string dir in Directory.GetDirectories(path))
                    {
                        DirectoryInfo dirInfo = new DirectoryInfo(dir);
                        
                        // Ignore hidden or system files
                        if ((dirInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden ||
                            (dirInfo.Attributes & FileAttributes.System) == FileAttributes.System)
                        {
                            continue;
                        }

                        TreeViewItem subItem = new TreeViewItem();
                        subItem.Header = "📁 " + dirInfo.Name;
                        subItem.Tag = dir;
                        subItem.Items.Add(null); // Add an empty node for the next level expansion
                        subItem.Expanded += Folder_Expanded;
                        item.Items.Add(subItem);
                    }
                }
                catch (UnauthorizedAccessException) { /* Ignore folders without read permission */ }
                catch (Exception) { }
            }
        }

        // When a folder is clicked and selected
        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem? selectedItem = FolderTreeView.SelectedItem as TreeViewItem;
            if (selectedItem?.Tag is string tagPath)
            {
                LoadImagesFromFolder(tagPath); // Load images from this folder
            }
        }

        // Read all images in the folder into the thumbnail list
        private void LoadImagesFromFolder(string folderPath)
        {
            ImageItems.Clear();
            try
            {
                var files = Directory.GetFiles(folderPath);
                foreach (string file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();
                    bool allow = false;

                    // Determine whether to load based on the checked status in current settings
                    if ((ext == ".jpg" || ext == ".jpeg") && AppSettings.ShowJpg) allow = true;
                    else if (ext == ".png" && AppSettings.ShowPng) allow = true;
                    else if (ext == ".bmp" && AppSettings.ShowBmp) allow = true;
                    else if (ext == ".gif" && AppSettings.ShowGif) allow = true;
                    else if ((ext == ".tif" || ext == ".tiff") && AppSettings.ShowTiff) allow = true;
                    else if (ext == ".ico" && AppSettings.ShowIco) allow = true;
                    else if (ext == ".svg" && AppSettings.ShowSvg) allow = true;

                    if (allow)
                    {
                        System.Windows.Media.ImageSource? thumb = null;
                        try 
                        {
                            if (ext == ".svg")
                            {
                                var svgDoc = Svg.SvgDocument.Open(file);
                                svgDoc.Width = 150; // Force small width/height to quickly generate thumbnails
                                svgDoc.Height = 150; 
                                using (var bitmap = svgDoc.Draw())
                                {
                                    using (var ms = new System.IO.MemoryStream())
                                    {
                                        bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                        ms.Position = 0;
                                        var bi = new BitmapImage();
                                        bi.BeginInit();
                                        bi.CacheOption = BitmapCacheOption.OnLoad;
                                        bi.StreamSource = ms;
                                        bi.EndInit();
                                        bi.Freeze(); // Freeze the object to improve performance and allow cross-thread access
                                        thumb = bi;
                                    }
                                }
                            }
                            else
                            {
                                BitmapImage bi = new BitmapImage();
                                bi.BeginInit();
                                bi.DecodePixelWidth = 150; // Limit decoding size to speed up and save memory
                                bi.UriSource = new Uri(file);
                                bi.CacheOption = BitmapCacheOption.OnLoad;
                                bi.EndInit();
                                bi.Freeze();
                                thumb = bi;
                            }
                        } 
                        catch { } // Ignore the thumbnail if specific image is corrupted, but keep the file item

                        ImageItems.Add(new ImageItem 
                        { 
                            ImagePath = file, 
                            FileName = Path.GetFileName(file),
                            Thumbnail = thumb
                        });
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (Exception) { }
        }

        // When a thumbnail is selected from the bottom list, display it in the central large image area
        private void ThumbnailListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThumbnailListView.SelectedItem is ImageItem selectedImage)
            {
                try
                {
                    // Reset zoom ratio when switching images
                    if (MainImageScale != null)
                    {
                        MainImageScale.ScaleX = 1;
                        MainImageScale.ScaleY = 1;
                    }

                    string ext = Path.GetExtension(selectedImage.ImagePath).ToLower();
                    System.Windows.Media.ImageSource? fullImage = null;

                    if (ext == ".svg")
                    {
                        var svgDoc = Svg.SvgDocument.Open(selectedImage.ImagePath);
                        // If there is an original dimension, set magnification factor to output high-res bitmap for large screen viewing
                        if (svgDoc.Width.Value > 0 && svgDoc.Height.Value > 0)
                        {
                            // Set width around 2000px for HD details
                            float scale = 2000f / svgDoc.Width.Value;
                            if (scale > 1)
                            {
                                svgDoc.Width = new Svg.SvgUnit(svgDoc.Width.Type, svgDoc.Width.Value * scale);
                                svgDoc.Height = new Svg.SvgUnit(svgDoc.Height.Type, svgDoc.Height.Value * scale);
                            }
                        }

                        using (var bitmap = svgDoc.Draw())
                        {
                            using (var ms = new System.IO.MemoryStream())
                            {
                                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                ms.Position = 0;
                                var bi = new BitmapImage();
                                bi.BeginInit();
                                bi.CacheOption = BitmapCacheOption.OnLoad;
                                bi.StreamSource = ms;
                                bi.EndInit();
                                fullImage = bi;
                            }
                        }
                    }
                    else
                    {
                        BitmapImage bi = new BitmapImage();
                        bi.BeginInit();
                        bi.UriSource = new Uri(selectedImage.ImagePath);
                        bi.CacheOption = BitmapCacheOption.OnLoad; // Ensure image is fully loaded into memory without locking the file
                        bi.EndInit();
                        fullImage = bi;
                    }

                    MainImageView.Source = fullImage;
                    MinimapImage.Source = fullImage; // Synchronously update the minimap thumbnail
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{Application.Current.Resources["Msg_CannotLoadImage"]}{ex.Message}", 
                        (string)Application.Current.Resources["Msg_Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Listen to mouse wheel events: Ctrl+wheel to zoom, normal wheel to switch previous/next image
        private void MainImageScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (ImageItems == null || ImageItems.Count == 0) return;

            // If Ctrl key is pressed and not currently in full screen mode, enter zoom mode
            if (!_isFullScreen && System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                double zoomStep = 1.2;
                if (e.Delta > 0)
                {
                    MainImageScale.ScaleX *= zoomStep;
                    MainImageScale.ScaleY *= zoomStep;
                }
                else if (e.Delta < 0)
                {
                    MainImageScale.ScaleX /= zoomStep;
                    MainImageScale.ScaleY /= zoomStep;
                }
                
                // Limit zoom ratio (0.1x to 10x)
                if (MainImageScale.ScaleX < 0.1) MainImageScale.ScaleX = 0.1;
                if (MainImageScale.ScaleY < 0.1) MainImageScale.ScaleY = 0.1;
                if (MainImageScale.ScaleX > 10) MainImageScale.ScaleX = 10;
                if (MainImageScale.ScaleY > 10) MainImageScale.ScaleY = 10;

                e.Handled = true;
                return;
            }

            // Ctrl is not pressed, execute switch image logic
            int currentIndex = ThumbnailListView.SelectedIndex;
            
            // If no item is selected, do not switch
            if (currentIndex == -1) return;
            
            // e.Delta > 0 means wheel scrolls up (previous image)
            if (e.Delta > 0)
            {
                if (currentIndex > 0)
                {
                    ThumbnailListView.SelectedIndex = currentIndex - 1;
                    ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
                }
            }
            // e.Delta < 0 means wheel scrolls down (next image)
            else if (e.Delta < 0)
            {
                if (currentIndex >= 0 && currentIndex < ImageItems.Count - 1)
                {
                    ThumbnailListView.SelectedIndex = currentIndex + 1;
                    ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
                }
            }
            
            // Prevent event from bubbling
            e.Handled = true;
        }

        // The bottom thumbnail area also supports wheel switching previous/next
        private void ThumbnailListView_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (ImageItems == null || ImageItems.Count == 0) return;

            int currentIndex = ThumbnailListView.SelectedIndex;
            if (currentIndex == -1) return;

            if (e.Delta > 0)
            {
                if (currentIndex > 0)
                {
                    ThumbnailListView.SelectedIndex = currentIndex - 1;
                    ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
                }
            }
            else if (e.Delta < 0)
            {
                if (currentIndex < ImageItems.Count - 1)
                {
                    ThumbnailListView.SelectedIndex = currentIndex + 1;
                    ThumbnailListView.ScrollIntoView(ThumbnailListView.SelectedItem);
                }
            }
            e.Handled = true;
        }

        // Middle mouse button pressed, start panning
        private void MainImageScrollViewer_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (_isFullScreen) return; // Disable panning in full screen

            if (e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                _panStartPoint = e.GetPosition(this); // Get absolute coordinates relative to the window
                _scrollStartOffset = new Point(MainImageScrollViewer.HorizontalOffset, MainImageScrollViewer.VerticalOffset);
                MainImageScrollViewer.CaptureMouse();
                
                // Show Minimap
                _minimapHideTimer.Stop();
                System.Windows.Media.Animation.DoubleAnimation fadeIn = new System.Windows.Media.Animation.DoubleAnimation(1, TimeSpan.FromMilliseconds(150));
                MinimapBorder.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            }
        }

        // Mouse move, perform image panning
        private void MainImageScrollViewer_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MainImageScrollViewer.IsMouseCaptured && e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(this);
                // Calculate panning distance
                double deltaX = currentPoint.X - _panStartPoint.X;
                double deltaY = currentPoint.Y - _panStartPoint.Y;
                
                // Update scrollbar position (inverse translation)
                MainImageScrollViewer.ScrollToHorizontalOffset(_scrollStartOffset.X - deltaX);
                MainImageScrollViewer.ScrollToVerticalOffset(_scrollStartOffset.Y - deltaY);
            }
        }

        // Middle mouse button released, end panning and restart countdown to hide minimap
        private void MainImageScrollViewer_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (MainImageScrollViewer.IsMouseCaptured && e.MiddleButton == System.Windows.Input.MouseButtonState.Released)
            {
                MainImageScrollViewer.ReleaseMouseCapture();
                
                // Start hide timer, fade out after 1 second
                _minimapHideTimer.Start();
            }
        }

        // When scrollbar scrolls or image scales (Extent changes), update the focus box position and size on the minimap
        private void MainImageScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (MinimapImage.Source == null || MainImageScrollViewer.ExtentWidth == 0 || MainImageScrollViewer.ExtentHeight == 0) return;

            // Get the actual display size of the image currently presented by the Minimap (because Stretch="Uniform" is used and centered)
            double mapW = MinimapImage.ActualWidth;
            double mapH = MinimapImage.ActualHeight;
            
            if (mapW == 0 || mapH == 0) return;

            double extentW = MainImageScrollViewer.ExtentWidth;
            double extentH = MainImageScrollViewer.ExtentHeight;
            double viewportW = MainImageScrollViewer.ViewportWidth;
            double viewportH = MainImageScrollViewer.ViewportHeight;
            double offsetX = MainImageScrollViewer.HorizontalOffset;
            double offsetY = MainImageScrollViewer.VerticalOffset;

            // Ratio calculation (scaling ratio of the image on the Minimap relative to the full content of the ScrollViewer)
            double scaleMapX = mapW / extentW;
            double scaleMapY = mapH / extentH;

            // Calculate the size of the current visual range on the Minimap (avoid being larger than the total size of the thumbnail)
            MinimapViewportRect.Width = Math.Min(viewportW * scaleMapX, mapW);
            MinimapViewportRect.Height = Math.Min(viewportH * scaleMapY, mapH);
            
            // Calculate the position offset of the current visual range on the Minimap
            Canvas.SetLeft(MinimapViewportRect, offsetX * scaleMapX);
            Canvas.SetTop(MinimapViewportRect, offsetY * scaleMapY);
        }

        // Detect left-click double-click to enter and exit full screen
        private void MainImageScrollViewer_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleFullScreen();
                e.Handled = true;
            }
        }

        // Toggle full screen / normal mode
        private void ToggleFullScreen()
        {
            _isFullScreen = !_isFullScreen;
            if (_isFullScreen)
            {
                // Save current splitter sizes in case user had dragged them
                _savedLeftColWidth = LeftColDef.Width;
                _savedBottomRowHeight = BottomRowDef.Height;

                // Full screen: Remove borders, maximize, hide surrounding elements
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;

                TopMenuBar.Visibility = Visibility.Collapsed;
                BottomStatusBar.Visibility = Visibility.Collapsed;
                LeftPanelBorder.Visibility = Visibility.Collapsed;
                VerticalSplitter.Visibility = Visibility.Collapsed;
                HorizontalSplitter.Visibility = Visibility.Collapsed;
                BottomPanelBorder.Visibility = Visibility.Collapsed;

                // Force row and column sizes of other Grids to 0 so the central large image fills the entire screen edge
                LeftColDef.Width = new GridLength(0);
                SplitterColDef.Width = new GridLength(0);
                SplitterRowDef.Height = new GridLength(0);
                BottomRowDef.Height = new GridLength(0);

                // Remove large image area margins and rounded corners for the most immersive full-screen experience
                MainImageGrid.Margin = new Thickness(0);
                MainImageBorder.CornerRadius = new CornerRadius(0);

                // Reset image zoom to fit window size
                if (MainImageScale != null)
                {
                    MainImageScale.ScaleX = 1;
                    MainImageScale.ScaleY = 1;
                }
            }
            else
            {
                // Restore to original state
                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;

                TopMenuBar.Visibility = Visibility.Visible;
                BottomStatusBar.Visibility = Visibility.Visible;
                LeftPanelBorder.Visibility = Visibility.Visible;
                VerticalSplitter.Visibility = Visibility.Visible;
                HorizontalSplitter.Visibility = Visibility.Visible;
                BottomPanelBorder.Visibility = Visibility.Visible;

                // Restore original splitter sizes
                LeftColDef.Width = _savedLeftColWidth;
                SplitterColDef.Width = new GridLength(5);
                SplitterRowDef.Height = new GridLength(5);
                BottomRowDef.Height = _savedBottomRowHeight;

                // Restore large image area margins and rounded corners
                MainImageGrid.Margin = new Thickness(10);
                MainImageBorder.CornerRadius = new CornerRadius(8);
            }
        }

        // Menu Event: File Format Filter Settings
        private void MenuFormatFilter_Click(object sender, RoutedEventArgs e)
        {
            FilterWindow filterWindow = new FilterWindow();
            filterWindow.Owner = this;
            if (filterWindow.ShowDialog() == true)
            {
                // After user clicks apply, reload current folder
                TreeViewItem? selectedItem = FolderTreeView.SelectedItem as TreeViewItem;
                if (selectedItem?.Tag is string tagPath)
                {
                    LoadImagesFromFolder(tagPath);
                }
            }
        }

        // Menu Event: Exit
        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // Menu Event: About
        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }
    }

    // Simple data structure that binds image path and file name together
    public class ImageItem
    {
        public string ImagePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public System.Windows.Media.ImageSource? Thumbnail { get; set; }
    }
}