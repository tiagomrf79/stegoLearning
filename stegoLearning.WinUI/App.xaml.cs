using Microsoft.UI.Xaml;
using stegoLearning.WinUI.comum;
using stegoLearning.WinUI.UI;
using System;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace stegoLearning.WinUI
{
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
            //UnhandledException += OnUnhandledException;
            this.InitializeComponent();
            //Application.DispatcherUnhandledException += (sender, args) => this.logger.Error(args.Exception.Message);
            //UnhandledException += (sender, e) => e.Handled = true;
            App.Current.UnhandledException += OnUnhandledException;
        }

        void OnUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            ErrosLog.EscreverErroEmLog(e.Exception);
            e.Handled = true;
        }

        //private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        //{
        //    MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    e.Handled = true;
        //}
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

        public static MenuWindow appWindow { get; private set; }
        public static IntPtr appWindowHandle { get; private set; }
    }
}
