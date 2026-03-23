using System.Configuration;
using System.Data;
using System.Windows;

namespace PicViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppSettings.Load();
        LanguageManager.InitializeDefaultLanguage();

        string langPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Languages", AppSettings.LanguageFile);
        if (System.IO.File.Exists(langPath))
        {
            LanguageManager.ApplyLanguage(langPath);
        }
    }
}

