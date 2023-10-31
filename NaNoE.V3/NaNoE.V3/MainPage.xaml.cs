using NaNoE.V2.Data;
using NaNoE.V3.Interact;

namespace NaNoE.V3;

public partial class MainPage : ContentPage
{
    private ViewModelLocator _vml;
    private Navigator _nav;
    public MainPage()
    {
        InitializeComponent();

        _vml = new();
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            _nav = new(frmContent, Interact.Platform.Android);
            frmContent.Margin = new Thickness(-10);
        }
        else // Default / Windows
        {
            _nav = new(frmContent, Interact.Platform.Windows);
        }

        // TODO: look up info on this: await Navigation.PushAsync(new HelloFromAndroid());
        _nav.GoTo(Loc.Main);
    }
}