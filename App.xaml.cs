using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PicViewer;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private Mutex? _mutex;
    private const string MutexName = "PicViewer_SingleInstance_Mutex";
    private const string PipeName = "PicViewer_NamedPipe";

    protected override void OnStartup(StartupEventArgs e)
    {
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Instance already running. Send args via Named Pipe.
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                try
                {
                    using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                    {
                        client.Connect(1000); // Wait up to 1 second
                        using (var writer = new StreamWriter(client))
                        {
                            writer.WriteLine(args[1]);
                            writer.Flush();
                        }
                    }
                }
                catch { }
            }
            // Exit this instance
            Application.Current.Shutdown();
            return;
        }

        // We are the first instance, start listening
        StartNamedPipeServer();

        base.OnStartup(e);
        AppSettings.Load();
        LanguageManager.InitializeDefaultLanguage();

        string langPath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Languages", AppSettings.LanguageFile);
        if (System.IO.File.Exists(langPath))
        {
            LanguageManager.ApplyLanguage(langPath);
        }
    }

    private void StartNamedPipeServer()
    {
        Task.Run(() =>
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
                    {
                        server.WaitForConnection();
                        using (var reader = new StreamReader(server))
                        {
                            string? filePath = reader.ReadLine();
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                // Dispatch to main window
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    if (Application.Current.MainWindow is PicViewer.MainWindow mainWindow)
                                    {
                                        mainWindow.OpenFileFromOtherInstance(filePath);
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore pipe errors to keep server running
                }
            }
        });
    }
}

