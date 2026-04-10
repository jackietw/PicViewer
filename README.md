# PicViewer

[English](#english) | [中文](#中文)

---

<a name="english"></a>

## English

**PicViewer** is a modern, lightweight, and feature-rich image viewer built with C# and WPF (.NET 10). It provides a clean, user-friendly interface with advanced features for browsing and viewing images locally.

### Features

- **Wide Format Support**: Supports popular image formats including JPG, JPEG, PNG, BMP, GIF, TIFF, ICO, and SVG.
- **Explorer Integration**: Open images directly from Windows Explorer. Launches in an immersive full-screen preview mode.
- **Full-Screen Mode**: Toggle full-screen mode by double-clicking the image. Press `Esc` to exit full-screen mode or close the app if opened directly from Explorer.
- **Smart Navigation**:
  - **Pan**: Press and hold the middle mouse button to pan across the image.
  - **Zoom**: Use `Ctrl + Mouse Wheel` to zoom in and out.
  - **Switch Images**: Scroll the mouse wheel (without Ctrl) to quickly jump to the previous or next image in the directory.
- **Minimap Preview**: A minimap appears during panning and zooming, showing the current viewport relative to the whole image.
- **Tree View & Quick Access**: Easily navigate through your PC's local drives and quick access folders (Desktop, Downloads, Pictures, etc.).
- **USB Hot-Plug Support**: Automatically detects and refreshes the tree view when USB drives are inserted or removed.
- **Multi-Language Support**: Supports English, Chinese, and more via external `.ini` language files.
- **Customizable Filters**: Filter which image formats are displayed in the current folder.
- **State Memory**: Automatically remembers your window size, position, and panel layout configurations for your next session.

### Requirements

- Windows OS
- .NET 10.0 Desktop Runtime

### Building from Source

1. Clone the repository.
2. Open `PicViewer.sln` in Visual Studio 2022.
3. Build the solution.

---

<a name="正體中文"></a>

## 中文

**PicViewer** 是一款使用 C# 和 WPF (.NET 10) 開發的現代化、輕量級且功能豐富的圖片檢視器。它提供簡潔、用戶友好的介面，以及在本地瀏覽和檢視圖片的進階功能。

### 核心功能

- **廣泛的格式支援**：支援常見的圖片格式，包括 JPG、JPEG、PNG、BMP、GIF、TIFF、ICO 和 SVG。
- **檔案總管整合**：直接從 Windows 檔案總管開啟圖片，並以沉浸式全螢幕預覽模式啟動。
- **全螢幕模式**：連按兩下圖片可切換全螢幕模式。按下 `Esc` 鍵可退出全螢幕，若直接從檔案總管開啟圖片，按 `Esc` 則會直接關閉程式。
- **智慧導覽與操控**：
  - **平移**：按住滑鼠中鍵以平移圖片。
  - **縮放**：使用 `Ctrl + 滑鼠滾輪` 進行放大與縮小。
  - **切換圖片**：直接滾動滑鼠滾輪可快速切換至目錄中的上一張或下一張圖片。
- **迷你地圖 (Minimap)**：在平移或縮放時會顯示迷你預覽圖，標示當前在整張圖片中的可視範圍。
- **樹狀目錄與快速存取**：可輕鬆瀏覽電腦的本地磁碟機及快速存取資料夾（桌面、下載、圖片等）。
- **USB 隨插即用支援**：插入或移除 USB 隨身碟時，能自動偵測並重新整理樹狀目錄。
- **多國語言支援**：透過外部 `.ini` 語言檔提供英文、中文等多語言切換。
- **格式過濾器**：可自訂在資料夾中要顯示的圖片格式。
- **狀態記憶**：自動記憶應用程式的視窗大小、位置以及面板佈局配置，方便下次使用。

### 系統需求

- Windows 作業系統
- .NET 10.0 桌面執行階段

### 建置與編譯

1. 複製此儲存庫 (Clone)。
2. 在 Visual Studio 2022 中開啟 `PicViewer.sln`。
3. 建置 (Build) 解決方案。
