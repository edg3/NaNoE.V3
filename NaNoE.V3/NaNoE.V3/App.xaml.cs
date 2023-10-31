using NaNoE.V2.Data;


namespace NaNoE.V3;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        Window window = base.CreateWindow(activationState);
#if ANDROID
        window.Stopped += (s, e) =>
        {
            DataConnection.Instance.Close();
            App.Current.Quit();
        };
#elif WINDOWS
        window.Created += (s, e) =>
        {
            var handle = WinRT.Interop.WindowNative.GetWindowHandle(window.Handler.PlatformView);
            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(handle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(id);
            appWindow.Closing += async (s, e) =>
            {
                e.Cancel = true;
                DataConnection.Instance.Close();
                App.Current.Quit();
            };
        };
#endif
        return window;
    }
}