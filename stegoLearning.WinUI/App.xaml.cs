using Microsoft.UI.Xaml;
using stegoLearning.WinUI.UI;
using System;

namespace stegoLearning.WinUI;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object.  This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Invoked when the application is launched normally by the end user.  Other entry points
    /// will be used such as when the application is launched to open a specific file.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        appWindow = new MenuWindow();
        appWindow.Activate();
        appWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(appWindow);
    }

    //é necessária window handle para abrir caixas de diálogo para abrir e guardar ficheiros
    public static MenuWindow appWindow { get; private set; }
    public static IntPtr appWindowHandle { get; private set; }
}
