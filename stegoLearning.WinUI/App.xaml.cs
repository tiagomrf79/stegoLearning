using Microsoft.UI.Xaml;
using stegoLearning.WinUI.Componentes;
using stegoLearning.WinUI.UI;
using System;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace stegoLearning.WinUI;

public partial class App : Application
{
    //é necessária window handle para abrir caixas de diálogo para abrir e guardar ficheiros
    public static MenuWindow appWindow { get; private set; }
    public static IntPtr appWindowHandle { get; private set; }

    public App()
    {
        this.InitializeComponent();
        
        //criar event handler para excepções não tratadas
        App.Current.UnhandledException += OnUnhandledExceptionAsync;
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        appWindow = new MenuWindow();
        appWindow.Activate();
        appWindowHandle = WinRT.Interop.WindowNative.GetWindowHandle(appWindow);
    }

    async void OnUnhandledExceptionAsync(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        //tratar todas as excepções não previstas
        var messageDialog = new MessageDialog("Ocorreu um erro e a operação terminou inesperadamente. " +
            "Tente novamente e reinicie ou reinstale a aplicação caso o erro persista.", "ERRO");
        WinRT.Interop.InitializeWithWindow.Initialize(messageDialog, App.appWindowHandle);
        await messageDialog.ShowAsync();

        ErrosLog.EscreverErroEmLog(e.Exception);
        e.Handled = true;
    }

}
